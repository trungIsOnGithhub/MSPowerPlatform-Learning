using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using PAS.Common.Configurations;
using Azure.Identity;
using PAS.Model.Domain;
using Microsoft.Graph.Models;
using System.IO;
using Microsoft.Kiota.Abstractions;
using System.Net.Http;
using Azure.Core;
using Microsoft.Extensions.Options;

namespace PAS.Services
{
    public class MsGraphService : IMsGraphService
    {
        private readonly string[] _defaultScope;
        private readonly string _accessToken;
        private readonly HttpClient _httpClient;
        private readonly AzureConfigurations azureConfigurations;
        public MsGraphService(AzureConfigurations azureConfigurations) {
            this.azureConfigurations = azureConfigurations;
            _defaultScope = new[] { "https://graph.microsoft.com/.default" };
            _accessToken = GetAccessToken();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
        public ClientSecretCredential GetClientSecretCredential()
        {
            var clientId = azureConfigurations.ClientId;
            var clientSecret = azureConfigurations.Password;
            var tenantId = azureConfigurations.Tenant;

            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            return clientSecretCredential;
        }
        public GraphServiceClient GetMsGraphClient()
        {
            var clientSecretCredential = GetClientSecretCredential();

            return new GraphServiceClient(clientSecretCredential, _defaultScope);
        }
        public string GetAccessToken()
        {
            var clientSecretCredential = GetClientSecretCredential();
            var tokenRequestContext = new TokenRequestContext(_defaultScope);

            return clientSecretCredential.GetToken(tokenRequestContext).Token;
        }
        public byte[] GetUserPhotoAsync(string userId)
        {
            string endpoint = string.Format("https://graph.microsoft.com/v1.0/users/{0}/photo/$value", userId);

            using (var response = _httpClient.GetAsync(endpoint).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    var stream = response.Content.ReadAsStreamAsync().Result;
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    return bytes;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task<List<UserAzureAD>> GetAllAzureUsersInfo()
        {
            /* 
                Get All users from AzureAD (including external) 
            */

            List<User> usersFromAzureAD = new List<User>();
            var graphClient = GetMsGraphClient();
            var pageResponse = await graphClient.Users.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "mail", "userPrincipalName" };
            });
            // Get next page. By default, page size = 100
            var pageIterator = PageIterator<User, UserCollectionResponse>.CreatePageIterator(graphClient, pageResponse, (user) => { usersFromAzureAD.Add(user); return true; });

            await pageIterator.IterateAsync();

            List<UserAzureAD> users = new List<UserAzureAD>();

            foreach (var user in usersFromAzureAD)
            {
                // Get user's photo
                /*var userPhoto = GetUserPhotoAsync(user.Id);*/

                if (user.Mail != null) users.Add(new UserAzureAD { Id = user.Id, Mail = user.Mail, Name = user.DisplayName });
                else
                {
                    // Preprocess Email field - chau.huynh.thi.bao_preciofishbone.se#EXT#@ngodev.onmicrosoft.com -> chau.huynh.thi.bao@preciofishbone.se
                    var extendKey = "#EXT#@" + azureConfigurations.Tenant;
                    var userMail = "";

                    if (user.UserPrincipalName.Contains(extendKey))
                    {
                        userMail = user.UserPrincipalName.Substring(0, user.UserPrincipalName.IndexOf(extendKey));
                        userMail.Replace('_', '@');
                    }
                    else userMail = user.UserPrincipalName;

                    users.Add(new UserAzureAD { Id = user.Id, Mail = userMail, Name = user.DisplayName });
                }
                
            }

            return users;
        }

        public async Task<List<UserAzureAD>> GetUserByFilter(string input)
        {
            /*
                Get User by (filtering userPrincicalName or userDisplayName) and userType == 'member'
            */
            List<User> usersFromAzureAD = new List<User>();
            var graphClient = GetMsGraphClient();
            var pageResponse = await graphClient.Users.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "userPrincipalName", "mail", "userType" };
                requestConfiguration.QueryParameters.Filter = "userType eq 'member'";
                requestConfiguration.QueryParameters.Search = string.Format("(\"displayName:{0}\" OR \"userPrincipalName:{0}\" OR \"mail:{0}\")", input);
                requestConfiguration.QueryParameters.Count = true;
                requestConfiguration.QueryParameters.Orderby = new string[] { "displayName" };
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
            });
            // Get next page. By default, page size = 100
            var pageIterator = PageIterator<User, UserCollectionResponse>.CreatePageIterator(graphClient, pageResponse, (user) => { usersFromAzureAD.Add(user); return true; });

            await pageIterator.IterateAsync();

            List<UserAzureAD> users = new List<UserAzureAD>();
            foreach (var user in usersFromAzureAD)
            {
                /*byte[] userPhoto = GetUserPhotoAsync(user.Id);*/
                users.Add(new UserAzureAD { Id = user.Id, Mail = user.Mail != null ? user.Mail : user.UserPrincipalName, Name = user.DisplayName });
            }

            return users;
        }
    }
}