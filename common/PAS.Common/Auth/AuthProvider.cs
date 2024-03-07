using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using PAS.Common.Configurations;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Luu y LIB: Microsoft.Identity.Client

namespace PAS.Common
{
    public class AuthProvider : IAuthProvider
    {
        private readonly IDistributedCache cache;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AzureConfigurations azureConfigurations;
        private readonly SharePointConfigurations sharePointConfigurations;


        public AuthProvider(IDistributedCache cache, AzureConfigurations azureConfigurations, SharePointConfigurations sharePointConfigurations, IHttpContextAccessor httpContextAccessor)
        {
            this.cache = cache;
            this.httpContextAccessor = httpContextAccessor;
            this.azureConfigurations = azureConfigurations;
            this.sharePointConfigurations = sharePointConfigurations;
        }

        public async Task<string> GetUserAccessTokenAsync()
        {
            string accessToken = "";
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var sw2 = System.Diagnostics.Stopwatch.StartNew();
            var sw4 = new System.Diagnostics.Stopwatch();

            var clientId = azureConfigurations.ClientId;
            var password = azureConfigurations.Password;
            var tenant = azureConfigurations.Tenant;
            var resource = sharePointConfigurations.TenantUrl;
            var authority = azureConfigurations.Authority;
            sw2.Stop();

            var sw5 = System.Diagnostics.Stopwatch.StartNew();
            var user = httpContextAccessor.HttpContext.User;
            // 1. Lấy userId = giá trị Claims thuộc kiểu nhất định
            var userId = user.Claims.Where(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Select(c => c.Value).SingleOrDefault();
            sw5.Stop();

            var sw3 = System.Diagnostics.Stopwatch.StartNew();
            var cachedAccessToken = cache.Get("AccessTokenCache_" + userId);
            sw3.Stop();

            if (cachedAccessToken == null)
            {
                sw4.Start();
                var appCred = new ClientCredential(clientId, password);

                var authContext = new AuthenticationContext(authority, new DistributedTokenCache(cache, userId));
                // Auth Context specify the context of an authentication event.

                string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                string userAccessToken = authHeader.Substring(authHeader.LastIndexOf(' ')).Trim();
                // xem header của NetWork Tab tìm 'Authorization '
                // userAccessToken là JWT
                var userAssertion = new UserAssertion(userAccessToken);

                // Accquire từ AuthContext --> Azure AD Cache, mình chỉ là 1 layer??
                var result = await authContext.AcquireTokenAsync(resource, appCred, userAssertion);

                accessToken = result.AccessToken;

                cache.Set("AccessTokenCache_" + userId, Encoding.ASCII.GetBytes(accessToken), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = (result.ExpiresOn.UtcDateTime - DateTime.UtcNow)
                });

                sw4.Stop();
            }
            else
            {
                accessToken = Encoding.ASCII.GetString(cachedAccessToken);
            }

            sw1.Stop();

            return accessToken;
        }
    }
}
