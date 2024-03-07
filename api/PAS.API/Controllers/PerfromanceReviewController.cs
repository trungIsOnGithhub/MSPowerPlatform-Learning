using PAS.Model.Domain;
using PAS.Model.Dto;
using PAS.Model.Enum;
using PAS.Model.Mapping;
using PAS.Services;
using PAS.Services.Criterion;
using PAS.Services.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/PerfromanceReview")]
    public class PerfromanceReviewController : BaseController
    {
        private const string MESSAGE_InvalidValue = "Invalid value";
        private readonly IPerformanceReviewService _performanceReviewService;
        private readonly IPerformanceReviewResultDtoMapper _performanceReviewResultDtoMapper;
        private readonly IProjectDtoMapper _projectDtoMapper;
        private readonly IPerformanceReviewPeriodDtoMapper _performanceReviewPeriodDtoMapper;
        private readonly IUserDtoMapper _userMapper;
        private readonly IEvaluationReviewOptionsDtoMapper _evaluationReviewOptionsDtoMapper;
        private readonly IProjectServices _projectServices;
        private readonly ICriteriaDtoMapper _criteriaDtoMapper;
        private readonly ApplicationContext _context;
        private readonly IUserService _userService;
        private readonly ICriterionServices _criterionServices;

        public PerfromanceReviewController(IPermissionService permissionService, IPerformanceReviewService performanceReviewService, IPerformanceReviewResultDtoMapper performanceReviewResultDtoMapper, IProjectDtoMapper projectDtoMapper, IPerformanceReviewPeriodDtoMapper performanceReviewPeriodDtoMapper, IUserDtoMapper userMapper, IEvaluationReviewOptionsDtoMapper evaluationReviewOptionsDtoMapper, IProjectServices projectServices, ICriteriaDtoMapper criteriaDtoMapper, ApplicationContext context, IUserService userService, ICriterionServices criterionServices) : base(permissionService)
        {
            _performanceReviewService = performanceReviewService;
            _performanceReviewResultDtoMapper = performanceReviewResultDtoMapper;
            _projectDtoMapper = projectDtoMapper;
            _performanceReviewPeriodDtoMapper = performanceReviewPeriodDtoMapper;
            _userMapper = userMapper;
            _evaluationReviewOptionsDtoMapper = evaluationReviewOptionsDtoMapper;
            _projectServices = projectServices;
            _criteriaDtoMapper = criteriaDtoMapper;
            _context = context;
            _userService = userService;
            _criterionServices = criterionServices;
        }

        [Route("current-period")]
        [HttpGet]
        public PerformanceReviewPeriodDto GetCurrentPeriod()
        {
            var result = this._performanceReviewService.GetCurrentReviewPeriod();
            if (result is null)
                return null;
            return this._performanceReviewPeriodDtoMapper.ToDto(result);
        }

        [Route("{periodId}/review-form")]
        [HttpGet]
        public PerformanceReviewFormDto GetPerformanceReviewDetail(int periodId)
        {
            var currentUserPerformanceReview = this._performanceReviewService.GetCurrentUserPerformanceReviewPeriod(periodId);
            if (currentUserPerformanceReview == null || currentUserPerformanceReview.Projects == null || !currentUserPerformanceReview.Projects.Any())
            {
                return null;
            }
            var result = new PerformanceReviewFormDto()
            {
                DefaultProject = new ProjectPerformanceReviewResult()
                {
                    PeriodId = periodId,
                    ProjectId = currentUserPerformanceReview.Projects.FirstOrDefault().Id,
                    DefaultProjectResult = this._performanceReviewResultDtoMapper.ToDtos(_performanceReviewService.GetPerformanceReviewAndUserResultOfProject(currentUserPerformanceReview.Projects.FirstOrDefault().Id, periodId))
                },
                RelatedProjects = _projectDtoMapper.ToDtos(currentUserPerformanceReview.Projects)
            };

            List<int> projectIds = new List<int>
            {
                currentUserPerformanceReview.Projects.FirstOrDefault().Id
            };
            var projectCriterion = _criterionServices.GetProjectCriterion(projectIds);
            result.DefaultProject.DefaultProjectResult = result.DefaultProject.DefaultProjectResult.Aggregate(new List<PerformanceReviewResultDto>(), (refinedList, obj) =>
             {
                 var criterion = projectCriterion.Where(x => x.ProjectId.Equals(currentUserPerformanceReview.Projects.FirstOrDefault().Id) && x.CriteriaId.Equals(obj.Criteria.Id)).FirstOrDefault();
                 if (criterion != null)
                 {
                     obj.Criteria.SortOrder = criterion.SortOrder;
                     refinedList.Add(obj);
                 }
                 return refinedList;
             });
            result.DefaultProject.DefaultProjectResult = result.DefaultProject.DefaultProjectResult.OrderBy(x => x.Criteria.SortOrder);
            return result;
        }

        [Route("{periodId}/{projectId}/review-result")]
        [HttpGet]
        public IEnumerable<PerformanceReviewResultDto> GetPerformanceReviewResultsOfProject(int projectId, int periodId)
        {
            List<int> projectIds = new List<int>
            {
                projectId
            };
            var projectCriterion = _criterionServices.GetProjectCriterion(projectIds);
            var result = _performanceReviewResultDtoMapper.ToDtos(_performanceReviewService.GetPerformanceReviewAndUserResultOfProject(projectId, periodId));
            result = result.Aggregate(new List<PerformanceReviewResultDto>(), (refinedList, obj) =>
            {
                var criterion = projectCriterion.Where(x => x.ProjectId.Equals(projectId) && x.CriteriaId.Equals(obj.Criteria.Id)).FirstOrDefault();
                if(criterion != null)
                {
                    obj.Criteria.SortOrder = criterion.SortOrder;
                    refinedList.Add(obj);
                }
                return refinedList;
            });
            return result.OrderBy(x => x.Criteria.SortOrder);
        }

        [Route("{periodId}/{projectId}/review-result")]
        [HttpPut]
        public bool UpdatePerformanceReviewResult(int periodId, int projectId, [FromBody] IEnumerable<PerformanceReviewResultDto> dtos)
        {
            _performanceReviewService.UpdatePerformanceReviewAndUseResult(projectId, periodId, _performanceReviewResultDtoMapper.ToDomains(dtos));
            return true;
        }

        [HttpGet]
        [Route("review-period-form")]
        public PerformanceReviewPeriodFormDto GetPerformanceReviewForm()
        {
            CheckAdminPagePermission();
            return new PerformanceReviewPeriodFormDto()
            {
                ProjectSettings = _projectDtoMapper.ToDtos(_performanceReviewService.GetLastestProjectSettings(-1)),
                AllPeriods = _performanceReviewPeriodDtoMapper.ToDtos(_performanceReviewService.GetAllPeriods().OrderByDescending(x => x.EvaluationEndDate))
            };
        }
        [HttpGet]
        [Route("{periodId}/project-settings")]
        public PerformanceReviewPeriodFormDto GetPerformanceReviewForm(int periodId)
        {
            CheckAdminPagePermission();
            return new PerformanceReviewPeriodFormDto()
            {
                Header = _performanceReviewPeriodDtoMapper.ToDto(_performanceReviewService.GetPeriodById(periodId)),
                ProjectSettings = _projectDtoMapper.ToDtos(_performanceReviewService.GetLastestProjectSettings(periodId)),
                ExcludedUsers = _performanceReviewService.GetExcludedUsers(periodId)
            };
        }

        [HttpGet]
        [Route("result")]
        public ManagerFormDto GetManagerForm()
        {
            CheckManagerPagePermission();
            var allPeriods = _performanceReviewService.GetAllPeriods();
            var relevantUsers = _performanceReviewService.GetRelevantUsers(allPeriods.FirstOrDefault().Id);
            relevantUsers = relevantUsers.Aggregate(new List<Model.User>(), (newList, obj) =>
            {
                obj.TechnicalLevel = _userService.GetCurrentTechnicalLevel(obj.Id)?.careerPathStepTitle;
                newList.Add(obj);
                return newList;
            }).OrderBy(x => x.TeamId).ThenBy(x => x.Name);
            return new ManagerFormDto()
            {
                EvaluationReviewOptions = _evaluationReviewOptionsDtoMapper.ToDtos(_performanceReviewService.GetEvaluationReviewOptions()),
                Users = _userMapper.ToDtos(relevantUsers),
                CurrentUser = relevantUsers.Any() ? _userMapper.ToDto(relevantUsers.FirstOrDefault()) : null,
                DefaultUserPerformanceResult = relevantUsers.Any() ? PrivateGetUserPerformanceResult(relevantUsers.FirstOrDefault().Id, allPeriods.FirstOrDefault().Id, allPeriods) : null
            };
        }
        [HttpGet]
        [Route("result/{userId}")]
        public ManagerFormDto GetManagerForm(int userId)
        {
            CheckManagerPagePermission();
            var allPeriods = _performanceReviewService.GetAllPeriods();
            var relevantUsers = _performanceReviewService.GetRelevantUsers(allPeriods.FirstOrDefault().Id);
            return new ManagerFormDto()
            {
                CurrentUser = _userMapper.ToDto(relevantUsers.FirstOrDefault(u => u.Id == userId)),
                Users = _userMapper.ToDtos(relevantUsers),
                EvaluationReviewOptions = _evaluationReviewOptionsDtoMapper.ToDtos(_performanceReviewService.GetEvaluationReviewOptions()),
                DefaultUserPerformanceResult = PrivateGetUserPerformanceResult(userId, allPeriods.FirstOrDefault().Id, allPeriods)
            };
        }
        [HttpGet]
        [Route("result/{userId}/{periodId}")]
        public UserPerformanceResultDto GetUserPerformanceResult(int userId, int periodId)
        {
            CheckManagerPagePermission();
            var allPeriods = _performanceReviewService.GetAllPeriods();
            return PrivateGetUserPerformanceResult(userId, periodId, allPeriods);
        }

        protected UserPerformanceResultDto PrivateGetUserPerformanceResult(int userId, int periodId, IEnumerable<PerformanceReviewPeriod> allPeriods)
        {
            var currentPeriod = allPeriods.Where(x => x.Id.Equals(periodId)).FirstOrDefault();
            var currentUserReview = currentPeriod.UserPerformanceReviews.Where(x => x.UserId.Equals(userId)).FirstOrDefault();
            return new UserPerformanceResultDto()
            {
                AllPeriods = _performanceReviewPeriodDtoMapper.ToDtos(allPeriods),
                CurrentPeriod = _performanceReviewPeriodDtoMapper.ToDto(currentPeriod),
                EvaluationReviewOption = currentUserReview?.EvaluationReviewOption != null ? _evaluationReviewOptionsDtoMapper.ToDto(currentUserReview.EvaluationReviewOption) : null,
                PerformanceResultDetail = PrivateGetUserPerformanceResult(userId, periodId, -1),
                EvaluationReviewComment = currentUserReview?.Comment
            };
        }

        [HttpGet]
        [Route("result/{userId}/{periodId}/{projectId}")]
        public UserPerformanceResultDetailDto GetUserPerformanceResult(int userId, int periodId, int projectId)
        {
            CheckManagerPagePermission();
            return PrivateGetUserPerformanceResult(userId, periodId, projectId);
        }

        protected UserPerformanceResultDetailDto PrivateGetUserPerformanceResult(int userId, int periodId, int projectId)
        {
            var projects = _performanceReviewService.GetProjectsOfUsersInPeriod(userId, periodId).Where(x => !x.IsIgnored);
            if (projectId == -1)
            {
                if (projects.Any())
                {
                    projectId = projects.FirstOrDefault().Id;
                }
                else
                {
                    var fakeProject = new ProjectDto { Id = -1, Name = "N/A" };
                    return new UserPerformanceResultDetailDto()
                    {
                        Projects = new List<ProjectDto>() { fakeProject },
                        CurrentProject = fakeProject,
                        DefaultReviewResult = new List<PerformanceReviewResultDto>()
                    };
                }
            }
            var relevantResults = _performanceReviewService.GetResultOfUserInProjectAndPeriod(userId, periodId, projectId);
            var projectCriterion = _criterionServices.GetProjectCriterion(projects.Select(x => x.Id)).ToList();
            var currentProject = projects.Where(x => x.Id.Equals(projectId)).FirstOrDefault();
            var result = new UserPerformanceResultDetailDto()
            {
                Projects = _projectDtoMapper.ToDtos(projects),
                CurrentProject =  _projectDtoMapper.ToDto(_projectServices.GetProjectDetail(projectId)),
                DefaultReviewResult = _performanceReviewResultDtoMapper.ToDtos(relevantResults)
            };
            result.DefaultReviewResult = result.DefaultReviewResult.AsParallel().Aggregate(new List<PerformanceReviewResultDto>(), (refinedList, obj) =>
            {
                obj.Criteria.SortOrder = projectCriterion.Where(x => x.ProjectId.Equals(obj.ProjectId) && x.CriteriaId.Equals(obj.CriteriaId)).FirstOrDefault().SortOrder;
                refinedList.Add(obj);
                return refinedList;
            });
            result.DefaultReviewResult = result.DefaultReviewResult.OrderBy(y => y.Criteria.SortOrder);
            return result;
        }

        [HttpGet]
        [Route("has-right-to-view-manager-page")]
        public bool HasRight()
        {
            return base.permissionService.HasRightToViewManagerPage();
        }


        [HttpPut]
        [Route("result/{periodId}/{userId}/evaluate")]
        public bool FinalizePerformanceReview(int periodId, int userId, [FromBody] FinalEvaluationDto dto)
        {
            CheckManagerPagePermission();
            return _performanceReviewService.FinalizePerformanceReview(userId, periodId, dto?.Comment, dto?.EvaluationOptionId);
        }


        [HttpGet]
        [Route("result-summarization")]
        public EvaluationSummarizationDto GetSummarization()
        {
            CheckManagerPagePermission();
            var currentUser = _context.CurrentUser;
            IEnumerable<Project> relevantProjects = _projectServices.GetRelevantProjects(currentUser);
            return new EvaluationSummarizationDto()
            {
                Projects = _projectDtoMapper.ToDtos(relevantProjects),
                CurrentProject = _projectDtoMapper.ToDto(relevantProjects.FirstOrDefault()),
                CurrentProjectSummarization = PrivateGetProjectSummarization(-1, relevantProjects.FirstOrDefault().Id)
            };
        }

        [HttpGet]
        [Route("result-summarization/{projectId}")]
        public EvaluationSummarizationDto GetEvaluationSummarization(int projectId)
        {
            CheckManagerPagePermission();
            var project = _projectServices.GetProjects(projectId).FirstOrDefault();
            return new EvaluationSummarizationDto()
            {
                CurrentProject = _projectDtoMapper.ToDto(project),
                CurrentProjectSummarization = PrivateGetProjectSummarization(-1, project.Id)
            };
        }
        [HttpGet]
        [Route("result-summarization/{projectId}/{periodId}")]
        public ProjectSummarizationDto GetProjectSummarization(int periodId, int projectId)
        {
            CheckManagerPagePermission();
            return PrivateGetProjectSummarization(periodId, projectId);
        }

        protected ProjectSummarizationDto PrivateGetProjectSummarization(int periodId, int projectId)
        {
            var periods = _performanceReviewService.GetAllPeriods();
            if (periodId == -1)
            {
                periodId = periods.FirstOrDefault().Id;
            }
            return new ProjectSummarizationDto()
            {
                CurrentPeriod = _performanceReviewPeriodDtoMapper.ToDto(periods.Where(x => x.Id.Equals(periodId)).FirstOrDefault()),
                Periods = _performanceReviewPeriodDtoMapper.ToDtos(periods),
                UsersSummarization = GetUsersSummarization(periodId, projectId)
            };
        }

        protected IEnumerable<UsersSummarizationDto> GetUsersSummarization(int periodId, int projectId)
        {
            List<int> projectIds = new List<int>();
            projectIds.Add(projectId);
            var projectCriteria = _criterionServices.GetProjectCriterion(projectIds);
            var relevantUsers = _performanceReviewService.GetRelevantUsers(periodId).OrderBy(x => x.Id).AsParallel().Select(x => x.Id);
            var currentPeriod = _performanceReviewService.GetPeriodByIdWithUserPerformanceReviews(periodId);
            var userSummarizationList = new List<UsersSummarizationDto>();
            var projectAndUser = _projectServices.GetProjectAndUsers();
            var result = currentPeriod.UserPerformanceReviews.Aggregate(userSummarizationList, (newList, obj) =>
            {
                if (obj.PerformanceReviewAndUserResults.FirstOrDefault(ur => ur.ProjectId == projectId) == null || !relevantUsers.Contains(obj.UserId)
                || _context.CurrentUser.Id.Equals(obj.UserId)
                || projectAndUser.Where(x => x.UserId.Equals(obj.UserId) && x.ProjectId.Equals(projectId)).FirstOrDefault() == null)
                {
                    return newList;
                }
                var addedItem = new UsersSummarizationDto()
                {
                    Comment = obj.Comment,
                    UserPerformanceReviewTechnicalLevel = _userService.GetCurrentTechnicalLevel(obj.UserId)?.careerPathStepTitle,
                    FinalEvaluation = obj.EvaluationReviewOption is null ? null : obj.EvaluationReviewOption.Name,
                    FinalEvaluationId = obj.EvaluationReviewOption?.Id,
                    Id = obj.Id,
                    UserPerformanceReviewName = obj.UserName,
                    UserSummarizationDetails = new List<UsersSummarizationDetailDto>()
                };
                var refinedSummarizationDetail = obj.PerformanceReviewAndUserResults.AsParallel().Where(x => x.ProjectId == projectId && obj.UserId != x.ReviewerId).Aggregate(new List<UsersSummarizationDetailDto>(), (refinedList, p) =>
                {
                    var existedCrit = refinedList.Where(x => x.Criteria.Id.Equals(p.CriteriaId)).FirstOrDefault();
                    if (existedCrit is null)
                    {
                        if (p.Criteria.TypeId.Equals((int)CriteriaTypes.Choices))
                        {
                            int countOfAnswer = 0;
                            decimal averageScore = 0;
                            if (p.CriteriaOptionsAnswer != null)
                            {
                                if (p.CriteriaOptionsAnswer.Score > 0)
                                {
                                    averageScore = p.CriteriaOptionsAnswer.Score;
                                    countOfAnswer = 1;
                                }
                            }
                            var newCrit = new UsersSummarizationDetailDto()
                            {
                                TextAnswers = new List<TextQuestionAnswerDto>(),
                                AverageScore = averageScore,
                                CountOfAnswer = countOfAnswer,
                                Criteria = _criteriaDtoMapper.ToDto(p.Criteria)
                            };
                            newCrit.Criteria.SortOrder = projectCriteria.Where(x => x.ProjectId.Equals(projectId) && x.CriteriaId.Equals(newCrit.Criteria.Id)).FirstOrDefault().SortOrder;
                            refinedList.Add(newCrit);
                        }
                        else if (p.Criteria.TypeId.Equals((int)CriteriaTypes.Ranking))
                        {
                            var newSum = new UsersSummarizationDetailDto()
                            {
                                AverageScore = p.RankNumber.HasValue ? p.RankNumber.Value : 0,
                                CountOfAnswer = p.RankNumber.HasValue ? 1 : 0,
                                Criteria = _criteriaDtoMapper.ToDto(p.Criteria),
                            };
                            newSum.Criteria.SortOrder = projectCriteria.Where(x => x.ProjectId.Equals(projectId) && x.CriteriaId.Equals(newSum.Criteria.Id)).FirstOrDefault().SortOrder;
                            refinedList.Add(newSum);
                        }
                        else
                        {
                            var sum = new UsersSummarizationDetailDto()
                            {
                                TextAnswers = new List<TextQuestionAnswerDto>(),
                                AverageScore = 0,
                                CountOfAnswer = 0,
                                Criteria = _criteriaDtoMapper.ToDto(p.Criteria)
                            };
                            sum.Criteria.SortOrder = projectCriteria.Where(x => x.ProjectId.Equals(projectId) && x.CriteriaId.Equals(sum.Criteria.Id)).FirstOrDefault().SortOrder;
                            sum.TextAnswers = sum.TextAnswers.Append(new TextQuestionAnswerDto()
                            {
                                AnswerText = p.CriteriaAnswerText,
                                ReviewerName = p.ReviewerName
                            });
                            refinedList.Add(sum);
                        }
                    }
                    else
                    {
                        if (p.Criteria.TypeId.Equals((int)CriteriaTypes.Choices))
                        {
                            if (p.CriteriaOptionsAnswer != null)
                            {
                                if (p.CriteriaOptionsAnswer.Score > 0)
                                {
                                    existedCrit.AverageScore += p.CriteriaOptionsAnswer.Score;
                                    existedCrit.CountOfAnswer += 1;
                                }
                            }
                        }
                        else if (p.Criteria.TypeId.Equals((int)CriteriaTypes.Ranking))
                        {
                            if (p.RankNumber.HasValue)
                            {
                                existedCrit.AverageScore += p.RankNumber.Value;
                                existedCrit.CountOfAnswer += 1;
                            }
                        }
                        else
                        {
                            existedCrit.TextAnswers = existedCrit.TextAnswers.Append(new TextQuestionAnswerDto()
                            {
                                AnswerText = p.CriteriaAnswerText,
                                ReviewerName = p.ReviewerName
                            });
                        }
                    }
                    return refinedList;
                });
                refinedSummarizationDetail.ForEach(x =>
                {
                    if (x.CountOfAnswer != 0 && x.AverageScore != 0)
                    {
                        x.AverageScore = x.AverageScore / x.CountOfAnswer;
                    }
                });
                addedItem.UserSummarizationDetails = refinedSummarizationDetail;
                newList.Add(addedItem);
                return newList;
            });
            result.ForEach(x =>
            {
                x.UserSummarizationDetails.OrderBy(y => y.Criteria.SortOrder);
            });
            return result;
        }

        [HttpPost]
        [Route("review-period")]
        public bool CreatePerformanceReviewPeriod([FromBody] PerformanceReviewPeriodFormDto dto)
        {
            // TODO Remove projectSettings out of this model
            // Since setting criteria in project have been done in project APIs
            CheckAdminPagePermission();
            if (dto is null)
                throw new ArgumentException(MESSAGE_InvalidValue);
            return _performanceReviewService.CreatePerformanceReviewPeriod(
                _projectDtoMapper.ToDomains(dto.ProjectSettings),
                _performanceReviewPeriodDtoMapper.ToDomain(dto.Header),
                dto.ExcludedUsers
            );
        }

        [HttpGet]
        [Route("has-right-to-view-admin-page")]
        public bool HasRightToViewAdminPage()
        {
            return base.permissionService.HasRightToViewAdminPage();
        }

        [HttpDelete]
        [Route("review-period/{periodId}")]
        public bool DeletePerformanceReviewPeriod(int periodId)
        {
            CheckAdminPagePermission();
            return _performanceReviewService.RemovePerformanceReviewPeriod(periodId);
        }

        [HttpPut]
        [Route("review-period/{periodId}")]
        public bool UpdatePerformanceReviewPeriod(int periodId, [FromBody] PerformanceReviewPeriodFormDto dto)
        {
            CheckAdminPagePermission();
            if (dto is null)
                throw new ArgumentException(MESSAGE_InvalidValue);
            var period = _performanceReviewPeriodDtoMapper.ToDomain(dto.Header);
            period.Id = periodId;
            period.ModifiedDate = DateTime.Now;
            period.ModifiedBy = _context.CurrentUser.Id;
            period.ModifiedByUser = _context.CurrentUser;
            return _performanceReviewService.UpdatePerformanceReviewPeriod(period, dto.ExcludedUsers);
        }
    }
}
