using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IStoryService
    {
        Model.Story GetStoryById(int id);
        Task<Model.Story> GetStoryByUserId(int userId);
        Task<List<Model.Dto.Story>> GetOldStories(int userId);
        Task<Model.Story> CreateStory(Model.Dto.Story story, int userId);
        Model.Story UpdateStory(int storyID, Model.Dto.Story storyDto);
        bool DeleteOldStory(int storySPId);
        Model.Story GetStoryLightByUserId(int userId);
    }
}
