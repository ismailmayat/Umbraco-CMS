using TestStack.Seleno.PageObjects;
using TestStack.Seleno.PageObjects.Locators;

namespace Umbraco.Tests.Selenium.PageTests.Installer.Models
{
    public class InstallPage : Page
    {
        public UmbracoBackOfficeContentPage InstallUmbraco(InstallerModel installerModel)
        {
            Find.Element(By.Name("name"))
                .SendKeys(installerModel.Name);

            Find.Element(By.Name("email"))
                .SendKeys(installerModel.Email);

            Find.Element(By.Name("installer.current.model.password"))
                .SendKeys(installerModel.Password);

            return Navigate.To<UmbracoBackOfficeContentPage>(By.CssSelector("input[type='submit']"));
        }
    }
}
