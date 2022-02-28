
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;




ChromeDriver driver;


if (!args.Any())
    return;

var options = new ChromeOptions();
options.AddArgument("--log-level=3");
options.DebuggerAddress = "localhost:9222";

using (driver = new ChromeDriver(options))
{

    var retVal = UserFollower.GetFollowStatus(driver, args.First());
    Environment.Exit((int)retVal);
}




public static class UserFollower
{

    public enum FollowResult
    {
        Followed,
        Already_Following,
        Request_in_progess,
        Error,
        Unavailable,
    }

    public enum UnfollowResult
    {
        Unfollowed,
        Already_Unfollowed,
        Error,
        Unavailable,
        Pending,
    }

    public enum FollowStatus
    {
        Following,
        Not_Following,
        Pending,
        Unavailable
    }

    public static void GoToUserPage(ChromeDriver driver, string user)
    {
        if(!driver.Url.Contains("www.instagram.com"))
            driver.Navigate().GoToUrl("https://www.instagram.com/");

        if (driver.Url == user)
            return;


        if (driver.FindElements(By.XPath("/html/body/div[1]/section/nav/div[2]/div/div/div[2]/input[@placeholder='Cerca']")).Any())
        {

            var textBox = driver.FindElement(By.XPath("/html/body/div[1]/section/nav/div[2]/div/div/div[2]/input[@placeholder='Cerca']"));
            string s = GetUsernameFromURL(user);

            Actions action = new Actions(driver);
            action.MoveToElement(textBox);
            action.Click().Build().Perform();

            for (int i = 0; i < s.Length; i++)
            {
                action.Reset();
                action.SendKeys(s[i].ToString());
                action.Build().Perform();

                Thread.Sleep(Random.Shared.Next(100, 500));
            }

            if (driver.FindElements(By.XPath("/html/body/div[1]/section/nav/div[2]/div/div/div[2]/div[3]/div/div[2]/div/div[1]")).Any())
            {
                var userSearched = driver.FindElement(By.XPath("/html/body/div[1]/section/nav/div[2]/div/div/div[2]/div[3]/div/div[2]/div/div[1]"));

                Actions action2 = new Actions(driver);
                action2.MoveToElement(userSearched);
                action2.Click().Build().Perform();

                return;
            }


        }

        driver.Navigate().GoToUrl(user);
        

    }

    public static string GetUsernameFromURL(string user)
    {
        if (user.IndexOf("instagram.com") == -1)
            throw new InvalidDataException();

        string s = user.Substring(user.IndexOf("instagram.com") + 14);
        s = s.Replace("/", "");
        return s;
    }

    public static FollowResult FollowUser(ChromeDriver driver, string user)
    {

        GoToUserPage(driver, user);

        Thread.Sleep(1000);

        if (driver.FindElements(By.CssSelector("._7UhW9.xLCgt.qyrsm.uL8Hv.T0kll")).Any())
        {
            IWebElement follow_button = driver.FindElement(By.CssSelector("._7UhW9.xLCgt.qyrsm.uL8Hv.T0kll"));
            string status = follow_button.Text;


            switch (status)
            {
                case "Segui":
                    Actions action = new Actions(driver);
                    action.MoveToElement(follow_button);
                    action.Click().Build().Perform();
                    return (FollowResult.Followed);
                    break;

                case "":
                    if (driver.FindElements(By.CssSelector(".sqdOP.L3NKy._8A5w5")).Any() && driver.FindElement(By.CssSelector(".sqdOP.L3NKy._8A5w5")).Text == "Messaggio")
                        return (FollowResult.Already_Following);
                    else
                        return (FollowResult.Error);
                    break;
            }

        }

        return (FollowResult.Unavailable);

    }

    public static FollowStatus GetFollowStatus(ChromeDriver driver, string user)
    {

        GoToUserPage(driver, user);

        Thread.Sleep(1000);

        if (driver.FindElements(By.CssSelector("._7UhW9.xLCgt.qyrsm.uL8Hv.T0kll")).Any())
        {
            var button = driver.FindElement(By.CssSelector("._7UhW9.xLCgt.qyrsm.uL8Hv.T0kll"));
            string s = button.Text;

            

            switch (button.Text)
            {
                case "Segui":
                    return (FollowStatus.Not_Following);

                case "Richiesta effettuata":
                    return (FollowStatus.Pending);


            }

        }

        if (driver.FindElements(By.XPath("/html/body/div[1]/section/main/div/header/section/div[1]/div[1]/div/div[1]/button")).Any())
        {
            var messageButton = driver.FindElement(By.XPath("/html/body/div[1]/section/main/div/header/section/div[1]/div[1]/div/div[1]/button"));
            
            if(messageButton.Text == "Messaggio")
            {
                return (FollowStatus.Following);
            }
            
        }

        return (FollowStatus.Unavailable);
    }

    public static UnfollowResult UnfollowUser(ChromeDriver driver, string user)
    {
        //driver.Navigate().GoToUrl(user);

        GoToUserPage(driver, user);


        if (driver.FindElements(By.CssSelector("._5f5mN.-fzfL._6VtSN.yZn4P")).Any())
        {
            var unfollowButton = driver.FindElement(By.CssSelector("._5f5mN.-fzfL._6VtSN.yZn4P"));

            Actions action = new Actions(driver);
            action.MoveToElement(unfollowButton);
            action.Click().Build().Perform();

            Thread.Sleep(Random.Shared.Next(2000, 4000));

            if (driver.FindElements(By.CssSelector(".aOOlW.-Cab_")).Any())
            {
                var confirmUnfollow = driver.FindElement(By.CssSelector(".aOOlW.-Cab_"));

                Actions action2 = new Actions(driver);
                action2.MoveToElement(confirmUnfollow);
                action2.Click().Build().Perform();

                return (UnfollowResult.Unfollowed);

            }
            else
            {
                return (UnfollowResult.Unavailable);
            }
        }




        return (UnfollowResult.Unavailable);

    }
}

