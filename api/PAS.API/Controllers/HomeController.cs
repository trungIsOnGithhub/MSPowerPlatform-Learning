using System.Web.Http;
using PAS.Services;
using PAS.Model.Mapping;
using System.Threading.Tasks;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/home")]
    public class HomeController : ApiController
    {
        private readonly IHomeService _homeService;
        private readonly ISharePointInfoDtoMapper _spInfoDtoMapper;
        public HomeController(IHomeService homeService, ISharePointInfoDtoMapper spInfoDtoMapper)
        {
            _homeService = homeService;
            _spInfoDtoMapper = spInfoDtoMapper;
        }

        [Route("getSharePointInfo")]
        [HttpGet]
        public async Task<Model.Dto.SharePointInfo> GetSharePointInfo()
        {
            var result = await _homeService.GetSharePointInfo().ConfigureAwait(false);
            return _spInfoDtoMapper.ToDto(result);
        }
    }
}