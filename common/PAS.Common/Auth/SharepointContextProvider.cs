using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using System.Configuration;
using System.Security;
using PAS.Common.Configurations;
using Microsoft.Extensions.Options;

namespace PAS.Common
{
    public class SharepointContextProvider : ISharepointContextProvider
    {
        private readonly IAuthProvider authProvider;
        private readonly AzureConfigurations azureConfigurations;
        private readonly SharePointConfigurations sharePointConfigurations;

        public SharepointContextProvider(IAuthProvider authProvider, AzureConfigurations azureConfigurations, SharePointConfigurations sharePointConfigurations)
        {
            this.authProvider = authProvider;
            this.azureConfigurations = azureConfigurations;
            this.sharePointConfigurations = sharePointConfigurations;
        }

        public async Task<ClientContext> GetClientContext(string webUrl = null)
        {
            webUrl = webUrl ?? sharePointConfigurations.SharePointRootUrl;
            var accessToken = await authProvider.GetUserAccessTokenAsync();
            return new AuthenticationManager().GetAzureADAccessTokenAuthenticatedContext(webUrl, accessToken);
        }

        public Task<ClientContext> GetAppOnlyContext(string webUrl = null)
        {
            webUrl = webUrl ?? sharePointConfigurations.SharePointRootUrl;

            var clientId = azureConfigurations.ClientId;
            var tenant = azureConfigurations.Tenant;

            return Task.FromResult(new AuthenticationManager().GetAzureADAppOnlyAuthenticatedContext(webUrl, clientId, tenant, AuthSettings.AppOnlyCertificate));
        }

        public ClientContext GetUserContext(string webUrl = null)
        {
            var userName = ConfigurationManager.AppSettings["AdminUserName"];
            var password = ConfigurationManager.AppSettings["AdminPassword"];

            webUrl = webUrl ?? sharePointConfigurations.SharePointRootUrl;

            var context = new ClientContext(webUrl);

            SecureString securePassword = new SecureString();

            foreach (char c in password.ToCharArray()) securePassword.AppendChar(c);

            context.Credentials = new SharePointOnlineCredentials(userName, securePassword);

            return context;
        }
    }
}
