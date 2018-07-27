using FluentAssertions;
using NUnit.Framework;

namespace Umbraco.Tests.Selenium.PageTests.Installer
{
    [TestFixture]
    public class InstallerTests:SeleniumTestsBase
    {
        [Test]
        public void When_Navigating_To_Site_Root_Expect_Installer_Page()
        {
            //todo as part of setup we need to ensure no db is setup so that we get the installer page
            var installPage = Host.NavigateToInitialPage<InstallPage>();

            installPage.Title.Should().Contain("Install Umbraco");
        }
        
    }
}
