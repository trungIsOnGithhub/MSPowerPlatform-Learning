using PAS.Common;
using PAS.Common.Constants;
using PAS.Model;
using PAS.Model.Mapping;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PAS.Model.Domain;
namespace PAS.Services
{
    public class UserService : IUserService
    {
        private IUserRepository userRepository;
        private readonly IRecordRepository<WorkRecord> workRecordRepository;

        private IUserDtoMapper _dtoMapper;
        private readonly IEnumService _enumService;
        private readonly IWorkHistoryService workHistoryService;
       

        public UserService(
            IUserRepository userRepository, 
            IUserDtoMapper dtoMapper,
            IEnumService enumService, 
            IWorkHistoryService workHistoryService,
            IRecordRepository<WorkRecord> workRecordRepository)
        {
            this.userRepository = userRepository;
            _dtoMapper = dtoMapper;
            _enumService = enumService;
            this.workHistoryService = workHistoryService;
            this.workRecordRepository = workRecordRepository;
        }

        public List<User> GetAllReferenceUsers()
        {
            var users = userRepository.GetAllUser();
            users.ForEach(user =>
            {
                user.TechnicalLevel = string.Empty;
                if (user.Manager != null)
                {
                    user.Manager.TechnicalLevel = string.Empty;
                }
            });
            return users;
        }

        public List<User> GetAllUser()
        {
            var users = userRepository.GetAllUser();
            users.ForEach(user =>
            {
                var firstRecord = workHistoryService.GetWorkRecord(user.Id).LastOrDefault();
                if (firstRecord != null)
                {
                    user.FirstTerminatedDate = firstRecord.EndDate;
                }
                user.YearOfExperience = workHistoryService.CalculateYearOfExperience(user.Id);
                user.YearOfService = workHistoryService.CalculateYearOfService(user.Id);
            });
            return users;
        }

        public List<User> GetAllCoach()
        {
            return userRepository.GetAllCoach();
        }

        public Model.Dto.User GetUserByLoginName(string loginName)
        {
            var user = userRepository.GetUserByLoginName(loginName);
            if (user == null)
            {
                throw new UnauthorizedAccessException($"User {loginName} is not existing in {Misc.SystemName}, please contact administrator to add you to the system");
            }

            var firstRecord = workHistoryService.GetWorkRecord(user.Id).LastOrDefault();
            if (firstRecord != null)
            {
                user.FirstTerminatedDate = firstRecord.EndDate;
            }

            user.YearOfExperience = workHistoryService.CalculateYearOfExperience(user.Id);
            user.YearOfService = workHistoryService.CalculateYearOfService(user.Id);
            user.TeamMembers = userRepository.GetTeamMembers(user);

            return _dtoMapper.ToDto(user);
        }

        
        public User GetUserById(int id)
        {
            var user = userRepository.GetUserById(id);
            if (user == null)
            {
                throw new UnauthorizedAccessException($"User {id} is not existing in {Misc.SystemName}, please contact administrator to add you to the system");
            }

            var firstRecord = workHistoryService.GetWorkRecord(id).LastOrDefault();
            if (firstRecord != null)
            {
                user.FirstTerminatedDate = firstRecord.EndDate;
            }
            user.YearOfExperience = workHistoryService.CalculateYearOfExperience(id);
            user.YearOfService = workHistoryService.CalculateYearOfService(id);

            return user;
        }

        public bool CheckUserRole(string roleName)
        {
            var roles = _enumService.ToListItem(typeof(Model.Enum.Role));
            var role = roles.FirstOrDefault(role => role.Name == roleName);
            if (role != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<User> GetUsersMember(int coachId)
        {
            return this.userRepository.GetMembers(coachId);
        }

        public IEnumerable<User> GetUsersWithPerformanceReviews()
        {
            return this.userRepository.GetAllUsersWithPerformanceReviews();
        }

        public IEnumerable<User> GetMembersWithPerformanceReviews(int coachId)
        {
            return this.userRepository.GetMembersWithPerformanceReviews(coachId);
        }

        public int AdminSaveUser(Model.User user, bool isEditMode)
        {
            int id = userRepository.AdminSaveUser(user, isEditMode);

            if (id > 0) // Handle Defined Id Only
            {
                var userdb = userRepository.GetUserById(id);

                if (userdb == null)
                {
                    throw new NotFoundException("A User Data is not existed");
                }

                if (!isEditMode)
                {
                    workRecordRepository.Create(new WorkRecord
                    {
                        User = userdb,
                        StartDate = DateTime.UtcNow,
                        EndDate = null
                    });
                } else
                {
                    var firstRecord = workRecordRepository.Read(id)
                        .OrderBy(record => record.StartDate)
                        .FirstOrDefault();
                    firstRecord.StartDate = user.EffectiveDate ?? user.ContractSigningDate ?? user.JoinedDate;
                    workRecordRepository.Update(firstRecord);
                }
            } 

            return id;
        }

        public int UpdateBillableType(Model.User user)
        {
            int res = userRepository.UpdateBillableType(user);
            if (res != -1) userRepository.UnitOfWork.SaveEntities();
            return res;
        }

        public List<User> FilterListUser(List<Model.User> listCoach, List<Team> listTeam, List<Model.CareerPathTemplateStep> listCareerPathTemplateSteps, Model.Status status, string query)
        {
            return userRepository.FilterListUser(listCoach, listTeam,listCareerPathTemplateSteps, status, query);
        }

        public List<PAS.Model.Domain.ViewUserDetails.ViewUserTeamDetails> GetUserTeamDetails(int idUser)
        {
            //var x =  this.userRepository.GetUserDetails(idUser);
            return this.userRepository.GetUserTeamDetails(idUser);
        }
        public List<PAS.Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails> GetUserTechnicalLevelDetails(int idUser)
        {
            return this.userRepository.GetUserTechnicalLevelDetails(idUser);
        }
        public List<PAS.Model.Domain.ViewUserDetails.ViewUserCoachingDetails> GetUserCoachingDetails(int idUser)
        {
            return this.userRepository.GetUserCoachingDetails(idUser);
        }

        public bool UpdateUserCoaching(int id, DateTime startDate, DateTime endDate)
        {
            this.userRepository.UpdateUserCoaching(id, startDate, endDate);
            this.userRepository.UnitOfWork.SaveEntities();
            return true;
        }
        public void UpdateUserTechLevel(int userId, int careerPathId)
        {
            userRepository.UpdateUserTechLevel(userId, careerPathId);
        }

        public Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails GetCurrentTechnicalLevel(int userId)
        {
            return userRepository.GetCurrentTechnicalLevel(userId);
        }
        public List<ProjectAndUsersWithObject> GetListProjectByUser(int userId)
        {
            return userRepository.GetListProjectByUser(userId);
        }

        public List<User> GetUsersWithManagerId(int managerId)
        {
            return this.userRepository.GetUsersWithManagerId(managerId);
        }
    }
}
