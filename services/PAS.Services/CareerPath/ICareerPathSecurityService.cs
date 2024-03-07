using PAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface ICareerPathSecurityService
    {
        bool hasFullControl();
        bool hasPermissionOnCareerPath(User user, StoryCareerPath careerPath);
        public bool hasUpdateCurrentStepPermission(User user, StoryCareerPath careerPath);
    }
}
