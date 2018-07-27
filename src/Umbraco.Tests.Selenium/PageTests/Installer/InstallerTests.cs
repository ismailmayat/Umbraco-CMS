using FluentAssertions;
using NUnit.Framework;
using TestStack.Seleno.Configuration;
using TestStack.Seleno.Configuration.WebServers;
using Umbraco.Tests.Selenium.Helper;

namespace Umbraco.Tests.Selenium.PageTests.Installer
{
    [TestFixture]
    public class InstallerTests
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

        [Test]
        public void When_Navigating_To_Site_Root_Expect_Installer_Page()
        {
            //todo as part of setup we need to ensure no db is setup so that we get the installer page
            var installPage = Host.NavigateToInitialPage<InstallPage>();

            installPage.Title.Should().Contain("Install Umbraco");

        }

    }
}
