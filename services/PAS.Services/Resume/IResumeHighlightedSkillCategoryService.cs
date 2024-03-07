using PAS.Model.Domain;
using System;
using System.Collections.Generic;

namespace PAS.Services
{
    public interface IResumeHighlightedSkillCategoryService
    {
        List<ResumeHighlightedSkillCategory> GetResumeHighlightedSkillCategoryByResumeId(int resumeId);
        bool AddResumeHighlightedSkillCategory(ResumeHighlightedSkillCategoryRequest resumeHighlightedSkillCategoryRequest);
        bool UpdateResumeHighlightedSkillCategory(ResumeHighlightedSkillCategoryRequest resumeHighlightedSkillCategoryRequest);
        bool DeleteResumeHighlightedSkillCategory(int resumeHighlightedSkillCategoryId);
    }
}
