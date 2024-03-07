using System.Collections.Generic;

namespace PAS.Services
{
    public interface IProfileService
    {
        List<Model.Domain.ProfileCategory> GetProfileCategories();
        List<Model.Domain.Skill> GetSkills();
        List<Model.Domain.ProfileCategory> GetResumeCategories();
        List<Model.Domain.SkillCategory> GetSkillCategories();
    }
}
