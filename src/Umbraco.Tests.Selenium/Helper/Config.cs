using System.Configuration;
using Castle.Components.DictionaryAdapter;

namespace Umbraco.Tests.Selenium.Helper
{
    public static class Config
    {
        public static IConfig Settings;

        static Config()
        {
            var factory = new DictionaryAdapterFactory();
            //using castle windsor dictionary adaptor maps app settings to interface                
            Settings = factory.GetAdapter<IConfig>(ConfigurationManager.AppSettings);
        }
    }
}
