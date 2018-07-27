using TestStack.Seleno.PageObjects;
using TestStack.Seleno.PageObjects.Locators;

namespace Umbraco.Tests.Selenium.PageTests.Installer
{
    public class InstallPage : Page
    {
        public InstallPage InstallUmbraco(InstallerModel installerModel)
        {
            Find.Element(By.Name("name"))
                .SendKeys(installerModel.Name);

            Find.Element(By.Name("email"))
                .SendKeys(installerModel.Email);

            Find.Element(By.Name("password"))
                .SendKeys(installerModel.Password);

            return Navigate.To<InstallPage>(By.CssSelector("input[type='submit']"));
        }
    }
}
