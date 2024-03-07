using System.Configuration;

namespace PAS.Common.Configurations
{
    public class WebConfigurations
    {
        public static string WebUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.AppSettingKeys.IDA_WebUrl];
            }
        }

        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["CoreContext"].ConnectionString; }
        }
    }
}
