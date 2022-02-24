using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading;

namespace Like_TikTok_User_Videos
{

    internal class Program
    {

        static Random r = new Random();
        private static int? navbarX_Position;
        private static int? navbarY_Position;

        static void Main(string[] args)
        {

            if (navbarX_Position == null || navbarY_Position == null)
            {
                setNavBarPosition();
            }

            List<string> scraped = new List<string>(Get_Scraped_From_DB());

            scraped.Sort();

            var options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9222";


            using (var driver = new ChromeDriver(options))
            {

                foreach (var item in scraped)
                {
                    driver.Navigate().GoToUrl(item);

                    Console.WriteLine("Sleep USER");
                    Thread.Sleep(TimeSpan.FromSeconds( r.Next(10, 16)));


                    var elements = driver.FindElements(By.CssSelector(".jsx-969240130.video-card-mask"));

                    int indexMin = 0;
                    int indexMax = elements.Count;

                    var rand1 = r.Next(indexMin, indexMax);
                    var rand2 = r.Next(indexMin, indexMax);

                    Console.WriteLine($"Like {rand1},{rand2} su {elements.Count} elem.");

                    Process p = new Process();
                    p.StartInfo.FileName = @".\Perform_Windows_Click.exe";

                    if (elements.Count > 0)
                        Console.WriteLine(elements[0].Location.Y);
                    else
                        continue;



                    for (int i = 0; i < elements.Count; i++)
                    {

                        if (i == rand1 || i == rand2)
                        {
                            Console.WriteLine("Sleep INDEX " + i);

                            Thread.Sleep(TimeSpan.FromSeconds(r.Next(8, 15)));


                            p.StartInfo.Arguments = $"--write l";
                            p.Start();
                            p.WaitForExit();

                        }

                        Thread.Sleep(TimeSpan.FromSeconds(r.Next(2, 5)));


                        p.StartInfo.Arguments = "--write {DOWN}";
                        p.Start();
                        p.WaitForExit();

                    }


                    

                }




            }

        }

        private static IEnumerable<string> Get_Scraped_From_DB()
        {
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder();
            sb.DataSource = @"C:\Users\Brd\source\repos\TikTok_Tools\TikTok_db.db";
            sb.Version = 3;

            using (SQLiteConnection conn = new SQLiteConnection(sb.ConnectionString))
            {

                conn.Open();


                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM ScrapedUsers";

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
    }

}
