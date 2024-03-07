using PAS.Repositories;
using PAS.Model.Domain;
using System.Collections.Generic;

namespace PAS.Services
{
    public class ResumeHighlightedSkillService : IResumeHighlightedSkillService
    {
        private readonly IResumeHighlightedSkillRepository resumeHighlightedSkillRepository;

        public ResumeHighlightedSkillService(IResumeHighlightedSkillRepository resumeHighlightedSkillRepository)
        {
            this.resumeHighlightedSkillRepository = resumeHighlightedSkillRepository;
        }

        public List<ResumeHighlightedSkill> GetResumeHighlightedSkillByResumeSkillCatId(int resumeHighlightedSkillCateId)
        {
            return resumeHighlightedSkillRepository.GetResumeHighlightedSkillByResumeSkillCatId(resumeHighlightedSkillCateId);
        }

    }
}
