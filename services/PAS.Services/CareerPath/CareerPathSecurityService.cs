using PAS.Model;

namespace PAS.Services
{
    public class CareerPathSecurityService : ICareerPathSecurityService
    {
        private IApplicationContext applicationContext;

        public CareerPathSecurityService(IApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        public bool hasFullControl()
        {
            if (applicationContext.CurrentUser.Role == Model.Enum.Role.BOD) return true;
            else return false;
        }

        public bool hasPermissionOnCareerPath(User user, StoryCareerPath careerPath)
        {
            if (user.Id == careerPath.Story.User.Id || user.Role == Model.Enum.Role.BOD || user.Id == careerPath.Story.User.Manager?.Id) return true;
            else return false;
        }
        public bool hasUpdateCurrentStepPermission(User user, StoryCareerPath careerPath)
        {
            if (user.Role == Model.Enum.Role.BOD || user.Id == careerPath.Story.User.Manager?.Id) return true;
            else return false;
        }
    }
}
