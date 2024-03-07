using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PAS.Services;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/statistics")]
    public class StatisticsController : ApiController
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [Route("get")]
        public Model.Domain.Statistics GetStatisticsByState(DateTime? startDate = null, DateTime? endDate = null, bool technicalOnly = false)
        { 
            return _statisticsService.GetStatisticsByState(startDate, endDate, technicalOnly);
        }
    }
}
