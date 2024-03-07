namespace PAS.Services
{
    public interface ITaskService
    {
        Model.Dto.TaskListResult GetTaskListing(Model.Dto.Filter filter);
        Model.Task GetTaskById(int id);
        void CreateTask(Model.Dto.Task task);
        Model.Dto.Task CreateSugestion(Model.Dto.Task task);
        void UpdateTask(Model.Dto.Task task);
        bool HasEditPermissionOnTask(int taskId);
        void DeleteTask(int taskId);
        bool UnFollowTask(int taskId);
        bool FollowTask(int taskId);
        Model.Dto.TaskListResult GetTaskNotComplete();
    }
}
