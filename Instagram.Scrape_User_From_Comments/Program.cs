using MySql.Data.MySqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;

using static UserFollower;

namespace Scrape_User_From_Comments
{
    internal class Program
    {
        static ChromeDriver driver;

        static List<string> comments = new List<string>();
        private static string welcomeMessage = @"
█ █▀▀   █▄▄ █▀█ ▀█▀   █▄▄ █▄█   █▄▄ █▀█ █▀▄ █▀█ █░█
█ █▄█   █▄█ █▄█ ░█░   █▄█ ░█░   █▄█ █▀▄ █▄▀ ▀▀█ ▀▀█ e BLT94";


        private static string InstagramURL = null;

        private static MySqlConnection conn = null;
        private static string connectionString = null;


        static void Main(string[] args)
        {

            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/b_settings.json";

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

            connectionString = config.GetSection("ConnectionStrings")["MySQL"];
            InstagramURL = config.GetSection("Instagram")["InstagramURL"];

            Console.WriteLine("Config PATH : " + configPath);

            var options = new ChromeOptions();
            options.AddArgument("--log-level=3");
            options.DebuggerAddress = "localhost:9222";


            Console.WriteLine("Connetto DB.....");
            conn = new MySqlConnection(connectionString);
            conn.Open();
            Console.WriteLine("Connesso");

            using (driver = new ChromeDriver(options))
            {

                Console.WriteLine("Carico Instagram.....", ConsoleColor.Green);

                driver.Navigate().GoToUrl("https://www.instagram.com/");




                while (true)
                {
                    try
                    {
                        PrintMenu();

                        var key = Console.ReadKey(false).Key;

                        ProcessCommand(key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        PrintConsoleColored("\n\n\n§ATTENZIONE ! Si è verificato un errore. " +
                            "Premere un tasto per riprendere l'esecuzione del programma.£");

                        Console.ReadKey(true);
                    }



                }



            }


            conn.Close();
            conn.Dispose();

        }

        private static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine(welcomeMessage);

            MySqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = $"SELECT COUNT(*) FROM ScrapedUsers WHERE Date_Followed IS NULL AND Is_Competitor = FALSE";
            var tofollow = cmd.ExecuteScalar();

            cmd.CommandText = $"SELECT COUNT(*) FROM ScrapedUsers WHERE Date_Followed <  DATE_ADD(CURRENT_DATE(), INTERVAL -5 DAY) AND Date_Unfollowed IS NULL AND Follow_Back = FALSE";
            var toUnfollow = cmd.ExecuteScalar();



            PrintConsoleColored("\n\n-> Premere O per aprire tutti i commenti" +
                $"\n-> Premere A per scraping dai commenti ^(da scaricare {comments.Count})£" +
                $"\n-> Premere G per scraping dai followers" +
                $"\n-> Premere D per dump su DB" +
                $"\n-> Premere S per seguire gli utenti acquisiti ^(ne restano {tofollow})£ " +
                $"\n-> Premere L per aggiornare la lista dei follow-back" +
                $"\n-> Premere R per stampare la lista dei follow-back" +
                $"\n-> Premere U per non seguire più chi non ha fatto follow-back ^(sono {toUnfollow})£");




            Console.Write("\n\n\nComando> ");
        }

        private static void PrintConsoleColored(string message)
        {
            for (int i = 0; i < message.Length; i++)
            {

                switch (message[i])
                {
                    case '^':
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        continue;

                    case '§':
                        Console.ForegroundColor = ConsoleColor.Red;
                        continue;                   


                    case '£':
                        Console.ResetColor();
                        continue;


                }

                Console.Write(message[i]);

            }
        }


        private static void ProcessCommand(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.O:
                    OpenAllComments();
                    Console.WriteLine("Tutti i commenti -dovrebbero- essere visibili");
                    break;

                case ConsoleKey.A:
                    comments.AddRange(Scrape_from_current_instagram());
                    Console.WriteLine("SCRAPING TERMINATO");
                    break;

                case ConsoleKey.G:
                    comments.AddRange(UpdateFollowers());
                    Console.WriteLine("SCRAPING TERMINATO");
                    break;

                case ConsoleKey.D:
                    Dump_Comments_to_db(comments);
                    comments.Clear();
                    Console.WriteLine("DUMP SU DB OK");
                    break;

                case ConsoleKey.Escape:
                case ConsoleKey.Q:
                    Console.WriteLine("\n\n CIAO");
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                    break;


                case ConsoleKey.S:
                    FollowAllUsersInDB();
                    break;

                case ConsoleKey.E:
                    throw new Exception("Errore di test");

                case ConsoleKey.L:

                    driver.Navigate().GoToUrl(InstagramURL);

                    var followers = UpdateFollowers();

                    foreach (var follower in followers)
                    {
                        MySqlCommand cmd = conn.CreateCommand();
                        cmd.CommandText = $"UPDATE ScrapedUsers SET Follow_Back = TRUE WHERE ID = '{follower}'";

                        cmd.ExecuteNonQuery();
                    }


                    Console.WriteLine("\nPremere un tasto per tornare al menu");
                    Console.ReadKey();
                    break;

                case ConsoleKey.R:
                    GetFollowBacks();

                    Console.WriteLine("\nPremere un tasto per tornare al menu");
                    Console.ReadKey();

                    break;

                case ConsoleKey.U:
                    UnfollowAllUsersInDB();

                    Console.WriteLine("\nPremere un tasto per tornare al menu");
                    Console.ReadKey();
                    break;

                default:
                    Console.WriteLine("CMD Non corretto");
                    break;

            }
        }

        private static void GetFollowBacks()
        {
            Console.WriteLine("\n\nFollow Backs: ");

            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM ScrapedUsers WHERE Follow_Back = TRUE";


            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string user = reader.GetString(0);
                    Console.WriteLine(user);

                }
            }

        }

        public static void ScrollTo(int xPosition = 0, int yPosition = 0)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(String.Format("window.scrollTo({0}, {1})", xPosition, yPosition));

        }



        public static void ScrollToView(IWebElement element)
        {
            if (element.Location.Y > 200)
            {
                ScrollTo(0, element.Location.Y - 100);
            }

        }



        private static List<string> UpdateFollowers()
        {
            var list = new List<string>();


            if (driver.FindElements(By.CssSelector("._7UhW9.vy6Bb.MMzan.KV-D4.uL8Hv.T0kll")).Any())
            {
                string totalFolStr = driver.FindElement(By.XPath("/html/body/div[1]/section/main/div/header/section/ul/li[2]")).Text.Replace(" follower", "");
                int totalFol = int.Parse(totalFolStr);

                var follower_a_tag = driver.FindElement(By.XPath("//a[contains(@href,'/followers')]"));

                follower_a_tag.Click();

                Thread.Sleep(2000);

                var scroll_element = driver.FindElement(By.XPath("//div[@class='isgrP']"));
                driver.ExecuteScript("return arguments[0].scrollIntoView();", scroll_element);

                string last_id = null;

                while (true)
                {

                    Thread.Sleep(2000);
                    var elements = scroll_element.FindElements(By.CssSelector(".notranslate._0imsa"));
                    var last_element = elements.Last();

                    driver.ExecuteScript("return arguments[0].scrollIntoView();", last_element);


                    if (last_element.ToString() == last_id)
                    {

                        foreach (var element in elements)
                        {
                            string t = element.GetAttribute("href");

                            list.Add(t);

                        }

                        Console.WriteLine("Totale followers : " + elements.Count);


                        break;
                    }
                    else
                        last_id = last_element.ToString();

                    while (scroll_element.FindElements(By.XPath("//div[@class='By4nA']")).Count > 0)
                    {
                        Thread.Sleep(1000);
                    }

                }


            }

            return list;
        }

        private static void UnfollowAllUsersInDB()
        {

            List<string> users = new List<string>();


            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM ScrapedUsers WHERE Date_Followed <  DATE_ADD(CURRENT_DATE(), INTERVAL -5 DAY) AND Date_Unfollowed IS NULL AND Follow_Back = FALSE";

            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine($"\n\nUTENTE                           DATA FOLLOW");

                while (reader.Read())
                {
                    string user = reader.GetString(0);
                    DateTime date = reader.GetDateTime(2);
                    Console.WriteLine($"{user.PadRight(50).Substring(20, 30)}   {date.ToString()}");
                    users.Add(user);

                }

            }



            foreach (var user in users)
            {


                FollowStatus retVal = UserFollower.GetFollowStatus(driver, user);

                switch (retVal)
                {
                    case FollowStatus.Following:

                        UserFollower.UnfollowUser(driver, user);

                        MySqlCommand cmd2 = conn.CreateCommand();
                        cmd2.CommandText = $"UPDATE ScrapedUsers SET  Date_Unfollowed=CURRENT_DATE() WHERE ID='{user}'";


                        Console.WriteLine($"Retval : {retVal} DBNQ : {cmd2.ExecuteNonQuery()} <- {user} UNFOLLOWED");

                        DoEvents(Random.Shared.Next(50000, 80000));

                        break;

                    case FollowStatus.Not_Following:

                        MySqlCommand cmd3 = conn.CreateCommand();
                        cmd3.CommandText = $"UPDATE ScrapedUsers SET Date_Unfollowed=FROM_UNIXTIME(0) WHERE ID='{user}'";

                        Console.WriteLine($"Retval : {retVal} DBNQ : {cmd3.ExecuteNonQuery()} <- {user} UNFOLLOWED");


                        break;

                    case FollowStatus.Pending:
                        Console.WriteLine($"L'utente {GetUsernameFromURL(user)} non ha ancora accettato la richiesta.");
                        break;



                }





            }
        }

        private static void FollowAllUsersInDB()
        {
            List<string> users = new List<string>();



            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM ScrapedUsers WHERE Date_Followed IS NULL AND Is_Competitor = FALSE";

            using (var reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    string user = reader.GetString(0);
                    users.Add(user);

                }

            }

            Console.WriteLine("\nTrovati " + users.Count + " utenti da seguire");

            foreach (var user in users)
            {

                if (Console.KeyAvailable)
                    return;

                Console.WriteLine($"Seguo {user}...");

                FollowResult retVal = UserFollower.FollowUser(driver, user);

                switch (retVal)
                {
                    case FollowResult.Followed: // Followed

                        MySqlCommand cmd2 = conn.CreateCommand();
                        cmd2.CommandText = $"UPDATE ScrapedUsers SET Processed_F_U = FALSE, Date_Followed=CURRENT_DATE() WHERE ID='{user}'";

                        if (cmd2.ExecuteNonQuery() == 1)
                            Console.WriteLine($"§ {user} FOLLOWED");

                        DoEvents(Random.Shared.Next(320 * 1000, 400 * 1000));
                        break;
                }





            }
        }

        static void DoEvents(int SleepFallbackTime)
        {

            DateTime end = DateTime.Now.AddMilliseconds(SleepFallbackTime);

            PrintMenu();

            while (end > DateTime.Now)
            {

                Console.Write($"\rSto attendendo {(int)(end - DateTime.Now).TotalSeconds} secondi. Premi un tasto per eseguire un'altra azione.");

                if (Console.KeyAvailable)
                {
                    ProcessCommand(Console.ReadKey(true).Key);
                    PrintMenu();
                }

                Thread.Sleep(1000);

            }
        }

        static void Dump_Comments_to_db(IEnumerable<string> comments)
        {

            foreach (var comment in comments)
            {

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"INSERT INTO ScrapedUsers(ID,Processed_F_U,Date_Followed) VALUES ('{comment}',FALSE,NULL)";

                try
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"↓ {comment}");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"x {comment} - Ragione : {ex.Message}");
                }

            }


        }

        static IEnumerable<string> Scrape_from_current_instagram()
        {
            if (driver == null)
                yield break;

            ReadOnlyCollection<IWebElement> elements = new ReadOnlyCollection<IWebElement>(new List<IWebElement>());


            elements = driver.FindElements(By.CssSelector(".sqdOP.yWX7d._8A5w5.ZIAjV"));

            List<IWebElement> div_elements = new List<IWebElement>(elements);

            int toSkip = 0;
            string prevHead = null;

            foreach (IWebElement element in div_elements)
            {
                string t = element.Text;
                Console.WriteLine(t);
                if (string.IsNullOrWhiteSpace(element.Text) || prevHead != element.Text)
                {
                    toSkip++;
                    prevHead = element.Text;
                }
                else
                    break;
            }

            if (toSkip <= div_elements.Count)
                div_elements = div_elements.Skip(toSkip).ToList();



            foreach (var element in div_elements)
            {
                string href = element.GetDomProperty("href");
                href = href.Replace("?lang=en", "");
                Console.WriteLine($"+ {href}");
                yield return href;
            }



        }

        static void OpenAllComments()
        {
            if (driver == null)
                return;

            Console.WriteLine();

            ConsoleKey? key = null;


            while (true)
            {
                if (Console.KeyAvailable)
                    return;

                if (driver.FindElements(By.XPath("/html/body/div[6]/div[3]/div/article/div/div[2]/div/div/div[2]/div[1]/ul/li/div/button")).Any())
                {
                    IWebElement plus_button = driver.FindElement(By.XPath("/html/body/div[6]/div[3]/div/article/div/div[2]/div/div/div[2]/div[1]/ul/li/div/button"));
                    Actions action = new Actions(driver);
                    action.MoveToElement(plus_button);
                    action.KeyDown(Keys.Space).KeyUp(Keys.Space).Click().Build().Perform();
                    Console.Write(".");
                    Thread.Sleep(5000);
                }
                else
                {
                    Console.WriteLine("Dovrei essere arrivato alla fine");
                    break;
                }

            }



        }
    }
}
