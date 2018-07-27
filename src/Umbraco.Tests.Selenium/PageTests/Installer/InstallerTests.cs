using FluentAssertions;
using NUnit.Framework;
using TestStack.Seleno.Configuration;
using TestStack.Seleno.Configuration.WebServers;
using Umbraco.Tests.Selenium.Helper;
using Umbraco.Tests.Selenium.PageTests.Installer.Models;

namespace Umbraco.Tests.Selenium.PageTests.Installer
{
    /// <summary>
    /// todo extract out a base class for all the setup and teardown did try this but tests never run see file SeleniumTestsBase
    /// </summary>
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
            //could run Umbraco-Cms\Build\RevertToEmptyInstall.bat
            var installPage = Host.NavigateToInitialPage<InstallPage>();

            installPage.Title.Should().Contain("Install Umbraco");

        }

        [Test]
        public void Can_Install_New_Blank_Site_With_Default_Options()
        {
            var installPage = Host.NavigateToInitialPage<InstallPage>();

            var backOfficePage = installPage.InstallUmbraco(ObjectMother.CreateInstallerModel());

            backOfficePage.Title.Should().Contain("Content");

        }

    }
}
