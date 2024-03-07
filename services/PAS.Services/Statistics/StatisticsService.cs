using PAS.Repositories;
using System;
using System.Linq;

namespace PAS.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsRepository;

        public StatisticsService(IStatisticsRepository statisticsRepository)
        {
            _statisticsRepository = statisticsRepository;
        }

        public Model.Domain.Statistics GetStatisticsByState(DateTime? startDate, DateTime? endDate, bool developerOnly = false)
        {
            startDate = startDate ?? DateTime.MinValue;
            endDate = endDate ?? DateTime.MaxValue;

            var users = _statisticsRepository
                    .GetEmployees(developerOnly)
                    .Where(u => u.EffectiveDate == null
                        || (u.EffectiveDate != null && u.FirstTerminatedDate == null && u.EffectiveDate <= endDate) ||
                        (u.EffectiveDate != null && u.EffectiveDate >= startDate && u.EffectiveDate <= endDate) ||
                        (u.EffectiveDate != null && u.FirstTerminatedDate != null && u.EffectiveDate <= startDate && u.FirstTerminatedDate >= startDate)
                    );

            return new Model.Domain.Statistics
            {
                users = users,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}
