using PAS.Repositories;
using PAS.Model.Domain;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PAS.Services
{
    public class ResumeHighlightedSkillCategoryService : IResumeHighlightedSkillCategoryService
    {
        private readonly IResumeHighlightedSkillCategoryRepository resumeHighlightedSkillCategoryRepository;

        public ResumeHighlightedSkillCategoryService(IResumeHighlightedSkillCategoryRepository resumeHighlightedSkillCategoryRepository)
        {
            this.resumeHighlightedSkillCategoryRepository = resumeHighlightedSkillCategoryRepository;
        }

        public List<ResumeHighlightedSkillCategory> GetResumeHighlightedSkillCategoryByResumeId(int resumeId)
        {
            return resumeHighlightedSkillCategoryRepository.GetResumeHighlightedSkillCategoryByResumeId(resumeId);
        }

        public bool AddResumeHighlightedSkillCategory(ResumeHighlightedSkillCategoryRequest resumeHighlightedSkillCategoryRequest)
        {
            if (resumeHighlightedSkillCategoryRequest != null)
            {
                var newHighlightedCategory = new ResumeHighlightedSkillCategory()
                {
                    SkillCategory = resumeHighlightedSkillCategoryRequest.ResumeHighlightedSkillCategory.SkillCategory,
                    ResumeHighlightedSkills = resumeHighlightedSkillCategoryRequest.ResumeHighlightedSkills,
                    Resume = resumeHighlightedSkillCategoryRequest.ResumeHighlightedSkillCategory.Resume
                };
                return resumeHighlightedSkillCategoryRepository.AddResumeHighlightedSkillCategory(newHighlightedCategory);
            }
            return false;
        }

        public bool UpdateResumeHighlightedSkillCategory(ResumeHighlightedSkillCategoryRequest resumeHighlightedSkillCategoryRequest)
        {
            if (resumeHighlightedSkillCategoryRequest != null)
            {
                var updateHighlightedCategory = new ResumeHighlightedSkillCategory()
                {
                    Id = resumeHighlightedSkillCategoryRequest.ResumeHighlightedSkillCategory.Id,
                    SkillCategory = resumeHighlightedSkillCategoryRequest.ResumeHighlightedSkillCategory.SkillCategory,
                    ResumeHighlightedSkills = resumeHighlightedSkillCategoryRequest.ResumeHighlightedSkills,
                    Resume = resumeHighlightedSkillCategoryRequest.ResumeHighlightedSkillCategory.Resume
                };
                return resumeHighlightedSkillCategoryRepository.UpdateResumeHighlightedSkillCategory(updateHighlightedCategory);
            }
            return false;
        }

        public bool DeleteResumeHighlightedSkillCategory(int resumeHighlightedSkillCategoryId)
        {
            return resumeHighlightedSkillCategoryRepository.DeleteResumeHighlightedSkillCategory(resumeHighlightedSkillCategoryId);
        }
    }
}