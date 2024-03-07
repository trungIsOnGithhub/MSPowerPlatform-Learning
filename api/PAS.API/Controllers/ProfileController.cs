using PAS.Model.Dto;
using PAS.Model.Mapping;
using PAS.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/profile")]
    public class ProfileController : ApiController
    {
        private readonly IProfileItemService profileItemService;
        private readonly IProfileService profileService;
        private readonly ILookupProfileService lookupProfileService;
        private readonly IPortfolioFileService portfolioFileService;
        private readonly IProfileItemDtoMapper profileItemDtoMapper;
        private readonly IProfileCategoryDtoMapper profileCategoryDtoMapper;
        private readonly ISkillDtoMapper skillDtoMapper;
        private readonly IGenericItemDtoMapper genericItemDtoMapper;
        private readonly IPortfolioFileDtoMapper portfolioFileDtoMapper;
        private readonly ISkillCategoryDtoMapper skillCategoryDtoMapper;
        private readonly ICompanySkillDtoMapper companySkillDtoMapper;

        public ProfileController(IProfileItemService profileItemService, IProfileService profileService, ILookupProfileService lookupProfileService, IPortfolioFileService portfolioFileService, IProfileItemDtoMapper profileItemDtoMapper, IProfileCategoryDtoMapper profileCategoryDtoMapper, ISkillDtoMapper skillDtoMapper, IGenericItemDtoMapper genericItemDtoMapper, IPortfolioFileDtoMapper portfolioFileDtoMapper, ISkillCategoryDtoMapper skillCategoryDtoMapper, ICompanySkillDtoMapper companySkillDtoMapper)
        {
            this.profileItemService = profileItemService;
            this.profileService = profileService;
            this.lookupProfileService = lookupProfileService;
            this.portfolioFileService = portfolioFileService;
            this.profileItemDtoMapper = profileItemDtoMapper;
            this.profileCategoryDtoMapper = profileCategoryDtoMapper;
            this.skillDtoMapper = skillDtoMapper;
            this.genericItemDtoMapper = genericItemDtoMapper;
            this.portfolioFileDtoMapper = portfolioFileDtoMapper;
            this.skillCategoryDtoMapper = skillCategoryDtoMapper;
            this.companySkillDtoMapper = companySkillDtoMapper;
        }

        [HttpGet]
        [Route("{userId}")]
        public Profile GetUserProfile(int userId)
        {
            var result = new Profile();
            var userProfileItems = profileItemService.GetUserProfile(userId);
            List<Model.Dto.ProfileCategoryDto> profileCategories = profileService.GetProfileCategories().Select(x => profileCategoryDtoMapper.ToDto(x)).ToList();
            foreach (var profileCategory in profileCategories)
            {
                foreach (var profileItem in userProfileItems)
                {
                    var item = profileItemDtoMapper.ToDto(profileItem);
                    if (profileItem.ProfileCategory.Id == profileCategory.Id)
                    {
                        if (profileItem.ProfileCategory.Id == (int)Model.Enum.ProfileCategory.Portfolio)
                        {
                            var listPortfolioFile = portfolioFileService.GetPortfolioFiles(profileItem.Id).Select(x => portfolioFileDtoMapper.ToDto(x)).ToList();
                            item.PortfolioFiles = listPortfolioFile;
                        }
                        profileCategory.Items.Add(item);
                    }
                }
            }
            result.ProfileCategories = profileCategories;
            result.Lookup.Languages = lookupProfileService.GetActiveLanguages().Select(x => genericItemDtoMapper.ToDto(x)).ToList();
            return result;
        }

        [HttpGet]
        [Route("skills/top-skills/{userId}/{topX}")]
        public List<Model.Dto.ProfileItemDto> GetTopSkills(int userId, int topX)
        {
            List<Model.Dto.ProfileItemDto> topSkills = profileItemService.GetTopSkills(userId, topX).Select(x => profileItemDtoMapper.ToDto(x)).ToList();
            return topSkills;
        }

        [HttpGet]
        [Route("skills/all-skill")]
        public List<Model.Dto.SkillDto> GetSkills()
        {
            List<Model.Dto.SkillDto> skills = profileService.GetSkills().Select(x => skillDtoMapper.ToDto(x)).ToList();
            return skills;
        }

        [HttpGet]
        [Route("skills/company-skill")]
        public List<Model.Dto.CompanySkillDto> GetCompanySkills()
        {
            List<Model.Dto.CompanySkillDto> companySkills = profileItemService.GetCompanySkills().Select(x => companySkillDtoMapper.ToDto(x)).ToList();
            return companySkills;
        }

        [HttpGet]
        [Route("skillCategories/all-skill-category")]
        public List<Model.Dto.SkillCategoryDto> GetSkillCategories()
        {
            List<Model.Dto.SkillCategoryDto> skillCats = profileService.GetSkillCategories().Select(x => skillCategoryDtoMapper.ToDto(x)).ToList();
            return skillCats;
        }

        [HttpGet]
        [Route("skills/company-skill/{skillId}")]
        public List<Model.Dto.ProfileItemDto> GetProfileItemsBySkillId(int skillId)
        {
            List<Model.Dto.ProfileItemDto> profileItems = profileItemService.GetProfileItemsBySkillId(skillId).Select(x => profileItemDtoMapper.ToDto(x)).ToList();
            return profileItems;
        }

        [HttpPost]
        [Route("ProfileItem")]
        public void AddProfileItem(Model.Dto.ProfileItemDto profileItem)
        {
            profileItemService.AddProfileItem(profileItemDtoMapper.ToDomain(profileItem));
        }

        [HttpPut]
        [Route("ProfileItem")]
        public void UpdateProfileItem(Model.Dto.ProfileItemDto profileItem)
        {
            profileItemService.UpdateProfileItem(profileItemDtoMapper.ToDomain(profileItem));
        }

        [HttpDelete]
        [Route("ProfileItem/{itemId}")]
        public void DeleteProfileItem(int itemId)
        {
            profileItemService.DeleteProfileItem(itemId);
        }

        [HttpPost]
        [Route("ProfileItem/PortfolioFile")]
        public Model.Dto.PortfolioFileDto AddPortfolioFile(Model.Dto.PortfolioFileDto portfolioFileDto)
        {
            var newPortfolioFile = portfolioFileService.AddPortfolioFile(portfolioFileDtoMapper.ToDomain(portfolioFileDto));
            return portfolioFileDtoMapper.ToDto(newPortfolioFile);
        }

        [HttpPut]
        [Route("ProfileItem/PortfolioFile")]
        public Model.Dto.PortfolioFileDto UpdatePortfolioFile(Model.Dto.PortfolioFileDto portfolioFileDto)
        {
            var updatePortfolioFile = portfolioFileService.UpdatePortfolioFile(portfolioFileDtoMapper.ToDomain(portfolioFileDto));
            return portfolioFileDtoMapper.ToDto(updatePortfolioFile);
        }

        [HttpDelete]
        [Route("ProfileItem/PortfolioFile/{itemId}")]
        public int DeletePortfolioFile(int itemId)
        {
            portfolioFileService.DeletePortfolioFile(itemId);
            return itemId;
        }
    }
}