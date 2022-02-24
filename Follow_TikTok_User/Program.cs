using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Follow_TikTok_User
{
    internal class Program
    {
        static int? posX_Follow_Start = null;
        static int? posY_Follow_Start = null;
        static int? posX_Follow_End = null;
        static int? posY_Follow_End = null;
        static int? navbarX_Position = null;
        static int? navbarY_Position = null;



        static Random r = new Random();

        static Func<bool> valid_Follow = () =>
        {
            return posX_Follow_Start != null && posY_Follow_Start != null && posX_Follow_End != null && posY_Follow_End != null;
        };

        static void Main(string[] args)
        {
            IEnumerable<string> to_follow = new List<string>(Get_Scraped_From_DB());


            if (navbarX_Position == null || navbarY_Position == null)
            {
                setNavBarPosition();
            }


            if (!valid_Follow())
            {
                setFollowPosition();

            }

            var options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9222";


            using (var driver = new ChromeDriver(options))
            {

                foreach (var user in to_follow)
                {

                    var followed_last24h = Check_Followed_Last24H();

                    Console.Write($"\rSeguiti nelle ultime 24H : {followed_last24h} ");

                    if (followed_last24h >= 200)
                        break;



                    var rand = r.Next(30, 60);

                    //Task taks = Task.Factory.StartNew(() => Console.ReadKey(true));            

                    for (int i = rand; i >= 0; i--)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        Console.Write($"\rProssimo follow tra {i} secondi per l'utente {user.Replace("https://www.tiktok.com/@", "").PadRight(36).Substring(0,36)}. ");
                        Console.Write("Premere S per saltare");

                        if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.S)
                            break;
                    }


                    var result = Follow_User(user, driver);


                    Set_Followed_User(user, result);



                }
            }

            Console.ReadKey(true);

        }

        private static long Check_Followed_Last24H()
        {
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder();
            sb.DataSource = @".\TikTok_db.db";
            sb.Version = 3;

            using (SQLiteConnection conn = new SQLiteConnection(sb.ConnectionString))
            {

                conn.Open();


                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM ScrapedUsers WHERE Date_Followed > datetime('now','-1 days')";

                return (long)cmd.ExecuteScalar();


            }
        }

        private static void Set_Followed_User(string user, FollowResult result)
        {
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder();
            sb.DataSource = @".\TikTok_db.db";
            sb.Version = 3;

            using (SQLiteConnection conn = new SQLiteConnection(sb.ConnectionString))
            {

                conn.Open();


                var cmd = conn.CreateCommand();
                cmd.CommandText = $"UPDATE ScrapedUsers SET Processed={result.Processed}, Date_Followed={(result.Followed ? "datetime('now')" : "NULL") } WHERE ID='{user}'";

                cmd.ExecuteNonQuery();


            }
        }

        private static FollowResult Follow_User(string user, IWebDriver driver)
        {

            var rand = r.Next(4000, 6000);

            Thread.Sleep(rand);


            if (valid_Follow())
                Follow(user);

            try
            {
                return new FollowResult
                {
                    Processed = true,
                    Followed = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[2]/div/div[1]/div[1]/div[2]/div/div[1]/button")).Text != "Follow"
                };

            }
            catch
            {
                return new FollowResult
                {
                    Processed = true,
                    Followed = false
                };
            }



        }

        private static void setNavBarPosition()
        {
            Process p = new Process();
            p.StartInfo.FileName = @".\Perform_Windows_Click.exe";


            Console.WriteLine("Posizionarsi sulla navbar e premere il tasto R");

            if (Console.ReadKey(true).Key == ConsoleKey.R)
            {

                try
                {
                    p.StartInfo.Arguments = "--getX";
                    p.Start();
                    p.WaitForExit();
                    navbarX_Position = p.ExitCode;

                    p.StartInfo.Arguments = "--getY";
                    p.Start();
                    p.WaitForExit();
                    navbarY_Position = p.ExitCode;

                }
                finally
                {

                }

            }
        }

        private static void Follow(string user)
        {

            Process p = new Process();
            p.StartInfo.FileName = @".\Perform_Windows_Click.exe";


            p.StartInfo.Arguments = $"--setXY {navbarX_Position} {navbarY_Position}";
            p.Start();
            p.WaitForExit();

            p.StartInfo.Arguments = $"--performClick";
            p.Start();
            p.WaitForExit();

            p.StartInfo.Arguments = $"--write {user}\n";
            p.Start();
            p.WaitForExit();


            int x_rnd = r.Next(posX_Follow_Start.Value, posX_Follow_End.Value);
            int y_rnd = r.Next(posY_Follow_Start.Value, posY_Follow_End.Value);


            for (int i = navbarY_Position.Value; i < y_rnd; i++)
            {
                Console.Write($"\rSpostamento a {(int)(i / ((float)y_rnd / x_rnd))}:{i}");
                p.StartInfo.Arguments = $"--setXY {(int)(i / ((float)y_rnd / x_rnd))} {i}";
                p.Start();
                p.WaitForExit();

            }

            p.StartInfo.Arguments = $"--performClick";
            p.Start();
            p.WaitForExit();

        }

        private static void setFollowPosition()
        {
            Process p = new Process();
            p.StartInfo.FileName = @".\Perform_Windows_Click.exe";


            Console.WriteLine("Posizionarsi sull'angolo sinistro in alto del bottone SEGUI e premere il tasto R");

            if (Console.ReadKey(true).Key == ConsoleKey.R)
            {

                try
                {
                    p.StartInfo.Arguments = "--getX";
                    p.Start();
                    p.WaitForExit();
                    posX_Follow_Start = p.ExitCode;

                    p.StartInfo.Arguments = "--getY";
                    p.Start();
                    p.WaitForExit();
                    posY_Follow_Start = p.ExitCode;

                }
                finally
                {

                }

            }

            Console.WriteLine("Posizionarsi sull'angolo destro in basso del bottone SEGUI e premere il tasto R");


            if (Console.ReadKey(true).Key == ConsoleKey.R)
            {

                try
                {

                    p.StartInfo.Arguments = "--getX";
                    p.Start();
                    p.WaitForExit();
                    posX_Follow_End = p.ExitCode;

                    p.StartInfo.Arguments = "--getY";
                    p.Start();
                    p.WaitForExit();
                    posY_Follow_End = p.ExitCode;

                }
                finally
                {

                }

            }
        }

        private static IEnumerable<string> Get_Scraped_From_DB()
        {
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder();
            sb.DataSource = @".\TikTok_db.db";
            sb.Version = 3;

            using (SQLiteConnection conn = new SQLiteConnection(sb.ConnectionString))
            {

                conn.Open();


                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM ScrapedUsers WHERE Processed=FALSE";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string val = reader["ID"].ToString();
                        yield return val;
                    }
                }


            }
        }
    }
}
