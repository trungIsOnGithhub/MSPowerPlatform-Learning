using Microsoft.SharePoint.Client;
using PAS.Common;
using PAS.Common.Configurations;
using PAS.Common.Utilities;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services
{
    public interface ISPUserService
    {
        int SyncUsers();
    }

    public class SPUserService : ISPUserService
    {
        private readonly ISharepointContextProvider _sharepointContextProvider;
        private readonly IUserRepository _userRepository;
        private readonly string listName = "User_VN";

        public SPUserService(ISharepointContextProvider sharepointContextProvider, IUserRepository userRepository)
        {
            _sharepointContextProvider = sharepointContextProvider;
            _userRepository = userRepository;
        }

        public int SyncUsers()
        {
            var users = _userRepository.GetAllUser().Where(u => u.Active).ToList();


            users.Add(new Model.User
            {
                Email = "user990@ngodev.onmicrosoft.com",
                IsDeveloper = true,
                JoinedDate = DateTime.Parse("2022/01/01"),
                LoginName = "user990@ngodev.onmicrosoft.com",
                Name = "Boss Lady Phuong",
                Active = true,
                TeamId = 2,
                Manager = users.Where(u => u.Id == 1).FirstOrDefault()
            });


            // The code below is used to sync users from sharepoint to a local db. I commented it out for testing purposes


            //string userListUrl = SharePointConfigurations.SharePointSyncUserListUrl;
            //ClientContext adminCtx = _sharepointContextProvider.GetAppOnlyContext(userListUrl).GetAwaiter().GetResult();
            //var spUserList = adminCtx.Web.Lists.GetByTitle(listName);
            //var spQuery = CamlQuery.CreateAllItemsQuery();
            //ListItemCollection spUsers = spUserList.GetItems(spQuery);
            //adminCtx.Load(spUsers);
            //adminCtx.ExecuteQuery();
            //var users = new List<Model.User>();

            //foreach (var spUser in spUsers)
            //{
            //    Model.User user = new Model.User { Role = Model.Enum.Role.Member };
            //    user.LoginName = user.Email = spUser["Email"]?.ToString()?.Trim();
            //    if (string.IsNullOrWhiteSpace(user.LoginName) || user.LoginName.Contains(";") || user.LoginName.Contains("#") || user.LoginName.Contains("|"))
            //    {
            //        continue;
            //    }

            //    users.Add(user);
            //    user.Name = spUser["Title"]?.ToString()?.Trim();
            //    user.Team = spUser["Team"]?.ToString()?.Trim() ?? "N/A";
            //    user.JoinedDate = spUser["EffectiveDate"] != null ? (DateTime)spUser["EffectiveDate"] : DateTime.Now;
            //    user.YearOfService = CommonUtilities.CalcNumOfYears(user.JoinedDate);
            //    DateTime? expDate = spUser["ExpStartFrom"] != null ? (DateTime)spUser["ExpStartFrom"] : (DateTime?)null;
            //    user.YearOfExperience = CommonUtilities.CalcNumOfYears(expDate);
            //    DateTime? terminatedDate = spUser["Terminated"] != null ? (DateTime)spUser["Terminated"] : (DateTime?)null;
            //    user.Active = terminatedDate == null;
            //    user.TechnicalLevel = spUser["TechLevel"]?.ToString()?.Trim();
            //}
            return _userRepository.SyncUsers(users);
        }
        
    }
}
