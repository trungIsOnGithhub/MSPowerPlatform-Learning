using PAS.Model.Mapping;
using PAS.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using PAS.Model.Domain;
using PAS.Model.Dto;
using System.Net;
using PAS.DataTransfer;
using PAS.Common;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IUserService userService;
        private readonly IUserDtoMapper userDtoMapper;
        private readonly IPermissionService permissionService;
        private readonly IMsGraphService _graphService;
        private readonly IWorkHistoryService workHistoryService;

        public UsersController(IUserService userService, IUserDtoMapper userDtoMapper, IPermissionService permissionService, IMsGraphService graphService, IWorkHistoryService workHistoryService, IApplicationContext appContext)
        {
            this.userService = userService;
            this.userDtoMapper = userDtoMapper;
            this.permissionService = permissionService;
            _graphService = graphService;
            this.workHistoryService = workHistoryService;
        }

        [HttpGet]
        public Model.Dto.User GetUserById(int id)
        {
            Model.User user = userService.GetUserById(id);
            var dtoUser = userDtoMapper.ToDto(user);
            BindTechnicalLevel(user, dtoUser);
            return dtoUser;
        }

        [HttpGet]
        [Route("teamDetails/{id}")]
        public List<PAS.Model.Domain.ViewUserDetails.ViewUserTeamDetails> GetUserTeamDetails(int id)
        {
            return this.userService.GetUserTeamDetails(id);
        }

        [HttpGet]
        [Route("technicalLevelDetails/{id}")]
        public List<PAS.Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails> GetUserTechnicalLevelDetails(int id)
        {
            var user = userService.GetUserById(id);
            if (permissionService.HasRightToViewTechnicalLevel(user))
            {
                return userService.GetUserTechnicalLevelDetails(id);
            }
            else
            {
                return new List<Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails>();
            }
        }

        [HttpGet]
        [Route("coacingDetails/{id}")]
        public List<PAS.Model.Domain.ViewUserDetails.ViewUserCoachingDetails> GetUserCoachingDetails(int id)
        {
            return this.userService.GetUserCoachingDetails(id);
        }

        [HttpGet]
        public List<Model.Dto.User> GetAllUser()
        {
            List<Model.User> users = userService.GetAllUser();
            List<Model.Dto.User> result = new List<Model.Dto.User>();
            foreach (var user in users)
            {
                var dtoUser = userDtoMapper.ToDto(user);
                BindTechnicalLevel(user, dtoUser);
                result.Add(dtoUser);
            }
            return result;
        }

        private void BindTechnicalLevel(Model.User domUser, User dtoUser)
        {
            if (!permissionService.HasRightToViewTechnicalLevel(domUser))
            {
                dtoUser.TechnicalLevel = string.Empty;
                if (dtoUser.Manager != null)
                {
                    dtoUser.Manager.TechnicalLevel = string.Empty;
                }
            }
        }

        [HttpGet]
        [Route("coach")]
        public List<Model.Dto.User> GetAllCoach()
        {
            List<Model.User> coachs = userService.GetAllCoach();
            List<Model.Dto.User> result = new List<Model.Dto.User>();
            foreach (var coach in coachs)
            {
                var dtoUser = userDtoMapper.ToDto(coach);
                BindTechnicalLevel(coach, dtoUser);
                result.Add(dtoUser);
            }
            return result;
        }

        [HttpGet]
        [Route("LoginName/{loginName}")]
        public Model.Dto.User GetUserByLoginName(string loginName)
        {
            var user = userService.GetUserByLoginName(loginName);
            var dtoUser = userDtoMapper.ToDto(user);
            BindTechnicalLevel(user, dtoUser);
            return dtoUser;
        }

        [HttpPost]
        [Route("filterListUser")]
        public List<Model.Dto.User> FilterListUser([FromBody] FilterUserParams filterUserParams)
        {
            if (filterUserParams.query == null)
            {
                filterUserParams.query = "";
            }
            List<Model.User> users = userService.FilterListUser(filterUserParams.listCoach, filterUserParams.listTeam,filterUserParams.listCareerPathTemplateSteps, filterUserParams.status,  filterUserParams.query);
            List<Model.Dto.User> result = new List<Model.Dto.User>();
            foreach (var user in users)
            {
                var dtoUser = userDtoMapper.ToDto(user);
                BindTechnicalLevel(user, dtoUser);
                result.Add(dtoUser);
            }
            return result;
        }


        [HttpPost]
        [Route("adminSaveUser")]
        public int AdminSaveUser([FromBody] SaveUserParams saveUserParams)
        {
            if (!permissionService.HasRightToViewAdminPage())
            {
                throw new System.Exception("Access Denied!");
            }
            return userService.AdminSaveUser(saveUserParams.user, saveUserParams.isEditMode);
        }

        [HttpPost]
        [Route("terminate")]
        public bool TerminateUser([FromBody] Model.User user)
        {
            if (!permissionService.HasRightToViewAdminPage())
            {
                throw new System.Exception("Access Denied!");
            }
            workHistoryService.TerminateUser(user);
            return true;
        }

        [HttpPost]
        [Route("restore")]
        public IHttpActionResult RestoreUser([FromBody] RestoredUser user)
        {
            if (!permissionService.HasRightToViewAdminPage())
            {
                return Content(HttpStatusCode.Forbidden, "Access Denied!");
            }

            if (user == null)
            {
                return BadRequest();
            }

            try
            {
                workHistoryService.RestoreUser(user, user.ReturnedDate);
                return Ok();
            } catch(BadRequestException badRequest)
            {
                return BadRequest(badRequest.Message);
            } catch(NotFoundException notFound)
            {
                return Content(HttpStatusCode.NotFound, notFound.Message);
            }
        }


        [HttpGet]
        [Route("record/work/{userId}")]
        public ICollection<WorkRecordDTO> GetWorkRecords(int userId)
        {
            if (!permissionService.HasRightToViewAdminPage())
            {
                throw new UnauthorizedAccessException($"Access Denied!");
            }

            return workHistoryService.GetWorkRecord(userId);
        }

        [HttpPost]
        [Route("record/work/update")]
        public IHttpActionResult UpdateWorkRecord([FromBody] WorkRecordDTO update)
        {
            if (!permissionService.HasRightToViewAdminPage())
            {
                return Content(HttpStatusCode.Forbidden, "Access Denied!");
            }

            if (update == null)
            {
                return BadRequest();
            }

            workHistoryService.UpdateRecord(update);

            return Ok();
        }


        [HttpGet]
        [Route("getAllUserFromAzure")]
        public Task<List<UserAzureAD>> GetAllUserFromAzure()
        {
            return _graphService.GetAllAzureUsersInfo();
        }

        [HttpGet]
        [Route("getUserPhoto/{userId}")]
        public byte[] GetUserPhoto(string userId)
        {
            return _graphService.GetUserPhotoAsync(userId);
        }

        [HttpGet]
        [Route("getUserByFilter/{input}")]
        public Task<List<UserAzureAD>> GetUserByFilter(string input)
        {
            return _graphService.GetUserByFilter(input);
        }

        [HttpPost]
        [Route("updateBillableType")]
        public int SaveBillableType([FromBody] Model.User user)
        {
            if (!permissionService.HasRightToViewAdminPage())
            {
                throw new System.Exception("Access Denied!");
            }

            return userService.UpdateBillableType(user);

        }

        [HttpPost]
        [Route("userCoaching/update/{userCoachingId}")]
        public bool UpdateUserCoaching([FromBody] tempDates tempDates, [FromUri(Name = "userCoachingId")] int id)
        {
            DateTime startDate = tempDates.startDate;
            DateTime endDate = tempDates.endDate;
            return this.userService.UpdateUserCoaching(id, startDate, endDate);
        }

        [HttpGet]
        [Route("technicalLevel/{userId}")]
        public Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails GetCurrentTechnicalLevel([FromUri] int userId)
        {
            var user = userService.GetUserById(userId);
            if (permissionService.HasRightToViewTechnicalLevel(user))
            {
                return userService.GetCurrentTechnicalLevel(userId);
            }
            else
            {
                return new Model.Domain.ViewUserDetails.ViewUserTechnicalLevelDetails { currentCareerPathTitle = string.Empty, currentCareerPathStepTitle = string.Empty, careerPathStepTitle = string.Empty, careerPathTemplateTitle = string.Empty };
            }
        }

        [HttpGet]
        [Route("projectHistory/{userId}")]
        public List<ProjectAndUsersWithObject> GetListProjectByUser([FromUri] int userId)
        {
            return userService.GetListProjectByUser(userId);
        }
    }
}