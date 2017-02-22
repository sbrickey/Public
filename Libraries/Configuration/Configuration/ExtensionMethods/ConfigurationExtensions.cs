namespace SBrickey.Libraries.Configuration.ExtensionMethods
{
    using System.Configuration;
    using System.Collections.Specialized;

    public static class ConfigurationExtensions
    {
        public static NameValueCollection AppSettings(this Configuration config)
        {
            // this is (more or less) the reflected code from ConfigurationManager.AppSettings
            var appSettingsSectionObj = config.GetSection("appSettings") as object;
            var appConfig = appSettingsSectionObj as NameValueCollection;
            return appConfig;
        }
    } // class
} // namespace