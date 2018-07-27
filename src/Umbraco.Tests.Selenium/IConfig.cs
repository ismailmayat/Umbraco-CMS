namespace Umbraco.Tests.Selenium
{
    public interface IConfig
    {
        string LocalSiteUrl { get; set; }
        int PortNo { get; set; }
    }
}
