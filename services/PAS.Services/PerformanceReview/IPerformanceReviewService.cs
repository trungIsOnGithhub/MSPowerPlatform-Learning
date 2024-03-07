using System;
using System.Collections.Generic;

namespace PAS.Services
{
    public interface IPerformanceReviewService
    {
        Model.Domain.PerformanceReviewPeriod GetCurrentReviewPeriod();
        Model.Domain.UserPerformanceReview GetCurrentUserPerformanceReviewPeriod(int periodId);
        IEnumerable<Model.Domain.PerformanceReviewAndUserResult> GetPerformanceReviewAndUserResultOfProject(int projectId, int periodId);
        IEnumerable<Model.Domain.PerformanceReviewAndUserResult> UpdatePerformanceReviewAndUseResult(int projectId, int periodId, IEnumerable<Model.Domain.PerformanceReviewAndUserResult> dtos);
        IEnumerable<Model.Domain.PerformanceReviewPeriod> GetAllPeriods();
        IEnumerable<Model.Domain.Project> GetLastestProjectSettings(int periodId);
        Model.Domain.PerformanceReviewPeriod GetLastestPeriod();
        Model.Domain.PerformanceReviewPeriod GetPeriodById(int periodId);
        IEnumerable<Model.User> GetRelevantUsers(int periodId);
        IEnumerable<Model.Domain.Project> GetProjectsOfUsersInPeriod(int userId, int periodId);
        IEnumerable<Model.Domain.PerformanceReviewAndUserResult> GetResultOfUserInProjectAndPeriod(int userId, int periodId, int projectId);
        IEnumerable<Model.Domain.EvaluationReviewOptions> GetEvaluationReviewOptions();
        bool FinalizePerformanceReview(int userId, int periodId, string comment, int? optionId);
        Model.Domain.PerformanceReviewPeriod GetPeriodByIdWithUserPerformanceReviews(int periodId);
        bool CreatePerformanceReviewPeriod(IEnumerable<Model.Domain.Project> projects, Model.Domain.PerformanceReviewPeriod period, IEnumerable<int> excludedUsers);
        IEnumerable<int> GetExcludedUsers(int periodId);
        bool RemovePerformanceReviewPeriod(int periodId);
        bool UpdatePerformanceReviewPeriod(Model.Domain.PerformanceReviewPeriod period, IEnumerable<int> excludedUsers);
    }
}