using PAS.Model;
using System.Linq;

namespace PAS.Services
{
    public interface ITasksSecurityService
    {
        bool HasSetPublicPermissionOnSuggestion(Task task, User currentUser);
        bool HasPermissionsToUpdateTask(Task task, User currentUser);
        bool HasPermisisonToViewTask(Task task, User currentUser);
        bool HasPermissionToCommentOnTask(Task task, User currentUser);
    }

    public class TasksSecurityService : ITasksSecurityService
    {
        public bool HasSetPublicPermissionOnSuggestion(Task task, User currentUser)
        {
            bool hasPermission = false;

            if (currentUser.Role == Model.Enum.Role.BOD)
            {
                hasPermission = true;
            }

            return hasPermission;
        }

        public bool HasPermissionsToUpdateTask(Task task, User currentUser)
        {
            bool hasPermisison = false;

            if (task.Category == Model.Enum.Category.Suggestion)
            {
                if (task.IsPublic == true)
                {
                    if (currentUser.Role == Model.Enum.Role.BOD
                    || task.AssignedUsers.Any(user => user.Id == currentUser.Id))
                    {
                        hasPermisison = true;
                    }
                }
                else 
                {
                    if (currentUser.Role == Model.Enum.Role.BOD 
                        || task.CreatedBy?.Id == currentUser.Id
                    || task.AssignedUsers.Any(user => user.Id == currentUser.Id))
                    {
                        hasPermisison = true;
                    }
                }
            }
            else
            {
                if (currentUser.Role == Model.Enum.Role.BOD 
                    || task.CreatedBy?.Id == currentUser.Id
                    || task.AssignedUsers.Any(user => user.Id == currentUser.Id)
                    || task.Story?.User.Id == currentUser.Id
                    || task.Story?.User.Manager?.Id == currentUser.Id)
                {
                    hasPermisison = true;
                }
            }
            
            return hasPermisison;
        }

        public bool HasPermisisonToViewTask(Task task, User currentUser)
        {
            bool hasPermisison = false;

            if (task.Category == Model.Enum.Category.Suggestion)
            {
                if (currentUser.Role == Model.Enum.Role.BOD || task.IsPublic || task.CreatedBy?.Id == currentUser.Id
                    || task.AssignedUsers.Any(user => user.Id == currentUser.Id))
                {
                    hasPermisison = true;
                }
            }
            else
            {
                if (currentUser.Role == Model.Enum.Role.BOD || task.CreatedBy?.Id == currentUser.Id
                || task.AssignedUsers.Any(user => user.Id == currentUser.Id)
                || task.AssignedUsers.Any(user => user.Manager != null && user.Manager.Id == currentUser.Id)
                    || task.Story?.User.Id == currentUser.Id
                    || task.Story?.User.Manager?.Id == currentUser.Id)
                {
                    hasPermisison = true;
                }
            }

            return hasPermisison;
        }

        public bool HasPermissionToCommentOnTask(Task task, User currentUser)
        {
            bool hasPermission = false;
            if (task.Category == Model.Enum.Category.Suggestion)
                return hasPermission;
            if (currentUser.Role == Model.Enum.Role.BOD || currentUser.Id == task.CreatedBy?.Id 
                || task.AssignedUsers.Any(user => user.Id == currentUser.Id)
                || task.AssignedUsers.Any(user => user.Manager != null && user.Manager.Id == currentUser.Id)
                || task.Story?.User.Id == currentUser.Id
                    || task.Story?.User.Manager?.Id == currentUser.Id)
            {
                hasPermission = true;
            }
            return hasPermission;
        }
    }
}
