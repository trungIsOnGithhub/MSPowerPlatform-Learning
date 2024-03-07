using PAS.Model.Dto;
using PAS.Model.Enum;
using PAS.Model.Mapping;
using PAS.Services;
using PAS.Services.Comment;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/tasks")]
    public class TasksController : ApiController
    {
        private ITaskDtoMapper _taskMapper;
        private IUserDtoMapper _userMapper;
        private ITaskService _taskService;
        private ICheckListService _checkListService;
        private IEnumService _enumService;
        private IUserService _userService;
        private ICommentService _commentService;
        private readonly ICommentDtoMapper _commentDtoMapper;
        private IApplicationContext _applicationContext;

        public TasksController(
            ITaskDtoMapper taskMapper, 
            IUserDtoMapper userMapper, 
            ITaskService taskService, 
            ICheckListService checkListService, 
            IEnumService enumService, 
            IUserService userService,
            ICommentService commentService,
            ICommentDtoMapper commentDtoMapper,
            IApplicationContext applicationContext)
        {
            _taskMapper = taskMapper;
            _userMapper = userMapper;
            _taskService = taskService;
            _checkListService = checkListService;
            _enumService = enumService;
            _userService = userService;
            _commentService = commentService;
            _commentDtoMapper = commentDtoMapper;
            _applicationContext = applicationContext;
        }

        [HttpGet]
        [Route("{id}")]
        public Task GetTask(int id)
        {
            Model.Task task = _taskService.GetTaskById(id);
            var existedTask = _taskMapper.ToDto(task);
            existedTask.CategoryName = _enumService.ToListItem(typeof(Category)).Where(c => c.Value.Equals(existedTask.Category)).FirstOrDefault().Name;
            existedTask.StatusName = _enumService.ToListItem(typeof(Status)).Where(c => c.Value.Equals(existedTask.Status)).FirstOrDefault().Name;
            return existedTask;
        }

        [HttpGet]
        [Route("~/api/tasks/taskform")]
        public TaskForm GetTaskForm()
        {
            TaskForm result = new TaskForm();
            result.Category = _enumService.ToListItem(typeof(Category)).Where(p=> p.Value != (int)Category.Suggestion).ToList();
            result.Status = _enumService.ToListItem(typeof(Status));

            List<Model.User> users = _userService.GetAllReferenceUsers();
            foreach(Model.User user in users)
            {
                Model.Dto.User dtoObject = _userMapper.ToDto(user);
                result.Users.Add(dtoObject);
            }

            if (_applicationContext.CurrentUser.Role == Role.BOD)
            {
                result.RelatedUsers = result.Users;
            }
            else
            {
                var members = _userService.GetUsersMember(_applicationContext.CurrentUser.Id).ToList();
                members.Add(_applicationContext.CurrentUser);
                foreach (Model.User user in members)
                {
                    Model.Dto.User dtoObject = _userMapper.ToDto(user);
                    if (!result.RelatedUsers.Any(u => u.Id == dtoObject.Id))
                    {
                        result.RelatedUsers.Add(dtoObject);
                    }
                }
            }
            //result.RelatedUsers = result.RelatedUsers.OrderBy(u => u.Name).ToList();
            result.RelatedUsers = result.Users;
            return result;
        }

        [HttpGet]
        [Route("{taskId}/permission")]
        public bool CheckPermisisonOnSuggestion(int taskId)
        {
            var result = _taskService.HasEditPermissionOnTask(taskId);
            return result;
        }

        [HttpPost]
        [Route("listings")]
        public TaskListResult GetListing(Filter filter)
        {
            var data = _taskService.GetTaskListing(filter);
            foreach(var task in data.Data)
            {
                task.HasEditedPermission = _taskService.HasEditPermissionOnTask(task.Id);
            }
            data.Categories = _enumService.ToListItem(typeof(Category));
            data.Status = _enumService.ToListItem(typeof(Status));

            return data;
        }

        [Route("{taskId}/unfollow")]
        [HttpPut]
        public bool UnfollowTask(int taskId)
        {
            return _taskService.UnFollowTask(taskId);
        }        
        [Route("{taskId}/follow")]
        [HttpDelete]
        public bool FollowTask(int taskId)
        {
            return _taskService.FollowTask(taskId);
        }
        [HttpPost]
        public void CreateTask(Task task)
        {
            _taskService.CreateTask(task);
        }

        [HttpPost]
        [Route("suggestion")]
        public Task CreateSuggestion(Task task)
        {
            var newTask = _taskService.CreateSugestion(task);
            return newTask;
        }

        [HttpPut]
        public void UpdateTask(Task task)
        {
            _taskService.UpdateTask(task);
        } 

        [HttpDelete]
        [Route("{taskId}")]
        public void DeleteTask(int taskId)
        {
            _taskService.DeleteTask(taskId);
        }
        [HttpGet]
        [Route("{taskId}/comments")]
        public IEnumerable<CommentDto> GetTaskComments(int taskId)
        {
            return  _commentDtoMapper.ToDtos(this._commentService.GetComments(taskId));
        }
        [HttpPost]
        [Route("comment")]
        public bool CreateComment([FromBody] CommentDto comment)
        {
            var newComment = _commentDtoMapper.ToDomain(comment);
            return this._commentService.CreateComment(newComment);
        }
        [HttpDelete]
        [Route("comment/{commentId}")]
        public bool DeleteComment(int commentId)
        {
            return this._commentService.DeleteComment(commentId);
        }
        [HttpPut]
        [Route("comment")]
        public bool UpdateComment([FromBody] CommentDto comment)
        {
            var editedComment = _commentDtoMapper.ToDomain(comment);
            return this._commentService.UpdateComment(editedComment);
        }
    }
}
