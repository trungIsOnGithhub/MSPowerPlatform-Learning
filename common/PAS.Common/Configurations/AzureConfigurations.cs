using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PAS.Common.Configurations
{
    public class AzureConfigurations
    {     
        public string ClientId
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }
        public string Tenant
        {
            get; set;
        }

        public string Authority
        {
            get; set;
        }

        public string Audience
        {
            get; set;
        }

        public string GraphApi
        {
            get; set;
        }

        public int ReminderRemainingTimeOfDueDate
        {
            get; set;
        }

        public string SyncUserTechLevel
        {
            get; set;
        }

        public List<int> SyncUserIgnoreIDs
        {
            get; set;
        }
    }
}
