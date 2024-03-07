using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using PAS.Model.Domain;
using Azure.Identity;
using System.IO;

namespace PAS.Services
{
    public interface IMsGraphService
    {
        GraphServiceClient GetMsGraphClient();
        Task<List<UserAzureAD>> GetAllAzureUsersInfo();
        ClientSecretCredential GetClientSecretCredential();
        string GetAccessToken();
        byte[] GetUserPhotoAsync(string userId);
        Task<List<UserAzureAD>> GetUserByFilter(string input);
    }
}