using System.Collections.Generic;
using PAS.Model.Domain;

namespace PAS.Services
{
    public interface IResumeHighlightedSkillService
    {
        List<ResumeHighlightedSkill> GetResumeHighlightedSkillByResumeSkillCatId(int resumeHighlightedSkillCateId);
    }
}
