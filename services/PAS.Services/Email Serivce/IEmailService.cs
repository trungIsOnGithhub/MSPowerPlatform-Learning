using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IEmailService
    {
        void StoryAddedNotification(Model.Story addedStory);
        void TaskAddedNotification(Model.Task addedTask);
        void TaskUpdatedNotification(Model.Task updatedTask);
        void FeedbackAddedNotification(Model.Task feedBack, bool iAnonymous);
        void TaskNewCommentAdded(IEnumerable<Model.User> relatedUsers, string taskName, string commentContent, int taskId);
        void TaskReminder(Model.Task task, int numberOfSends, int days);
        void KudoGiveNotification(Model.User receiver, string message);
    };
}
