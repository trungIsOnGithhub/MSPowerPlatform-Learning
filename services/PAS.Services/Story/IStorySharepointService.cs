using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IStorySharepointService
    {
        Task<Model.Story> GetStory(Model.Story storyModel);
        Task<List<Model.Dto.Story>> GetOldStories(int userId);
        Task<int> CreateStory(Model.Story storyModel);
        Model.Story UpdateStory(Model.Story storyModel);
        bool DeleteOldStory(int storySPId);
    }
}
