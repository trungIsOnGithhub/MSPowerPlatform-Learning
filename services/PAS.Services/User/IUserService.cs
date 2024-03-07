using System.Collections.Generic;
using System.Threading.Tasks;
using PAS.Repositories;
using PAS.Model.Domain;
using PAS.Model.Dto;
using System;
namespace PAS.Services
{
    public interface IUserService
    {
        Model.User GetUserById(int id);
        Model.Dto.User GetUserByLoginName(string loginName);
        List<Model.User> GetAllReferenceUsers();
        List<Model.User> GetAllUser();
        List<Model.User> GetAllCoach();
        bool CheckUserRole(string roleName);
        IEnumerable<Model.User> GetUsersMember(int coachId);
        IEnumerable<Model.User> GetUsersWithPerformanceReviews();
        IEnumerable<Model.User> GetMembersWithPerformanceReviews(int coachId);
        int AdminSaveUser(Model.User user, bool isEditMode);
        int UpdateBillableType(Model.User user);
        List<Model.User> FilterListUser(List<Model.User> listCoach, List<Team> listTeam, List<Model.CareerPathTemplateStep> ListCareerPathTemplateSteps, Model.Status status ,string query);
        List<PAS.Model.Domain.ViewUserDetails.ViewUserTeamDetails> GetUserTeamDetails(int idUser);
        List<PAS.Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails> GetUserTechnicalLevelDetails(int idUser);
        List<PAS.Model.Domain.ViewUserDetails.ViewUserCoachingDetails> GetUserCoachingDetails(int idUser);
        bool UpdateUserCoaching(int id, DateTime startDate, DateTime endDate);
        void UpdateUserTechLevel(int userId, int careerPathId);
        Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails GetCurrentTechnicalLevel(int userId);
        List<ProjectAndUsersWithObject> GetListProjectByUser(int userId);
        List<Model.User> GetUsersWithManagerId(int managerId);
    }
}
