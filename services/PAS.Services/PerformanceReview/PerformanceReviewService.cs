using PAS.Model;
using PAS.Model.Domain;
using PAS.Model.Enum;
using PAS.Repositories;
using PAS.Services.Criterion;
using PAS.Services.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public partial class PerformanceReviewService : IPerformanceReviewService
    {
        private readonly IPerformanceReviewPeriodRepository _performanceReviewPeriod;
        private readonly IProjectServices _projectServices;
        private readonly IApplicationContext _context;
        private readonly IUserService _userService;
        private readonly IEvaluationReviewOptionsRepository _evaluationReviewOptionsRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICriterionServices _criterionServices;

        public PerformanceReviewService(IPerformanceReviewPeriodRepository performanceReviewPeriod, IProjectServices projectServices, IApplicationContext context, IUserService userService, IEvaluationReviewOptionsRepository evaluationReviewOptionsRepository, IUserRepository userRepository, ICriterionServices criterionServices)
        {
            _performanceReviewPeriod = performanceReviewPeriod;
            _projectServices = projectServices;
            _context = context;
            _userService = userService;
            _evaluationReviewOptionsRepository = evaluationReviewOptionsRepository;
            _userRepository = userRepository;
            _criterionServices = criterionServices;
        }

        public PerformanceReviewPeriod GetCurrentReviewPeriod()
        {
            var latest = _performanceReviewPeriod.GetLatestPerformanceReviewPeriod();
            if (latest is null)
                return null;
            if (latest.IsValid())
            {
                return latest;
            }
            return null;
        }

        public UserPerformanceReview GetCurrentUserPerformanceReviewPeriod(int periodId)
        {
            var userId = _context.CurrentUser.Id;
            var latestPeriod = _performanceReviewPeriod.GetPerformanceReviewPeriodWithUserReviewForCurrentUser(periodId, userId);
            if (latestPeriod is null || !latestPeriod.IsValid())
            {
                throw new Exception("Invalid performance review period");
            }

            var userReview = latestPeriod.UserPerformanceReviews.Where(u => u.UserId.Equals(userId)).FirstOrDefault();
            if (userReview is null)
            {
                return null;
            }

            userReview.Projects = _projectServices.GetProjectOfUserInPeriod(userId, latestPeriod.ReviewStartDate, latestPeriod.ReviewEndDate).Select(project =>
            {
                project.Users = new List<ProjectAndUsers>();
                return project;
            });

            return userReview;
        }
        public IEnumerable<Model.Domain.PerformanceReviewAndUserResult> GetPerformanceReviewAndUserResultOfProject(int projectId, int periodId)
        {
            var userId = _context.CurrentUser.Id;
            var existedProject = _projectServices.GetProjectById(projectId);
            if (existedProject is null)
                throw new Exception("Invalid project");
            var period = _performanceReviewPeriod.GetByIdWithoutAnyRelations(periodId);
            List<ProjectAndUsers> colleges = GetColleges(existedProject, userId, period);
            var queryUserIds = colleges.Select(c => c.UserId).ToList();
            queryUserIds.Add(userId);
            queryUserIds = queryUserIds.Distinct().ToList();
            var latestPeriod = _performanceReviewPeriod.GetPerformanceReviewPeriodWithUserReviewForCurrentReviewer(periodId, queryUserIds);
            if (!latestPeriod.IsValid())
                throw new Exception("Invalid period");
            var performanceReviewAndResult = latestPeriod.UserPerformanceReviews.SelectMany(x => x.PerformanceReviewAndUserResults.Where(x => x.ReviewerId.Equals(userId)));

            if (performanceReviewAndResult.Where(x => x.ProjectId.Equals(projectId)).Count() == 0)
            {
                return GeneratePerformanceReviewAndUserResult(existedProject, userId, latestPeriod);
            }
            var allSamples = GeneratePerformanceReviewAndUserResult(existedProject, userId, latestPeriod);
            var refinedResult = performanceReviewAndResult.Where(x => x.ProjectId.Equals(projectId) && x.ReviewerId.Equals(userId));
            return allSamples.AsParallel().Aggregate(new List<Model.Domain.PerformanceReviewAndUserResult>(), (newList, obj) =>
            {
                var existedResult = refinedResult.Where(x => x.CriteriaId.Equals(obj.CriteriaId) && x.UserPerformanceReviewId.Equals(obj.UserPerformanceReviewId)).FirstOrDefault();
                if (existedResult is null)
                {
                    newList.Add(obj);
                }
                else
                {
                    existedResult.NumberOfColleges = obj.NumberOfColleges;
                    existedResult.Criteria = obj.Criteria;
                    newList.Add(existedResult);
                }
                return newList;
            });
        }
        public IEnumerable<Model.Domain.PerformanceReviewAndUserResult> GeneratePerformanceReviewAndUserResult(Project fromProject, int userId, PerformanceReviewPeriod performanceReviewPeriod)
        {
            var currentUserPeriod = fromProject.Users.Where(x => x.UserId.Equals(userId)).FirstOrDefault();
            List<ProjectAndUsers> colleges = GetColleges(fromProject, userId, performanceReviewPeriod);
            var numberOfColleges = colleges.Where(x => performanceReviewPeriod.UserPerformanceReviews.Select(y => y.UserId).Contains(x.UserId)).Count();
            return fromProject.ProjectCriterion.AsParallel().Aggregate(new List<Model.Domain.PerformanceReviewAndUserResult>(), (newList, obj) =>
            {
                if (!obj.IsActive)
                    return newList;
                return colleges.Aggregate(newList, (newList, col) =>
                {
                    if (col.IsBeingIgnoredByOthers)
                        return newList;
                    var performanceReview = performanceReviewPeriod.UserPerformanceReviews.Where(x => x.UserId.Equals(col.UserId)).FirstOrDefault();
                    if (performanceReview is null)
                    {
                        return newList;
                    }
                    newList.Add(new PerformanceReviewAndUserResult()
                    {
                        CriteriaAnswerText = "",
                        CriteriaOptionAnswerId = null,
                        CriteriaId = obj.CriteriaId,
                        ReviewerId = userId,
                        ProjectId = fromProject.Id,
                        CreatedDate = DateTime.Now,
                        UserPerformanceReviewId = performanceReview.Id,
                        Criteria = obj,
                        NumberOfColleges = numberOfColleges,
                        UserPerformanceReviewName = col.UserName
                    });
                    return newList;
                });
            });
        }

        private static List<ProjectAndUsers> GetColleges(Project fromProject, int userId, PerformanceReviewPeriod period)
        {

            ProjectAndUsers currentUserPeriodLatestVersion = fromProject.Users.Where(x => x.UserId == userId && x.EndDate == null)
                                                                            .FirstOrDefault();
            if(currentUserPeriodLatestVersion == null)
            {
                currentUserPeriodLatestVersion = fromProject.Users.Where(x => x.UserId == userId)
                                                                            .OrderBy(x => x.EndDate)
                                                                            .LastOrDefault();
            }

            return fromProject.Users.AsParallel().Aggregate(new List<ProjectAndUsers>(), (newList, obj) =>
            {
                if(currentUserPeriodLatestVersion.UserId == obj.UserId)
                {
                    if (currentUserPeriodLatestVersion.Id == obj.Id)
                        newList.Add(obj);
                }
                else
                {
                    if (!currentUserPeriodLatestVersion.EndDate.HasValue)
                    {
                        if (!obj.EndDate.HasValue || obj.EndDate >= period.ReviewStartDate)
                        {
                            newList.Add(obj);
                        }
                    }
                    else
                    {
                        if (currentUserPeriodLatestVersion.EndDate.Value >= obj.StartDate)
                        {
                            if (!obj.EndDate.HasValue || obj.EndDate >= period.ReviewStartDate)
                            {
                                newList.Add(obj);
                            }
                        }
                    }
                    
                }
                return newList;

            });
        }

        public IEnumerable<PerformanceReviewAndUserResult> UpdatePerformanceReviewAndUseResult(int projectId, int periodId, IEnumerable<PerformanceReviewAndUserResult> dtos)
        {
            var currentUserId = _context.CurrentUser.Id;
            List<PerformanceReviewAndUserResult> updatedResult = new List<PerformanceReviewAndUserResult>();
            var latestPeriod = _performanceReviewPeriod.GetLatestPerformanceReviewPeriod();
            if (!latestPeriod.IsValid() || latestPeriod.Id != periodId)
                throw new Exception("Invalid period");

            var existedProject = _projectServices.GetProjectByIdWithUsers(projectId);
            var colleges = GetColleges(existedProject, currentUserId, latestPeriod);
            var queryUserIds = colleges.Select(c => c.UserId).ToList();
            queryUserIds.Add(currentUserId);
            queryUserIds = queryUserIds.Distinct().ToList();
            var periodDetail = _performanceReviewPeriod.GetPerformanceReviewPeriodWithUserReviewForCurrentReviewer(latestPeriod.Id, queryUserIds);

            var performanceReivewerIds = dtos.Aggregate(new List<int>(), (newList, obj) =>
            {
                if (obj.UserPerformanceReviewId.Equals(default(int)))
                    throw new Exception("Invalid user performance review id");
                if (obj.CriteriaId.Equals(default(int)))
                    throw new Exception("Invalid criteria id");
                newList.Add(obj.UserPerformanceReviewId);
                return newList;
            });
            List<int> projectIds = new List<int>();
            projectIds.Add(projectId);
            var projectCriterion = _criterionServices.GetProjectCriterion(projectIds);
            var nonExistedCriteria = dtos.Aggregate(new List<int>(), (list, obj) =>
            {
                if (projectCriterion.Where(x => x.ProjectId.Equals(projectId) && x.CriteriaId.Equals(obj.CriteriaId)).FirstOrDefault() == null)
                {
                    list.Add(obj.CriteriaId);
                }
                return list;
            });
            if (nonExistedCriteria.Count != 0)
                throw new Exception("Invalid criterion for this project");

            periodDetail.UserPerformanceReviews.Where(x => performanceReivewerIds.Contains(x.Id)).Select(x =>
            {
                if (colleges.Where(y => y.UserId.Equals(x.UserId)).FirstOrDefault() is null)
                    throw new Exception("Invalid college to be reviewed");
                return x;
            });
            var existedResult = periodDetail.UserPerformanceReviews.SelectMany(x => x.PerformanceReviewAndUserResults).Where(x => x.ProjectId.Equals(projectId) && x.ReviewerId.Equals(currentUserId)).ToList();
            if (existedResult.Count == 0 || existedResult is null)
            {
                dtos = dtos.Where(x => !string.IsNullOrEmpty(x.CriteriaAnswerText) || x.CriteriaOptionAnswerId.HasValue || x.RankNumber.HasValue);
                var refinedResult = dtos.Aggregate(new List<PerformanceReviewAndUserResult>(), (newList, obj) =>
                {
                    if (obj.Criteria.TypeId.Equals(CriteriaTypes.Choices))
                    {
                        obj.CreatedDate = DateTime.Now;
                        obj.ReviewerId = currentUserId;
                        newList.Add(obj);
                    }
                    else
                    {
                        obj.CreatedDate = DateTime.Now;
                        obj.ReviewerId = currentUserId;
                        newList.Add(obj);
                    }
                    return newList;
                });
                _performanceReviewPeriod.AddPerformanceReviewResults(refinedResult);
                updatedResult.AddRange(refinedResult);
            }
            else
            {
                dtos = dtos.Aggregate(new List<PerformanceReviewAndUserResult>(), (refinedList, obj) =>
                {
                    if (existedResult.Where(y => y.CriteriaId.Equals(obj.CriteriaId) && y.ProjectId.Equals(obj.ProjectId) && y.UserPerformanceReviewId.Equals(obj.UserPerformanceReviewId)).Any())
                    {
                        refinedList.Add(obj);
                    }
                    else if (!string.IsNullOrEmpty(obj.CriteriaAnswerText) || obj.CriteriaOptionAnswerId.HasValue || obj.RankNumber.HasValue)
                    {
                        refinedList.Add(obj);
                    }
                    return refinedList;
                });
                List<PerformanceReviewAndUserResult> updatedList = new List<PerformanceReviewAndUserResult>();
                var addedList = dtos.Aggregate(new List<PerformanceReviewAndUserResult>(), (addList, obj) =>
                {
                    if (obj.ReviewerId != currentUserId)
                        return addList;
                    var existedItem = existedResult.Where(x => x.CriteriaId.Equals(obj.CriteriaId) && x.UserPerformanceReviewId.Equals(obj.UserPerformanceReviewId)).FirstOrDefault();
                    if (existedItem != null)
                    {
                        obj.ModifiedDate = DateTime.Now;
                        obj.CreatedDate = existedItem.CreatedDate;
                        obj.ReviewerId = currentUserId;
                        obj.CriteriaId = existedItem.CriteriaId;
                        obj.ProjectId = existedItem.ProjectId;
                        if (obj.CriteriaAnswerText != existedItem.CriteriaAnswerText || obj.CriteriaOptionAnswerId != existedItem.CriteriaOptionAnswerId)
                        {
                            updatedList.Add(obj);
                        }
                    }
                    else
                    {
                        obj.ReviewerId = currentUserId;
                        obj.CreatedDate = DateTime.Now;
                        obj.ProjectId = projectId;
                        addList.Add(obj);
                    }
                    return addList;
                });
                _performanceReviewPeriod.AddPerformanceReviewResults(addedList);
                _performanceReviewPeriod.UpdateExistedPerfomanceReview(updatedList);
                updatedResult.AddRange(addedList);
                updatedResult.AddRange(updatedList);
            }
            if (_performanceReviewPeriod.UnitOfWork.SaveEntities())
            {
                return updatedResult;
            }
            return new List<PerformanceReviewAndUserResult>();
        }       

        public IEnumerable<User> GetRelevantUsers(int periodId)
        {
            var userRole = _context.CurrentUser.Role;
            var period = _performanceReviewPeriod.GetByIdWithoutAnyRelations(periodId);
            var result = new List<User>();
            if (userRole.Equals(Role.BOD))
            {
                var relevantUser = _userService.GetUsersWithPerformanceReviews().Where(x => x.Id != _context.CurrentUser.Id && x.UserPerformanceReviews.Where(x => x.PerformanceReivewPeriodId == periodId).Any()
                 && x.ProjectAndUsers.Where(y => !y.IsBeingIgnoredByOthers && y.UserId == x.Id).Count() != 0);
                foreach(var user in relevantUser)
                {
                    var projectsInPeriod = _projectServices.GetProjectOfUserInPeriodNotIgnore(user.Id, period.ReviewStartDate, period.ReviewEndDate);
                    if (projectsInPeriod.Any())
                    {
                        result.Add(user);
                    }
                }              
                return result;
            }
            else
            {
                var relevantUser = _userService.GetMembersWithPerformanceReviews(_context.CurrentUser.Id).Where(x => x.ProjectAndUsers.Where(y => y.UserId == x.Id).Any() && x.UserPerformanceReviews.Where(x => x.PerformanceReivewPeriodId == periodId).Any());
                foreach (var user in relevantUser)
                {
                    var projectsInPeriod = _projectServices.GetProjectOfUserInPeriod(user.Id, period.ReviewStartDate, period.ReviewEndDate);
                    if (projectsInPeriod.Any())
                    {
                        result.Add(user);
                    }
                }
                return result;
            }
        }

        public IEnumerable<Project> GetProjectsOfUsersInPeriod(int userId, int periodId)
        {
            var currentPeriod = _performanceReviewPeriod.GetByIdWithoutAnyRelations(periodId);
            if (currentPeriod is null)
            {
                throw new Exception("Invalid period");
            }
            return _projectServices.GetProjectOfUserInPeriod(userId, currentPeriod.ReviewStartDate, currentPeriod.ReviewEndDate);
        }

        public IEnumerable<PerformanceReviewAndUserResult> GetResultOfUserInProjectAndPeriod(int userId, int periodId, int projectId)
        {
            var currentPeriod = _performanceReviewPeriod.GetPeriodWithUserReviewAndResult(periodId);
            if (currentPeriod is null)
                throw new Exception("Period is not existed");
            var userPf = currentPeriod.UserPerformanceReviews.Where(x => x.UserId.Equals(userId)).FirstOrDefault();
            if (userPf is null)
            {
                return new List<PerformanceReviewAndUserResult>();
            }
            return userPf.PerformanceReviewAndUserResults.Where(x => x.ProjectId.Equals(projectId));
        }

        public IEnumerable<EvaluationReviewOptions> GetEvaluationReviewOptions()
        {
            return this._evaluationReviewOptionsRepository.GetAll();
        }

        public bool FinalizePerformanceReview(int userId, int periodId, string comment, int? optionId)
        {
            var existedPeriod = _performanceReviewPeriod.GetById(periodId);
            if (existedPeriod is null)
                throw new Exception("Invalid period");
            if (existedPeriod.IsCompleted)
                throw new Exception("Period has been completed");
            var pr = existedPeriod.UserPerformanceReviews.Where(x => x.UserId.Equals(userId)).FirstOrDefault();
            if (pr is null)
                throw new Exception("Invalid user review");
            pr.Comment = comment;
            pr.EvaluationReviewOptionId = optionId;
            pr.EvaluatedDate = DateTime.Now;
            pr.EvaluatedBy = _context.CurrentUser.Id;
            _performanceReviewPeriod.UpdateUserPerformanceReview(pr);
            return _performanceReviewPeriod.UnitOfWork.SaveEntities();
        }

        public bool CreatePerformanceReviewPeriod(IEnumerable<Project> projects, PerformanceReviewPeriod period, IEnumerable<int> exlucedUsers)
        {
            if (projects is null || period is null)
                throw new Exception("Invalid parameters");
            if (period.EvaluationStartDate > period.EvaluationEndDate ||
                period.ReviewStartDate > period.ReviewEndDate)
                return false;
            var lastesPeriod = _performanceReviewPeriod.GetLatestPerformanceReviewPeriod();
            if (lastesPeriod != null)
                if (lastesPeriod.IsValid())
                    throw new Exception("Another period is active");
            period.CreatedBy = _context.CurrentUser.Id;
            var allUsers = _userService.GetAllReferenceUsers().Where(x => x.Active && !exlucedUsers.Contains(x.Id)).Select(x => x.Id);
            _performanceReviewPeriod.AddPerformanceReviewPeriod(period, allUsers, projects);
            return _performanceReviewPeriod.UnitOfWork.SaveEntities();
        }

        public IEnumerable<int> GetExcludedUsers(int periodId)
        {
            var existedPeriod = _performanceReviewPeriod.GetPeriodWithUserReviewAndResult(periodId);
            if (existedPeriod is null)
                return new List<int>();
            var allUsers = _userService.GetAllReferenceUsers();
            var excludedUserIds = allUsers
                .Where(u => !existedPeriod.UserPerformanceReviews.Any(x => x.UserId == u.Id))
                .Select(u => u.Id);
            return excludedUserIds;
        }

        public bool RemovePerformanceReviewPeriod(int periodId)
        {
            _performanceReviewPeriod.RemovePerformanceReviewPeriod(periodId);
            return _performanceReviewPeriod.UnitOfWork.SaveEntities();
        }

        public bool UpdatePerformanceReviewPeriod(PerformanceReviewPeriod period, IEnumerable<int> excludedUserIds)
        {
            _performanceReviewPeriod.UpdatePerformanceReviewPeriod(period, excludedUserIds);
            return _performanceReviewPeriod.UnitOfWork.SaveEntities();
        }
    }
    public static class PerformanceReviewExtension
    {
        public static bool IsValid(this PerformanceReviewPeriod pf)
        {
            if (!pf.IsCompleted)
            {
                if (DateTime.Now.Date.CompareTo(pf.EvaluationStartDate.Date) >= 0 && DateTime.Now.Date.CompareTo(pf.EvaluationEndDate.Date) <= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}