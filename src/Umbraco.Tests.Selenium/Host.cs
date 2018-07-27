using Castle.Core.Logging;
using OpenQA.Selenium.Chrome;
using TestStack.Seleno.Configuration;
using TestStack.Seleno.Configuration.WebServers;
using Umbraco.Tests.Selenium.Helper;

namespace Umbraco.Tests.Selenium
{

    public class Host
    {
        public static readonly SelenoHost Instance = new SelenoHost();

        static Host()
        {

            var config = Config.Settings;

            string siteInstanceUrl = config.LocalSiteUrl + ":" + Config.Settings.PortNo;

            Instance.Run(configure => configure
                .WithWebServer(new InternetWebServer(siteInstanceUrl))
                .WithRemoteWebDriver(GetChrome)
                .UsingLoggerFactory(new ConsoleFactory()));

        }

        private static ChromeDriver GetChrome()
        {
            return new ChromeDriver();
        }
         
    }

}
