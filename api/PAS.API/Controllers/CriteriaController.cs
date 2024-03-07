using PAS.Model.Dto;
using PAS.Model.Mapping;
using PAS.Services;
using PAS.Services.Criterion;
using PAS.API.ErrorHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/criteria")]
    public class CriteriaController : BaseController
    {
        private readonly ICriterionServices _criterionServices;
        private readonly IOptionListService _optionListService;
        private readonly ICriteriaGroupDtoMapper _criteriaGroupMapper;
        private readonly ICriteriaOptionListDtoMapper _criteriaOptionListDtoMapper;
        private readonly IEvaluationReviewOptionsService _evaluationOptionService;
        private readonly IEvaluationReviewOptionsDtoMapper _evaluationReviewOptionsDtoMapper;
        private readonly ICriteriaOptionMapper _criteriaOptionMapper;
        private readonly ICriteriaDtoMapper _criteriaDtoMapper;
        private readonly ApplicationContext _context;

        public CriteriaController(IPermissionService permissionService, ICriterionServices criterionServices, IOptionListService optionListService, ICriteriaGroupDtoMapper criteriaGroupMapper, ICriteriaOptionListDtoMapper criteriaOptionListDtoMapper, IEvaluationReviewOptionsService evaluationOptionService, IEvaluationReviewOptionsDtoMapper evaluationReviewOptionsDtoMapper, IPerformanceReviewService performanceReviewService, ICriteriaOptionMapper criteriaOptionMapper, ICriteriaDtoMapper criteriaDtoMapper, ApplicationContext context) : base(permissionService)
        {
            _criterionServices = criterionServices;
            _optionListService = optionListService;
            _criteriaGroupMapper = criteriaGroupMapper;
            _criteriaOptionListDtoMapper = criteriaOptionListDtoMapper;
            _evaluationOptionService = evaluationOptionService;
            _evaluationReviewOptionsDtoMapper = evaluationReviewOptionsDtoMapper;
            _criteriaOptionMapper = criteriaOptionMapper;
            _criteriaDtoMapper = criteriaDtoMapper;
            _context = context;
        }

        [Route("group")]
        [HttpPost]
        public bool CreateCriteriaGroup([FromBody] CriteriaGroupDto dto)
        {
            CheckAdminPagePermission();
            return _criterionServices.AddCriteriaGroup(_criteriaGroupMapper.ToDomain(dto));
        }

        [HttpPost]
        [Route("criteria-item")]
        public bool CreateCriteria([FromBody] CriteriaDto dto)
        {
            CheckAdminPagePermission();
            return _criterionServices.AddCriteria(_criteriaDtoMapper.ToDomain(dto));
        }

        [Route("group")]
        [HttpGet]
        public IEnumerable<CriteriaGroupDto> GetGroups()
        {
            CheckAdminPagePermission();
            return _criteriaDtoMapper.ToDtos(_criterionServices.GetGroups());
        }
        [Route("option-list")]
        [HttpPost]
        public bool CreateOptionList([FromBody] CriteriaOptionListDto dto)
        {
            CheckAdminPagePermission();
            return this._optionListService.CreateOptionList(_criteriaOptionListDtoMapper.ToDomain(dto));
        }
        [Route("option-list")]
        [HttpPut]
        public bool UpdateOptionList([FromBody] CriteriaOptionListDto dto)
        {
            CheckAdminPagePermission();
            return this._optionListService.UpdateList(_criteriaOptionListDtoMapper.ToDomain(dto));
        }
        [Route("option-list/{optionListId}/addOption")]
        [HttpPost]
        public bool AddOptionToOptionList(int optionListId, [FromBody] CriteriaOptionsDto dto)
        {
            CheckAdminPagePermission();
            if (dto == null)
            {
                throw new ArgumentNullException(null, "No option list provided.");
            }
            if (dto.CreatedBy == default(int))
            {
                dto.CreatedBy = _context.CurrentUser.Id;
            }
            return _optionListService.AddOptionToList(optionListId, _criteriaOptionMapper.ToDomain(dto));
        }

        [Route("option-list/{optionListId}/deleteOption/{optionId}")]
        [HttpDelete]
        public bool DeleteOptionFromOptionList(int optionListId, int optionId)
        {
            CheckAdminPagePermission();
            return _optionListService.RemoveOptionFromList(optionListId, new Model.Domain.CriteriaOptions
            {
                Id = optionId
            });
        }

        [Route("option-list")]
        [HttpGet]
        public IEnumerable<CriteriaOptionListDto> GetOptionList()
        {
            CheckAdminPagePermission();
            return _criteriaOptionListDtoMapper.ToDtos(this._optionListService.GetOptionList());
        }

        [Route("option-list/options")]
        [HttpGet]
        public IEnumerable<CriteriaOptionsDto> GetOptions()
        {
            CheckAdminPagePermission();
            return _criteriaOptionMapper.ToDtos(this._optionListService.GetOptions());
        }

        [Route("option-list/options/{optionId}")]
        [HttpPut]
        public CriteriaOptionsDto UpdateOption(int optionId, [FromBody] CriteriaOptionsDto dto)
        {
            CheckAdminPagePermission();
            var result = _optionListService.UpdateOption(optionId, _criteriaOptionMapper.ToDomain(dto));
            return result != null ? _criteriaOptionMapper.ToDto(result) : null;
        }

        [Route("evaluation-review-option")]
        [HttpGet]
        public IEnumerable<EvaluationReviewOptionsDto> GetEvaluationReviewOptions()
        {
            CheckAdminPagePermission();
            return _evaluationReviewOptionsDtoMapper.ToDtos(_evaluationOptionService.GetEvaluationReviewOptions());
        }
        [HttpPost]
        [Route("evaluation-review-option")]
        public bool CreateEvaluationOption([FromBody] IEnumerable<EvaluationReviewOptionsDto> evaluationOptionDto)
        {
            CheckAdminPagePermission();
            return _evaluationOptionService.CreateEvluationReviewOption(_evaluationReviewOptionsDtoMapper.ToDomains(evaluationOptionDto));
        }

        [HttpDelete]
        [Route("option-list/{id}")]
        public bool DeleteOptionList(int id)
        {
            CheckAdminPagePermission();
            return this._optionListService.RemoveOptionList(id);
        }
        [HttpPut]
        [Route("evaluation-review-option/{optionId}")]
        public bool UpdateEvaluationReviewOption(int optionId, [FromBody] EvaluationReviewOptionsDto evaluationOptionDto)
        {
            CheckAdminPagePermission();
            if (evaluationOptionDto == null)
            {
                throw new ArgumentNullException(null, "No evaluation body provided.");
            }

            evaluationOptionDto.Id = optionId;
            return _evaluationOptionService.UpdateEvaluationReviewOption(_evaluationReviewOptionsDtoMapper.ToDomain(evaluationOptionDto));
        }

        [HttpPut]
        [Route("criteria-item")]
        public bool UpdateCriteria([FromBody] Model.Dto.CriteriaDto criteria)
        {
            CheckAdminPagePermission();
            return _criterionServices.UpdateCriteria(_criteriaDtoMapper.ToDomain(criteria));
        }

        [HttpDelete]
        [Route("delete/{criteriaId}")]
        public bool DeleteCriteria(int criteriaId)
        {
            CheckAdminPagePermission();
            return _criterionServices.DeleteCriteria(criteriaId);
        }
    }
}
