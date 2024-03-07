using PAS.Model.Domain;
using PAS.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services
{
    public class ProfileItemService : IProfileItemService
    {
        private readonly IProfileItemRepository profileItemRepository;
        private readonly IResumeRepository resumeRepository;

        public ProfileItemService(IProfileItemRepository profileItemRepository, IResumeRepository resumeRepository)
        {
            this.profileItemRepository = profileItemRepository;
            this.resumeRepository = resumeRepository;
        }

        public ProfileItem AddProfileItem(ProfileItem profileItem)
        {
            var newProfileItem = profileItemRepository.AddProfileItem(profileItem);

            if (profileItem.IsAtPrecioFishbone)
            {
                PrecioFishboneWorkExperience pf = new PrecioFishboneWorkExperience();
                pf.ProfileItemId = newProfileItem.Id;
                profileItemRepository.AddPrecioFishboneWorkExperience(pf);
            }



            var resumes = resumeRepository.GetUserResumes(profileItem.User.Id);
            foreach(var resume in resumes)
            {
                var resumeItem = profileItem;
                resumeItem.ParentItem = newProfileItem;
                resumeItem.Resume = resume;
                resumeItem.CreatedBy = newProfileItem.CreatedBy;
                resumeItem.ModifiedBy = newProfileItem.ModifiedBy;
                resumeItem.IsShownOnResume = true;
                profileItemRepository.AddProfileItem(resumeItem);
            }

            if (profileItem.ProfileCategory.Id == (int)Model.Enum.ProfileCategory.WorkExperience)
                AddNotExistsSkillItems(profileItem);

            return newProfileItem;
        }

        public void UpdateProfileItem(ProfileItem profileItem)
        {
            profileItemRepository.UpdateProfileItem(profileItem);

            if (profileItem.ProfileCategory.Id == (int)Model.Enum.ProfileCategory.WorkExperience)
                AddNotExistsSkillItems(profileItem);
            if (!profileItem.IsAtPrecioFishbone)
            {
                profileItemRepository.DeletePrecioFishboneWorkExperienceByProfileItemId(profileItem.Id);
            }
            else
            {
                PrecioFishboneWorkExperience pf = new PrecioFishboneWorkExperience();
                pf.ProfileItemId = profileItem.Id;
                profileItemRepository.AddPrecioFishboneWorkExperience(pf);
            }
        }

        public void DeleteProfileItem(int id)
        {
            var childrenItems = profileItemRepository.GetChildrenItems(id);
            profileItemRepository.DeletePrecioFishboneWorkExperienceByProfileItemId(id);
            profileItemRepository.DeleteProfileItem(id);

            foreach (var item in childrenItems)
                profileItemRepository.DeleteProfileItem(item.Id);
        }

        public List<ProfileItem> GetUserProfile(int userId)
        {
            var profileItems = profileItemRepository.GetUserProfile(userId);

            if (profileItems.FirstOrDefault(x => x.ProfileCategory.Id == (int)Model.Enum.ProfileCategory.Presentation) == null)
            {
                var user = new Model.User
                {
                    Id = userId
                };
                var defaultItem = new ProfileItem
                {
                    Title = "Project Manager",
                    User = user,
                    CreatedBy = user,
                    ModifiedBy = user,
                    ProfileCategory = new ProfileCategory
                    {
                        Id = (int)Model.Enum.ProfileCategory.Presentation
                    }
                };
                var newItem = AddProfileItem(defaultItem);
                profileItems.Add(newItem);
            }

            return profileItems;
        }

        public List<ProfileItem> GetTopSkills(int userId, int topX)
        {
            var profileItems = profileItemRepository.GetTopSkills(userId, topX);
            return profileItems;
        }

        private void AddNotExistsSkillItems(ProfileItem profileItem)
        {
            var existsSkillItems = profileItemRepository.GetExistsItems((int)Model.Enum.ProfileCategory.UserSkill, profileItem.User.Id);
            var notExistsSkills = profileItem.WorkExperienceSkills.Where(r1 => !existsSkillItems.Any(r2 => r2.Skill.Id == r1.Id)).ToList();

            foreach (var skill in notExistsSkills)
            {
                var user = new Model.User
                {
                    Id = profileItem.User.Id
                };
                var newSkillItem = new ProfileItem
                {
                    Title = skill.Title,
                    User = user,
                    CreatedBy = user,
                    ModifiedBy = user,
                    ProfileCategory = new ProfileCategory
                    {
                        Id = (int)Model.Enum.ProfileCategory.UserSkill
                    }, 
                    Skill = skill
                };
                AddProfileItem(newSkillItem);
            }
        }

        public List<CompanySkill> GetCompanySkills()
        {
            var listCompanySkills = profileItemRepository.GetCompanySkills();
            return listCompanySkills;
        }

        public List<ProfileItem> GetProfileItemsBySkillId(int skillId)
        {
            var listProfileItems = profileItemRepository.GetProfileItemsBySkillId(skillId);
            return listProfileItems;
        }
    }
}
