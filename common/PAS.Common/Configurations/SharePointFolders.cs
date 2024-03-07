using System;

namespace PAS.Common.Configurations
{
    public class SharePointFolders
    {
        public static string IdeaApprovalFolder
        {
            get
            {
                return "IdeaApproval" + DateTime.Now.ToString("yyyyMMdd");
            }
        }        
    }
}
