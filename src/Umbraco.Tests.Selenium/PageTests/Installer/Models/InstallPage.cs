using TestStack.Seleno.PageObjects;
using TestStack.Seleno.PageObjects.Locators;

namespace Umbraco.Tests.Selenium.PageTests.Installer.Models
{
    public class InstallPage : Page
    {
        public InstallPage InstallUmbraco(InstallerModel installerModel)
        {
            Find.Element(By.Name("name"))
                .SendKeys(installerModel.Name);

            Find.Element(By.Name("email"))
                .SendKeys(installerModel.Email);

            Find.Element(By.Name("installer.current.model.password"))
                .SendKeys(installerModel.Password);

            //todo after submitting the db etc is created but then a js redirect happens
            //we need to be able to tap into that to get the backoffice page atm we still have the install page
            return Navigate.To<InstallPage>(By.CssSelector("input[type='submit']"));

        }
    }
}
