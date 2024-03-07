using PAS.Model.Domain;
using PAS.Model.Enum;
using PAS.Repositories;

namespace PAS.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IApplicationContext _applicationContext;
        private readonly IUserRepository _userRepository;

        public PermissionService(IApplicationContext applicationContext, IUserRepository userRepository)
        {
            _applicationContext = applicationContext;
            _userRepository = userRepository;
        }

        public bool HasRightToViewAdminPage()
        {
            return _applicationContext.CurrentUser != null && (_applicationContext.CurrentUser.Role == Role.BOD || _applicationContext.CurrentUser.Role == Role.Admin);
        }

        public bool HasRightToViewManagerPage()
        {
            return _applicationContext.CurrentUser != null && !(_applicationContext.CurrentUser.Role == Role.Member && !_userRepository.HasMembers(_applicationContext.CurrentUser.Id));
        }

        public bool HasRightToViewResumePage(Resume resume)
        {
            var result = false;

            if (_applicationContext.CurrentUser != null)
            {
                result = resume.IsPublic == true;
                if (!result)
                {
                    var memberId = resume.User?.Id;
                    var userRole = _applicationContext.CurrentUser.Role;
                    result = (userRole == Role.BOD || userRole == Role.Admin || (memberId > 0 && memberId == _applicationContext.CurrentUser.Id)) 
                        || (_userRepository.HasMembers(_applicationContext.CurrentUser.Id, memberId));
                }
            }
            return result;
        }

        public bool HasRightToViewTechnicalLevel(Model.User userProfile)
        {
            var currentUser = _applicationContext.CurrentUser;
            return userProfile != null && (userProfile.Id == currentUser.Id || userProfile.Manager?.Id == currentUser.Id || currentUser.Role == Model.Enum.Role.BOD || currentUser.Role == Model.Enum.Role.Admin);
        }

        public bool HasRightToViewAllLeaveRequestAndTrack(Model.User userProfile)
        {
            //var currentUser = _applicationContext.CurrentUser;

            //if (currentUser == null)
            //{ return false; }

            return userProfile.Role == Role.BOD || userProfile.Role == Role.Admin;
        }
    }
}
