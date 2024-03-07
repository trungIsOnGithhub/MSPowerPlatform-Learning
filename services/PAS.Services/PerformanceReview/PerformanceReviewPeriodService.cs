using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public partial class PerformanceReviewService
    {
        public IEnumerable<Model.Domain.PerformanceReviewPeriod> GetAllPeriods()
        {
            return _performanceReviewPeriod.GetAll().OrderByDescending(x => x.ReviewStartDate);
        }
        public IEnumerable<Model.Domain.Project> GetLastestProjectSettings(int periodId)
        {
            var result = _projectServices.GetProjectCriterionAtPeriod(periodId);
            if(result is null || result.Count() == 0)
            {
                return _projectServices.GetProjectAndCriterion();
            }
            return result;
        }
        public Model.Domain.PerformanceReviewPeriod GetLastestPeriod()
        {
            return _performanceReviewPeriod.GetLatestPerformanceReviewPeriod();
        }
        public Model.Domain.PerformanceReviewPeriod GetPeriodById(int periodId)
        {
            return _performanceReviewPeriod.GetById(periodId);
        }
        public Model.Domain.PerformanceReviewPeriod GetPeriodByIdWithUserPerformanceReviews(int periodId)
        {
            return _performanceReviewPeriod.GetPeriodWithUserReviewAndResult(periodId);
        }
    }
}
