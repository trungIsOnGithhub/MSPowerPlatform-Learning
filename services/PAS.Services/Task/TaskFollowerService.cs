using PAS.Repositories;
using System.Collections.Generic;

namespace PAS.Services
{
    public class TaskFollowerService : ITaskFollowerService
    {
        private readonly ITaskUnfollowerRepository _taskUnfollowerRepository;

        public TaskFollowerService(ITaskUnfollowerRepository taskUnfollowerRepository)
        {
            _taskUnfollowerRepository = taskUnfollowerRepository;
        }

        public bool IsFollowingTask(int taskId, int userId)
        {
            return _taskUnfollowerRepository.FindTaskAndUser(new Model.TaskUnfollower()
            {
                TaskId = taskId,
                UserId = userId
            }) == null ? true : false;
        }
        public IEnumerable<Model.TaskUnfollower> GetUsersUnfollowThisTask(int taskId)
        {
            return _taskUnfollowerRepository.GetUsersUnfollowTask(taskId);
        }
    }
}
