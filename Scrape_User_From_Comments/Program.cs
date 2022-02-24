using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Scrape_User_From_Comments
{
    internal class Program
    {
        static ChromeDriver driver;

        static List<string> comments = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Current DIR : " + Environment.CurrentDirectory);

            var options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9222";

            using (driver = new ChromeDriver(options))
            {

                driver.Navigate().GoToUrl("https://www.tiktok.com/");

                Console.WriteLine("In attesa di CMD");

                var key = Console.ReadKey(true).Key;

                while (key != ConsoleKey.Escape)
                {
                    switch (key)
                    {
                        case ConsoleKey.A:
                            comments.AddRange(Scrape_from_current_tiktok());
                            Console.WriteLine("SCRAPE TERMINATO");
                            break;
                        case ConsoleKey.D:
                            Dump_Comments_to_db(comments);
                            Console.WriteLine("DUMP OK");
                            break;

                        default:
                            Console.WriteLine("CMD Non corretto");
                            break;

                    }

                    key = Console.ReadKey(true).Key;
                }



            }
        }

        static void Dump_Comments_to_db(IEnumerable<string> comments)
        {

            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder();
            sb.DataSource = @".\TikTok_db.db";
            sb.Version = 3;

            using (SQLiteConnection conn = new SQLiteConnection(sb.ConnectionString))
            {

                conn.Open();

                foreach (var comment in comments)
                {

                    SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = $"INSERT INTO ScrapedUsers VALUES ('{comment}',FALSE,NULL)";

                    try
                    {
                        cmd.ExecuteNonQuery();

                    }
                    catch (SQLiteException ex)
                    {
                        if (ex.ErrorCode == 19)
                            Console.WriteLine($"L'utente {comment} è già stato inserito");
                        else throw ex;
                    }

                }



            }
        }

        static IEnumerable<string> Scrape_from_current_tiktok()
        {
            if (driver == null)
                yield break;

            var div_elements = driver.FindElements(By.CssSelector(".tiktok-1rua9e7-StyledUserLinkName.evpz7zo4"));

            foreach (var element in div_elements)
            {
                string href = element.GetDomProperty("href");
                href = href.Replace("?lang=en", "");
                Console.WriteLine($"AGGIUNTO: {href}");
                yield return href;
            }


        }
    }
}
