using PAS.Model.Domain;
using PAS.Model.Dto;
using PAS.Repositories;
using System.Collections.Generic;

namespace PAS.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            this.profileRepository = profileRepository;
        }

        public List<ProfileCategory> GetProfileCategories()
        {
            return profileRepository.GetProfileCategories();
        }

        public List<ProfileCategory> GetResumeCategories()
        {
            return profileRepository.GetResumeCategories();
        }

        public List<SkillCategory> GetSkillCategories()
        {
            return profileRepository.GetSkillCategories();
        }

        public List<Model.Domain.Skill> GetSkills()
        {
            return profileRepository.GetSkills();
        }
    }
}
