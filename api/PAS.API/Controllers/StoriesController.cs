using PAS.Model.Dto;
using PAS.Model.Mapping;
using PAS.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/stories")]
    public class StoriesController : ApiController
    {
        private IStoryService _storyService;
        private IStoryDtoMapper _mapper;

        public StoriesController(
            IStoryService storyService,
            IStoryDtoMapper mapper)
        {
            _storyService = storyService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<Story> GetStoryByUserId(int id)
        {
            var storyModel = await _storyService.GetStoryByUserId(id).ConfigureAwait(false);
            var dto = _mapper.ToDto(storyModel);
            dto.CreatedAt = dto.ModifiedAt;
            return dto;
        }

        [HttpGet]
        [Route("{id:int}/ByStoryId")]
        public Story GetStoryById(int id)
        {
            var storyModel = _storyService.GetStoryById(id);
            return _mapper.ToDto(storyModel);
        }

        [HttpGet]
        [Route("{userId}/oldstories")]
        public async Task<List<Story>> GetOldStories(int userId)
        {
            var result = await _storyService.GetOldStories(userId).ConfigureAwait(false);
            return result;
        }

        [HttpDelete]
        [Route("{storySPId}")]
        public bool DeleteOldStories(int storySPId)
        {
            return _storyService.DeleteOldStory(storySPId);
        }

        [HttpPost]
        [Route("{userId}")]
        public async Task<Story> Create(int userId, Story story)
        {
            var storyModel = await _storyService.CreateStory(story, userId).ConfigureAwait(false);
            return _mapper.ToDto(storyModel);
        }

        [HttpPut]
        [Route("{id:int}")]
        public Story UpdateStory(int id, Story story)
        {
            var storyModel = _storyService.UpdateStory(id, story);
            return _mapper.ToDto(storyModel);
        }

    }
}
