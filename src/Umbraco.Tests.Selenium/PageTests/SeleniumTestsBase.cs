using NUnit.Framework;
using TestStack.Seleno.Configuration;
using TestStack.Seleno.Configuration.WebServers;
using Umbraco.Tests.Selenium.Helper;

namespace Umbraco.Tests.Selenium.PageTests
{
    [SetUpFixture]
    public class SeleniumTestsBase
    {
        protected SelenoHost Host;
        protected static IisExpressWebServer WebServer;

        [SetUp]
        public void SetUp()
        {
            var app = new WebApplication(ProjectLocation.FromFolder(Constants.ApplicationNameForIISExpress), Config.Settings.PortNo);
            WebServer = new IisExpressWebServer(app);
            WebServer.Start();
            Host = Selenium.Host.Instance;
        }


        [TearDown]
        public void TearDown()
        {
            Host.Dispose();
            WebServer.Stop();
        }
    }
}
