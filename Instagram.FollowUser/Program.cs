
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

    var retVal = UserFollower.FollowUser(driver, args.First());
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
    }

    public static FollowResult FollowUser(ChromeDriver driver, string user)
    {

        driver.Navigate().GoToUrl(user);


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

    public static UnfollowResult UnfollowUser(ChromeDriver driver, string user)
    {
        driver.Navigate().GoToUrl(user);

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

