using PAS.Repositories;
using PAS.Model.Domain;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IResumeRepository resumeRepository;
        private readonly IProfileItemRepository profileItemRepository;
        private readonly IResumeHighlightedSkillCategoryRepository resumeHighlightedSkillCategoryRepository;

        public ResumeService(IResumeRepository resumeRepository, IProfileItemRepository profileItemRepository, IResumeHighlightedSkillCategoryRepository resumeHighlightedSkillCategoryRepository)
        {
            this.resumeRepository = resumeRepository;
            this.profileItemRepository = profileItemRepository;
            this.resumeHighlightedSkillCategoryRepository = resumeHighlightedSkillCategoryRepository;
        }

        public void AddResume(Resume resume)
        {
            var newResume = resumeRepository.AddResume(resume);

            var userProfileItems = profileItemRepository
                                    .GetUserProfile(resume.User.Id)
                                    .Where(x => x.ProfileCategory.Id != (int)Model.Enum.ProfileCategory.OtherSkill
                                            && x.ProfileCategory.Id != (int)Model.Enum.ProfileCategory.References
                                            && x.ProfileCategory.Id != (int)Model.Enum.ProfileCategory.Portfolio)
                                    .ToList();
            foreach (var profileItem in userProfileItems)
            {
                var resumeItem = profileItem;
                resumeItem.ParentItem = profileItem;
                resumeItem.Resume = newResume;
                resumeItem.CreatedBy = newResume.CreatedBy;
                resumeItem.ModifiedBy = newResume.ModifiedBy;
                resumeItem.IsShownOnResume = true;
                profileItemRepository.AddProfileItem(resumeItem);
            }
        }

        public void UpdateResume(Resume resume)
        {
            resumeRepository.UpdateResume(resume);
        }

        public void DeleteResume(int resumeId)
        {
            var resumeItems = resumeRepository.GetResumeItems(resumeId);
            var resumeSkillCategories = resumeHighlightedSkillCategoryRepository.GetResumeHighlightedSkillCategoryByResumeId(resumeId);
            foreach (var category in resumeSkillCategories)
            {
                resumeHighlightedSkillCategoryRepository.DeleteResumeHighlightedSkillCategory(category.Id);
            }
            resumeRepository.DeleteResume(resumeId);
            foreach (var resumeItem in resumeItems)
                DeleteResumeItem(resumeItem.Id);
        }

        public void CopyResume(int resumeId, Resume resume)
        {
            var newResume = resumeRepository.AddResume(resume);

            var resumeItems = resumeRepository.GetResumeItems(resumeId);
            foreach (var resumeItem in resumeItems)
            {
                var newResumeItem = resumeItem;
                newResumeItem.Resume = newResume;
                newResumeItem.CreatedBy = newResume.CreatedBy;
                newResume.ModifiedBy = newResume.ModifiedBy;
                profileItemRepository.AddProfileItem(newResumeItem);
            }
        }

        public void AddResumeItem(ProfileItem resumeItem, List<Resume> resumes)
        {
            resumeItem.Resume = null;
            var profileItem = profileItemRepository.AddProfileItem(resumeItem);
            var userResumes = resumeRepository.GetUserResumes(resumeItem.User.Id);

            foreach (var resume in userResumes)
            {
                var newItem = profileItem;
                newItem.ParentItem = profileItem;
                newItem.Resume = resume;
                newItem.CreatedBy = profileItem.CreatedBy;
                newItem.ModifiedBy = profileItem.ModifiedBy;
                newItem.IsShownOnResume = resumes.FirstOrDefault(x => x.Id == resume.Id) != null;
                profileItemRepository.AddProfileItem(newItem);
            }
        }

        public void UpdateResumeItem(ProfileItem resumeItem)
        {
            profileItemRepository.UpdateProfileItem(resumeItem);
        }

        public void UpdateResumeItems(ProfileItem resumeItem, List<Resume> resumes)
        {
            var profileItem = resumeItem.ParentItem;
            MapUpdateData(resumeItem, profileItem);
            profileItemRepository.UpdateProfileItem(profileItem);

            var childrenItems = profileItemRepository.GetChildrenItems(resumeItem.ParentItem.Id);

            var updatedItems = childrenItems.Where(x =>
                                                resumes.Select(x => x.Id)
                                                        .ToList()
                                                        .Contains(x.Resume.Id))
                                            .ToList();

            foreach (var children in updatedItems)
            {
                MapUpdateData(resumeItem, children);
                profileItemRepository.UpdateProfileItem(children);
            }
        }

        public void DeleteResumeItem(int resumeItemId)
        {
            profileItemRepository.DeleteProfileItem(resumeItemId);
        }

        public List<Resume> GetUserResumes(int userId)
        {
            return resumeRepository.GetUserResumes(userId);
        }

        public List<ProfileItem> GetResumeItems(int resumeId)
        {
            return resumeRepository.GetResumeItems(resumeId);
        }

        public Resume GetResume(int resumeId)
        {
            return resumeRepository.GetResume(resumeId);
        }

        private void MapUpdateData(ProfileItem resumeItem, ProfileItem updatedItem)
        {
            updatedItem.Title = resumeItem.Title;
            updatedItem.Description = resumeItem.Description;
            updatedItem.EducationLevel = resumeItem.EducationLevel;
            updatedItem.StartDate = resumeItem.StartDate;
            updatedItem.EndDate = resumeItem.EndDate;
            updatedItem.IsFavorite = resumeItem.IsFavorite;
            updatedItem.LanguageLevel = resumeItem.LanguageLevel;
            updatedItem.Link = resumeItem.Link;
            updatedItem.Location = resumeItem.Location;
            updatedItem.Name = resumeItem.Name;
            updatedItem.FirstName = resumeItem.FirstName;
            updatedItem.LastName = resumeItem.LastName;
            updatedItem.Email = resumeItem.Email;
            updatedItem.Phone = resumeItem.Phone;
            updatedItem.PersonalPresentation = resumeItem.PersonalPresentation;
            updatedItem.ProfesionalPresentation = resumeItem.ProfesionalPresentation;
            updatedItem.SkillScore = resumeItem.SkillScore;
            updatedItem.OnGoing = resumeItem.OnGoing;
            updatedItem.IsCertificate = resumeItem.IsCertificate;
            updatedItem.Skill = resumeItem.Skill;
            updatedItem.Language = resumeItem.Language;
            updatedItem.ModifiedBy = resumeItem.ModifiedBy;
        }
    }
}