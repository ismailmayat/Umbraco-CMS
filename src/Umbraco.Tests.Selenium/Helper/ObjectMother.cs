using Umbraco.Tests.Selenium.PageTests.Installer;
using Umbraco.Tests.Selenium.PageTests.Installer.Models;

namespace Umbraco.Tests.Selenium.Helper
{
    public static class ObjectMother
    {
        public static InstallerModel CreateInstallerModel()
        {
            var installerModel = new InstallerModel
            {
                Name = "admin",
                Email = "admin@admin.com",
                Password = "1234567890"
            };

            return installerModel;

        }
    }
}
