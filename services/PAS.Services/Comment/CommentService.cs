using PAS.Model.Dto;
using PAS.Model.Mapping;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services.Comment
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ITaskRepository _taskRepsitory;
        private readonly ITasksSecurityService _taskSecurityService;
        private readonly IApplicationContext _applicationContext;
        private readonly IEmailService _emailService;

        public CommentService(ICommentRepository commentRepository, ITaskRepository taskRepsitory, ITasksSecurityService taskSecurityService, IApplicationContext applicationContext, IEmailService emailService)
        {
            _commentRepository = commentRepository;
            _taskRepsitory = taskRepsitory;
            _taskSecurityService = taskSecurityService;
            _applicationContext = applicationContext;
            _emailService = emailService;
        }

        public bool CreateComment(Model.Comment newComment)
        {
            var existedTask = _taskRepsitory.GetTaskById(newComment.TaskId);
            if (existedTask is null)
                return false;
            if (!_taskSecurityService.HasPermissionToCommentOnTask(existedTask, _applicationContext.CurrentUser))
            {
                return false;
            }
            if (newComment.Content.Length <= 0)
                return false;
            newComment.CreatedBy = _applicationContext.CurrentUser.Id;
            newComment.ModifiedDate = DateTime.Now;
            newComment.CreatedDate = DateTime.Now;

            if (_commentRepository.CreateComment(newComment))
            {
                var currentUser = _applicationContext.CurrentUser;
                List<Model.User> relatedUsers = new List<Model.User>();
                existedTask.AssignedUsers.ToList().ForEach(user =>
                {
                    if(user.Id != currentUser.Id)
                        relatedUsers.Add(user);
                });
                if(existedTask.CreatedBy != null)
                {
                    if (existedTask.CreatedBy.Id != currentUser.Id && !relatedUsers.Contains(existedTask.CreatedBy, new UserComparer()))
                    {
                        relatedUsers.Add(existedTask.CreatedBy);
                    }
                }
              
                if(existedTask.Story != null)
                {
                    if(!relatedUsers.Contains(existedTask.Story.User, new UserComparer()) && existedTask.Story.User.Id != currentUser.Id)
                    {
                        relatedUsers.Add(existedTask.Story.User);
                    }
                }
                foreach (var comment in existedTask.Comments)
                {
                    if (!relatedUsers.Contains(comment.User, new UserComparer()) && comment.User.Id != currentUser.Id)
                    {
                        relatedUsers.Add(comment.User);
                    }
                }
                _emailService.TaskNewCommentAdded(relatedUsers, existedTask.Title, newComment.Content, existedTask.Id);
                return true;
            }
            return false;
        }
       
        public bool DeleteComment(int commentId)
        {
            var existedComment = _commentRepository.GetCommentById(commentId);
            if (existedComment is null)
                return false;
            if (!HasPermissionToEditComment(existedComment.CreatedBy))
            {
                return false;
            }
            return _commentRepository.DeleteComment(commentId);
        }

        public IEnumerable<Model.Comment> GetComments(int taskId)
        {
            var existedTask = this._taskRepsitory.GetTaskById(taskId);
            if (existedTask is null)
                return new List<Model.Comment>();
            if (!_taskSecurityService.HasPermissionToCommentOnTask(existedTask, _applicationContext.CurrentUser))
                return new List<Model.Comment>();
            var comments = this._commentRepository.GetCommentsByTaskId(taskId);
            return comments;
           
        }

        public bool UpdateComment(Model.Comment comment)
        {
            var existedComment = _commentRepository.GetCommentById(comment.Id);
            if (existedComment is null)
                return false;
            if (comment.Content == "" || comment.Content.Trim().Length == 0)
                return false;
            if (!HasPermissionToEditComment(existedComment.CreatedBy))
            {
                return false;
            }
            existedComment.Content = comment.Content;
            existedComment.ModifiedDate = DateTime.Now;
            return _commentRepository.UpdateComment(existedComment);
        }
        private bool HasPermissionToEditComment(int commentOwner)
        {
            if (commentOwner != _applicationContext.CurrentUser.Id)
                return false;
            return true;
        }
    }
    class UserComparer : IEqualityComparer<Model.User>
    {
        public bool Equals(Model.User x, Model.User y)
        {
            return x.Id == y.Id ? true : false;
        }

        public int GetHashCode(Model.User obj)
        {
            return obj.GetHashCode();
        }
    }
}
