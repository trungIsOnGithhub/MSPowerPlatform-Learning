using PAS.Common;
using PAS.Model.Dto;
using PAS.Model.Enum;
using PAS.Model.Mapping;
using PAS.Repositories;
using PAS.Repositories.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services
{
    public class TaskService : ITaskService
    {
        private ITaskRepository taskRepository;
        private IEmailService emailService;

        private IApplicationContext applicationContext;

        private ITaskDtoMapper taskDtoMapper;
        private ITaskMapper taskMapper;
        private readonly IStoryService _storyService;
        private readonly IUserDtoMapper _userDtoMapper;
        private readonly ITasksSecurityService _tasksSecurityService;
        private readonly ITaskUnfollowerRepository _taskUnfollowerRepository;
        private readonly ITaskFollowerService _taskFollowerService;

        public TaskService(ITaskRepository taskRepository, IEmailService emailService, IApplicationContext applicationContext, ITaskDtoMapper taskDtoMapper, ITaskMapper taskMapper, IStoryService storyService, IUserDtoMapper userDtoMapper, ITasksSecurityService tasksSecurityService, ITaskUnfollowerRepository taskUnfollowerRepository, ITaskFollowerService taskFollowerService)
        {
            this.taskRepository = taskRepository;
            this.emailService = emailService;
            this.applicationContext = applicationContext;
            this.taskDtoMapper = taskDtoMapper;
            this.taskMapper = taskMapper;
            _storyService = storyService;
            _userDtoMapper = userDtoMapper;
            _tasksSecurityService = tasksSecurityService;
            _taskUnfollowerRepository = taskUnfollowerRepository;
            _taskFollowerService = taskFollowerService;
        }

        public Model.Task GetTaskById(int id)
        {
            Model.Task task = taskRepository.GetTaskById(id);
            if (task == null) 
            {
                throw new NotFoundException($"Cannot find Task ID={id}");
            }
            var currentUser = applicationContext.CurrentUser;
            task.IsUserFollowingTask = _taskFollowerService.IsFollowingTask(id, currentUser.Id);
            if (!_tasksSecurityService.HasPermisisonToViewTask(task, currentUser))
            {
                throw new UnauthorizedAccessException($"You do not have permisison to view task ID={id}");
            }
            task.HasEditedPermission = _tasksSecurityService.HasPermissionsToUpdateTask(task, currentUser);
            return task;
        }

        public void CreateTask(Task task)
        {
            Model.Task newTask = taskDtoMapper.ToDomain(task);


            var currentUser = applicationContext.CurrentUser;
            
            if (newTask.Category == Category.Suggestion)
            {
                if (newTask.Status != Status.Planned
                    || newTask.AssignedUsers.Count != 0
                    || !Nullable.Equals<DateTime>(newTask.DueDate, null)
                    || newTask.IsPublic == true)
                {
                   
                    if (currentUser.Role != Role.BOD)
                    {
                        throw new UnauthorizedException("Only BOD can assigne users, set duedate, status for Suggestions.");
                    }
                }
            }
            else if(newTask.Category == Category.Feedback)
            {
                if (!task.Anonymous)
                {
                    if(newTask.Story?.Id is null)
                    {
                        throw new NotFoundException("Couldn't find meeting");
                    }
                    newTask.CreatedBy = currentUser;
                    newTask.ModifiedBy = currentUser;
                }
                else
                {
                    newTask.CreatedBy = null;
                    newTask.ModifiedBy = null;
                }
                newTask.ModifiedDate = DateTime.UtcNow;
                newTask.CreatedDate = DateTime.UtcNow;
                taskRepository.CreateTask(newTask);
                emailService.FeedbackAddedNotification(newTask, task.Anonymous);
            }
            else
            {
                if (newTask.Story?.Id != null)
                {
                    var targetStory = _storyService.GetStoryById(newTask.Story.Id);
                    if (targetStory != null)
                    {
                        // Old logic: only BOD/Coach/ThatUser can create a task for that user
                        //if (currentUser.Role == Role.BOD 
                        //    || targetStory.User.Id == currentUser.Id
                        //    || targetStory.User.Manager?.Id == currentUser.Id)
                        //{

                        //}
                        //else
                        //{
                        //    throw new UnauthorizedException("Don't have permission to create task for this related user.");
                        //}

                        newTask.CreatedBy = applicationContext.CurrentUser;
                        newTask.ModifiedBy = applicationContext.CurrentUser;
                        newTask.CreatedDate = DateTime.UtcNow;
                        newTask.ModifiedDate = DateTime.UtcNow;
                        taskRepository.CreateTask(newTask);
                        emailService.TaskAddedNotification(newTask);
                    }
                    else
                    {
                        throw new NotFoundException("User does not setup correctly!");
                    }
                }
                else
                {
                    throw new BadRequestException("Requires a meeting created");
                }
            }
        }

        public Model.Dto.Task CreateSugestion(Task task)
        {
            var currentUser = applicationContext.CurrentUser;
            var currentUserDto = _userDtoMapper.ToDto(currentUser);

            if (!task.Anonymous)
            {
                task.CreatedBy = currentUserDto;
                task.ModifiedBy = currentUserDto;
            }

            task.CreatedDate = DateTime.UtcNow;
            task.ModifiedDate = DateTime.UtcNow;

            var newTask = taskRepository.CreateSuggestion(task);
            emailService.TaskAddedNotification(taskDtoMapper.ToDomain(newTask));
            return newTask;
        }

        public void UpdateTask(Task task)
        {
            Model.Task updatedTask = taskDtoMapper.ToDomain(task);

            var targetTask = taskRepository.GetTaskById(task.Id);
            if (targetTask == null)
            {
                throw new NotFoundException($"Couldn't find task with ID={updatedTask.Id}");
            }
            updatedTask.IsSended = targetTask.IsSended;
            
            if(updatedTask.Story == null)
            {
                updatedTask.Story = targetTask.Story;
            }

            if (updatedTask.DueDate.HasValue && targetTask.DueDate.HasValue)
            {
                if (DateTime.Compare((DateTime)updatedTask.DueDate, (DateTime)targetTask.DueDate) != 0)
                {
                    updatedTask.IsSended = 0;
                }
            }
            if (updatedTask.DueDate.HasValue && targetTask.DueDate == null)
            {
                 updatedTask.IsSended = 0;               
            }
            
           
            if (targetTask.Category == Model.Enum.Category.Suggestion)
            {
                updatedTask.Category = targetTask.Category;
                if (targetTask.IsPublic != updatedTask.IsPublic
                    || targetTask.AssignedUsers.Where(p => !updatedTask.AssignedUsers.Any(l => p.Id == l.Id)).ToList().Count > 0
                    || updatedTask.AssignedUsers.Where(p => !targetTask.AssignedUsers.Any(l => p.Id == l.Id)).ToList().Count > 0
                    || !Nullable.Equals<DateTime>(targetTask.DueDate, updatedTask.DueDate)
                    || targetTask.Status != updatedTask.Status)
                {
                    var currentUser = applicationContext.CurrentUser;

                    if (_tasksSecurityService.HasPermissionsToUpdateTask(targetTask, currentUser))
                    {
                        updatedTask.ModifiedBy = applicationContext.CurrentUser;
                        updatedTask.ModifiedDate = DateTime.UtcNow;
                        taskRepository.UpdateTask(updatedTask);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException($"Only BOD can assign users, set duedate, status for Suggestions.");
                    }

                    emailService.TaskUpdatedNotification(updatedTask);
                }
                else
                {
                    var currentUser = applicationContext.CurrentUser;
                    if (_tasksSecurityService.HasPermissionsToUpdateTask(targetTask, currentUser))
                    {
                        updatedTask.ModifiedBy = applicationContext.CurrentUser;
                        updatedTask.ModifiedDate = DateTime.UtcNow;
                        taskRepository.UpdateTask(updatedTask);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException($"You don't have permission to update this item");
                    }
                }
            }
            else
            {
                updatedTask.ModifiedBy = applicationContext.CurrentUser;
                updatedTask.ModifiedDate = DateTime.UtcNow;
                var existedTask = this.taskRepository.GetTaskById(updatedTask.Id);
                List<int> removalUsers = new List<int>();
                foreach (var oldAssignedUsers in existedTask.AssignedUsers)
                {
                    var isAssigned = false;
                    foreach(var newAssignedUsers in updatedTask.AssignedUsers)
                    {
                        if(newAssignedUsers.Id.Equals(oldAssignedUsers.Id))
                        {
                            isAssigned = true;
                            break;
                        }
                    }
                    if (!isAssigned)
                    {
                        removalUsers.Add(oldAssignedUsers.Id);
                    }
                }
                taskRepository.UpdateTask(updatedTask);
                foreach(var user in removalUsers)
                {
                    if(!this._taskFollowerService.IsFollowingTask(updatedTask.Id, user))
                    {
                        this._taskUnfollowerRepository.FollowATask(new Model.TaskUnfollower()
                        {
                            TaskId = updatedTask.Id,
                            UserId = user
                        });
                    }
                }
                emailService.TaskUpdatedNotification(updatedTask);
            }

        }

        public TaskListResult GetTaskListing(Filter filter)
        {
            var model = new Model.TaskList();


            var currentUser = applicationContext.CurrentUser;
            var tasks = taskRepository.GetTaskListing(filter).ToList();
            
            if (currentUser == null)
            {
                throw new Exception("Cannot find current user");
            }
            if (currentUser.Role != Role.BOD)
            {
                tasks = tasks.Where(p => _tasksSecurityService.HasPermisisonToViewTask(p, currentUser)).ToList();
            }

            var result = new TaskListResult()
            {
                Data = tasks,
                TotalCount = tasks.Count, 
            };

            return result;
        }

        public bool HasEditPermissionOnTask(int taskId)
        {
            var currentUser = applicationContext.CurrentUser;
            var task = taskRepository.GetTaskById(taskId);
            if (task != null)
            {
                var hasPermisison = _tasksSecurityService.HasPermissionsToUpdateTask(task, currentUser);
                return hasPermisison;
            }
            else
            {
                throw new NotFoundException($"Cannot find task ID={taskId}");
            }
        }

        public void DeleteTask(int taskId)
        {
            taskRepository.DeleteTask(taskId);
        }

        public bool UnFollowTask(int taskId)
        {
            var existedTask = taskRepository.GetTaskById(taskId);
            if (existedTask is null)
                return false;
            if (!_tasksSecurityService.HasPermisisonToViewTask(existedTask, applicationContext.CurrentUser))
                return false;
            return _taskUnfollowerRepository.AddUnfollower(new Model.TaskUnfollower()
            {
                TaskId = taskId,
                UserId = this.applicationContext.CurrentUser.Id
            });
        }

        public bool FollowTask(int taskId)
        {
            return this._taskUnfollowerRepository.FollowATask(new Model.TaskUnfollower()
            {
                TaskId = taskId,
                UserId = this.applicationContext.CurrentUser.Id
            });
        }

        public TaskListResult GetTaskNotComplete()
        {
            var tasks = taskRepository.GetTaskNotComplete().ToList();

            TaskListResult result = new TaskListResult();

            if (tasks != null)
            {
                result = new TaskListResult()
                {
                    Data = tasks,
                    TotalCount = tasks.Count,
                };
            }
            return result;           
        }
    }
}
