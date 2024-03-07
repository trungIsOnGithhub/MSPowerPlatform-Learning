using PAS.Common.Constants;
using System.Configuration;

namespace PAS.Common.Configurations
{
    public class SharePointConfigurations
    {
        public string SharePointRootUrl
        {
            get; set;
        }

        public string TenantUrl
        {
            get; set;
        }

        public string SharePointSyncUserListUrl
        {
            get; set;
        }
    }
}
