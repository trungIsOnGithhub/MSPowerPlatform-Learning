using PAS.Model.Dto;
using PAS.Model.Mapping;
using PAS.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PAS.Model.Enum;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/resumes")]
    public class ResumeController : BaseController
    {
        private readonly IResumeService resumeService;
        private readonly IProfileService profileService;
        private readonly IProfileItemService profileItemService;
        private readonly ITemplateService templateService;
        private readonly ILookupProfileService lookupProfileService;
        private readonly IResumeHighlightedSkillService resumeHighlightedSkillService;
        private readonly IResumeHighlightedSkillCategoryService resumeHighlightedSkillCategoryService;

        private readonly IGenericItemDtoMapper genericItemDtoMapper;
        private readonly IResumeDtoMapper resumeDtoMapper;
        private readonly IProfileItemDtoMapper profileItemDtoMapper;
        private readonly IProfileCategoryDtoMapper profileCategoryDtoMapper;
        private readonly ITemplateDtoMapper templateDtoMapper;
        private readonly ITemplateProfileCategoryDtoMapper templateProfileCategoryDtoMapper;
        private readonly ISkillDtoMapper skillDtoMapper;
        private readonly ISkillCategoryDtoMapper skillCategoryDtoMapper;
        private readonly IResumeHighlightedSkillDtoMapper resumeHighlightedSkillDtoMapper;
        private readonly IResumeHighlightedSkillCategoryDtoMapper resumeHighlightedSkillCategoryDtoMapper;
        private readonly IResumeHighlightedSkillCategoryRequestDtoMapper highlightedSkillCategoryRequestDtoMapper;

        public ResumeController(IPermissionService permissonService, IResumeService resumeService, IProfileService profileService, IProfileItemService profileItemService, ITemplateService templateService, ILookupProfileService lookupProfileService, IResumeHighlightedSkillService resumeHighlightedSkillService, IResumeHighlightedSkillCategoryService resumeHighlightedSkillCategoryService, IGenericItemDtoMapper genericItemDtoMapper, IResumeDtoMapper resumeDtoMapper, IProfileItemDtoMapper profileItemDtoMapper, IProfileCategoryDtoMapper profileCategoryDtoMapper, ITemplateDtoMapper templateDtoMapper, ITemplateProfileCategoryDtoMapper templateProfileCategoryDtoMapper, ISkillDtoMapper skillDtoMapper, ISkillCategoryDtoMapper skillCategoryDtoMapper, IResumeHighlightedSkillDtoMapper resumeHighlightedSkillDtoMapper, IResumeHighlightedSkillCategoryDtoMapper resumeHighlightedSkillCategoryDtoMapper, IResumeHighlightedSkillCategoryRequestDtoMapper highlightedSkillCategoryRequestDtoMapper) : base(permissonService)
        {
            this.resumeService = resumeService;
            this.profileService = profileService;
            this.profileItemService = profileItemService;
            this.templateService = templateService;
            this.lookupProfileService = lookupProfileService;
            this.resumeHighlightedSkillService = resumeHighlightedSkillService;
            this.resumeHighlightedSkillCategoryService = resumeHighlightedSkillCategoryService;
            this.genericItemDtoMapper = genericItemDtoMapper;
            this.resumeDtoMapper = resumeDtoMapper;
            this.profileItemDtoMapper = profileItemDtoMapper;
            this.profileCategoryDtoMapper = profileCategoryDtoMapper;
            this.templateDtoMapper = templateDtoMapper;
            this.templateProfileCategoryDtoMapper = templateProfileCategoryDtoMapper;
            this.skillDtoMapper = skillDtoMapper;
            this.skillCategoryDtoMapper = skillCategoryDtoMapper;
            this.resumeHighlightedSkillDtoMapper = resumeHighlightedSkillDtoMapper;
            this.resumeHighlightedSkillCategoryDtoMapper = resumeHighlightedSkillCategoryDtoMapper;
            this.highlightedSkillCategoryRequestDtoMapper = highlightedSkillCategoryRequestDtoMapper;
        }

        [HttpGet]
        [Route("{userId}")]
        public List<ResumeDto> GetUserResumes(int userId)
        {
            return resumeService.GetUserResumes(userId).Select(x => resumeDtoMapper.ToDto(x)).ToList();
        }

        [HttpPost]
        [Route("resume")]
        public void AddResume(Model.Dto.ResumeDto resume)
        {
            resumeService.AddResume(resumeDtoMapper.ToDomain(resume));
        }

        [HttpPut]
        [Route("resume")]
        public void UpdateResume(Model.Dto.ResumeDto resume)
        {
            resumeService.UpdateResume(resumeDtoMapper.ToDomain(resume));
        }

        [HttpDelete]
        [Route("resume/{resumeId}")]
        public void DeleteResume(int resumeId)
        {
            resumeService.DeleteResume(resumeId);
        }

        [HttpPost]
        [Route("resume/{resumeId}")]
        public void CopyResume(int resumeId, Model.Dto.ResumeDto resume)
        {
            resumeService.CopyResume(resumeId, resumeDtoMapper.ToDomain(resume));
        }

        [HttpGet]
        [Route("resume/item/{resumeId}")]
        public ResumeItemResponse GetResumeItems(int resumeId)
        {
            var resume = resumeService.GetResume(resumeId);
            if (resume != null && resume.User != null)
            {
                CheckResumePagePermission(resume);
                var resumeItems = resumeService.GetResumeItems(resume.Id);
                var templateHeadings = templateService.GetTemplateHeadings(resume.Template.Id);
                List<Model.Dto.ProfileCategoryDto> resumeCategories = profileService.GetResumeCategories().Select(x => profileCategoryDtoMapper.ToDto(x)).ToList();
                List<Model.Dto.ResumeHighlightedSkillCategoryDto> highlightedSkillCategories = resumeHighlightedSkillCategoryService.GetResumeHighlightedSkillCategoryByResumeId(resumeId).Select(x => resumeHighlightedSkillCategoryDtoMapper.ToDto(x)).ToList();
                foreach (var categorySkill in highlightedSkillCategories)
                {
                    var skills = resumeHighlightedSkillService.GetResumeHighlightedSkillByResumeSkillCatId(categorySkill.Id).Select(x => resumeHighlightedSkillDtoMapper.ToDto(x)).ToList();
                    categorySkill.ResumeHighlightedSkills = skills.OrderBy(x => x.SortOrder).ToList();
                }
                foreach (var resumeCategory in resumeCategories)
                {
                    foreach (var profileItem in resumeItems)
                    {
                        if (profileItem.ProfileCategory.Id == resumeCategory.Id)
                            resumeCategory.Items.Add(profileItemDtoMapper.ToDto(profileItem));
                    }
                }
                var hasHighghtedCategory = resumeCategories.Any(x => x.Id == (int)ProfileCategory.SkillsByCategory);
                if (hasHighghtedCategory)
                {
                    resumeCategories.First(x => x.Id == (int)ProfileCategory.SkillsByCategory).ResumeHighlightedSkillCategories = highlightedSkillCategories;
                }
                return new ResumeItemResponse
                {
                    TemplateHeadings = templateHeadings.Select(x => templateProfileCategoryDtoMapper.ToDto(x)).ToList(),
                    ProfileCategories = resumeCategories
                };
            }
            return null;
        }

        [HttpPost]
        [Route("resume/item")]
        public void AddResumeItem(ResumeItemRequest request)
        {
            if (request != null)
                resumeService.AddResumeItem(profileItemDtoMapper.ToDomain(request.resumeItem),
                                                request.resumes.Select(x => resumeDtoMapper.ToDomain(x)).ToList());
        }

        [HttpPut]
        [Route("resume/item")]
        public void UpdateResumeItem(ProfileItemDto resumeItem)
        {
            resumeService.UpdateResumeItem(profileItemDtoMapper.ToDomain(resumeItem));
        }

        [HttpDelete]
        [Route("resume/item/{parentItemId}")]
        public void DeleteResumeItem(int parentItemId)
        {
            profileItemService.DeleteProfileItem(parentItemId);
        }

        [HttpPut]
        [Route("resume/items")]
        public void UpdateResumeItems(ResumeItemRequest request)
        {
            if (request != null)
                resumeService.UpdateResumeItems(profileItemDtoMapper.ToDomain(request.resumeItem),
                                                request.resumes.Select(x => resumeDtoMapper.ToDomain(x)).ToList());
        }

        [HttpGet]
        [Route("lookup")]
        public ResumeLookup GetResumeLookup()
        {
            return new ResumeLookup
            {
                Languages = lookupProfileService.GetActiveLanguages().Select(x => genericItemDtoMapper.ToDto(x)).ToList(),
                Skills = profileService.GetSkills().Select(x => skillDtoMapper.ToDto(x)).ToList(),
                SkillCategories = profileService.GetSkillCategories().Select(x => skillCategoryDtoMapper.ToDto(x)).ToList()
            };
        }

        [HttpGet]
        [Route("template/all-templates")]
        public List<TemplateDto> GetTemplates()
        {
            List<Model.Dto.TemplateDto> templates = templateService.GetTemplates().Select(x => templateDtoMapper.ToDto(x)).ToList();
            return templates;
        }

        [HttpGet]
        [Route("resume/highlighted-skill-category/{resumeId}")]
        public List<ResumeHighlightedSkillCategoryDto> GetHighlightedSkillByResumeId(int resumeId)
        {
            var listHighlightedSkills = resumeHighlightedSkillCategoryService.GetResumeHighlightedSkillCategoryByResumeId(resumeId).Select(x => resumeHighlightedSkillCategoryDtoMapper.ToDto(x)).ToList();
            foreach (var item in listHighlightedSkills)
            {
                var skillList = resumeHighlightedSkillService.GetResumeHighlightedSkillByResumeSkillCatId(item.Id).Select(x => resumeHighlightedSkillDtoMapper.ToDto(x)).ToList();
                item.ResumeHighlightedSkills = skillList.OrderBy(x => x.SortOrder).ToList();
            }
            return listHighlightedSkills;
        }

        [HttpPost]
        [Route("resume/highlighted-skill-category")]
        public bool AddResumeHighlightedSkillCategory(Model.Dto.ResumeHighlightedSkillCategoryRequestDto resumeHighlightedSkillCategoryRequest)
        {
            return resumeHighlightedSkillCategoryService.AddResumeHighlightedSkillCategory(highlightedSkillCategoryRequestDtoMapper.ToDomain(resumeHighlightedSkillCategoryRequest));
        }

        [HttpPut]
        [Route("resume/highlighted-skill-category")]
        public bool UpdateResumeHighlightedSkillCategory(Model.Dto.ResumeHighlightedSkillCategoryRequestDto resumeHighlightedSkillCategoryRequest)
        {
            return resumeHighlightedSkillCategoryService.UpdateResumeHighlightedSkillCategory(highlightedSkillCategoryRequestDtoMapper.ToDomain(resumeHighlightedSkillCategoryRequest));
        }

        [HttpDelete]
        [Route("resume/highlighted-skill-category/{resumeHighlightedSkillId}")]
        public bool DeleteResumeHighlightedSkillCategory(int resumeHighlightedSkillId)
        {
            return resumeHighlightedSkillCategoryService.DeleteResumeHighlightedSkillCategory(resumeHighlightedSkillId);
        }
    }
}