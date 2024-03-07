using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IStatisticsService
    {
        public Model.Domain.Statistics GetStatisticsByState(DateTime? startDate, DateTime? endDate, bool developersOnly = false);
    }
}
