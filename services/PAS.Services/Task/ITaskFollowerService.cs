using System.Collections.Generic;

namespace PAS.Services
{
    public interface ITaskFollowerService
    {
        bool IsFollowingTask(int taskId, int userId);
        IEnumerable<Model.TaskUnfollower> GetUsersUnfollowThisTask(int taskId);
    }
}