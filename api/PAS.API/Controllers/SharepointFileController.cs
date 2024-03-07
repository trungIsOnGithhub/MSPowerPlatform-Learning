using System.Web.Http;
using PAS.Services;
using PAS.Model.Mapping;
using System.Threading.Tasks;
using System.Collections.Generic;
using PAS.Model.Dto;
using System.Linq;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/sharepointFile")]
    public class SharepointFileController : ApiController
    {
        private readonly ISharepointFileService sharepointFileService;
        private readonly ISharepointImageDtoMapper sharepointImageDtoMapper;

        public SharepointFileController(ISharepointFileService sharepointFileService, ISharepointImageDtoMapper sharepointImageDtoMapper)
        {
            this.sharepointFileService = sharepointFileService;
            this.sharepointImageDtoMapper = sharepointImageDtoMapper;
        }

        [Route("getAllFolderFiles")]
        [HttpGet]
        public async Task<List<SharepointImageDto>> GetAllFolderFiles(System.Uri folderRelativeUrl)
        {
            if(folderRelativeUrl != null)
            {
                var result = await sharepointFileService.GetAllFolderFiles(folderRelativeUrl.ToString()).ConfigureAwait(false);
                return result.Select(x => sharepointImageDtoMapper.ToDto(x)).ToList();
            }
            return null;
        }

        [Route("readSharepointFile")]
        [HttpGet]
        public async Task<SharepointImageDto> ReadSharepointFile(System.Uri fileRelativeUrl)
        {
            if (fileRelativeUrl != null)
            {
                var result = await sharepointFileService.ReadSharepointFile(fileRelativeUrl.ToString()).ConfigureAwait(false);
                return sharepointImageDtoMapper.ToDto(result);
            }   
            return null;
        }
    }
}