using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.DependencyInjection;
using PAS.Common;
using PAS.Common.Constants;
using PAS.Common.Utilities;
using PAS.Model.Enum;
using PAS.Repositories;
using PAS.Services.BackgroundTask;
using PEEP.Common.Constants;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web.Hosting;

namespace PAS.Services
{
    public class EmailService : IEmailService
    {
        private ISharepointContextProvider sharepointContextProvider;
        private IApplicationContext applicationContext;
        private IUserRepository userRepository;
        private ITaskFollowerService _taskService;
        private ITaskRepository taskRepository;
        private IServiceProvider serviceProvider;


        public EmailService(ISharepointContextProvider sharepointContextProvider, IApplicationContext applicationContext, IUserRepository userRepository, ITaskFollowerService taskService, ITaskRepository taskRepository, IServiceProvider serviceProvider)
        {
            this.sharepointContextProvider = sharepointContextProvider;
            this.applicationContext = applicationContext;
            this.userRepository = userRepository;
            _taskService = taskService;
            this.taskRepository = taskRepository;
            this.serviceProvider = serviceProvider;
        }

        private void SendEmail(string receiverName, string receiverEmail, NotificationEmail template)
        {
            try
            {
                var currentUser = applicationContext.CurrentUser;
                if (currentUser.Email != receiverEmail)
                {
                    var senderEmail = ConfigurationManager.AppSettings["NotificationEmailSender"];
                    var senderPass = ConfigurationManager.AppSettings["NotificationEmailSenderPass"];
                    var senderName = ConfigurationManager.AppSettings["NotificationEmailSenderName"];
                    ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010_SP1);
                    service.UseDefaultCredentials = false;
                    service.Credentials = new WebCredentials(senderEmail, senderPass);
                    string serviceUrl = "https://outlook.office365.com/ews/exchange.asmx";
                    service.Url = new Uri(serviceUrl);
                    EmailMessage emailMsg = new EmailMessage(service);
                    emailMsg.From = new Microsoft.Exchange.WebServices.Data.EmailAddress(senderName, senderEmail);
                    emailMsg.ToRecipients.Add(new Microsoft.Exchange.WebServices.Data.EmailAddress(receiverName, receiverEmail));
                    emailMsg.Subject = template.subject;
                    // emailMsg.Body = template.htmlContent;
                    emailMsg.Body = new MessageBody(BodyType.HTML, template.htmlContent);

                    emailMsg.Send();
                }
            }
            catch (ServiceRequestException error)
            {
                Console.WriteLine($"[ERROR Send Email]: {error}");
            }
        }
        public void StoryAddedNotification(Model.Story newStory)
        {
            var currentUser = applicationContext.CurrentUser;
            using IServiceScope scope = serviceProvider.CreateScope();
            IBackgroundTaskQueue taskQueue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            taskQueue.EnqueueTask(async (IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken) =>
            {
                string storyLink = ConfigurationManager.AppSettings["HomepageUrl"] + "/story/" + newStory.User.Id + "/1-on-1-meetings";
                NotificationEmail template = (NotificationEmail)NotificationEmailTemplate.STORY_ADDED.Clone();
                template.subject = template.subject.Replace("{{story}}", newStory.Title);
                template.htmlContent = template.htmlContent.ReplaceWithBold("{{user}}", newStory.User.Name);
                template.htmlContent = template.htmlContent.ReplaceWithBold("{{currentuser}}", currentUser.Name);
                template.htmlContent = template.htmlContent.ReplaceWithBold("{{story}}", newStory.Title);
                template.htmlContent = template.htmlContent.Replace("{{story_link}}", storyLink);
                SendEmail(newStory.User.Name, newStory.User.Email, template);
            });
        }

        public void TaskAddedNotification(Model.Task addedTask)
        {
            var currentUser = applicationContext.CurrentUser;
            using IServiceScope scope = serviceProvider.CreateScope();
            IBackgroundTaskQueue taskQueue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            taskQueue.EnqueueTask(async (IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken) =>
            {
                string homepage = ConfigurationManager.AppSettings["HomepageUrl"];
                NotificationEmail template;
                string receiverName;
                string receiverEmail;

                if (addedTask.Category == Model.Enum.Category.Suggestion)
                {
                    var bod = userRepository.GetUsersByRole(Model.Enum.Role.BOD);
                    foreach (var user in bod)
                    {
                        template = (NotificationEmail)NotificationEmailTemplate.SUGGESTION_ADDED.Clone();
                        receiverName = user.Name;
                        receiverEmail = user.Email;
                        BuildTaskContent(addedTask, $"{homepage}/start/{addedTask.Id}/view", template, receiverName, currentUser);
                        SendEmail(receiverName, receiverEmail, template);
                    }
                }
                else
                {
                    var send = new List<string>();
                    // member
                    template = (NotificationEmail)NotificationEmailTemplate.TASK_ADDED_TO_STORY.Clone();
                    receiverName = addedTask.Story.User.Name;
                    receiverEmail = addedTask.Story.User.Email;
                    BuildTaskContent(addedTask, $"{homepage}/start/{addedTask.Id}/view", template, receiverName, currentUser);
                    SendEmail(receiverName, receiverEmail, template);
                    send.Add(receiverEmail);

                    // assigned to
                    var assigned = addedTask.AssignedUsers.Where(p => !send.Contains(p.Email)).ToList().Distinct().ToList();
                    foreach (var user in assigned)
                    {
                        template = (NotificationEmail)NotificationEmailTemplate.TASK_ADDED.Clone();
                        receiverName = user.Name;
                        receiverEmail = user.Email;
                        BuildTaskContent(addedTask, $"{homepage}/start/{addedTask.Id}/view", template, receiverName, currentUser);
                        SendEmail(receiverName, receiverEmail, template);
                        send.Add(receiverEmail);
                    }
                }
            });
        }

        public void TaskUpdatedNotification(Model.Task updatedTask)
        {
            var currentUser = applicationContext.CurrentUser;
            using IServiceScope scope = serviceProvider.CreateScope();
            IBackgroundTaskQueue taskQueue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            taskQueue.EnqueueTask(async (IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken) =>
            {
                string homepage = ConfigurationManager.AppSettings["HomepageUrl"];
                NotificationEmail template;
                string receiverName;
                string receiverEmail = string.Empty;

                if (updatedTask.Category == Model.Enum.Category.Suggestion)
                {
                    if (updatedTask.CreatedBy != null && _taskService.IsFollowingTask(updatedTask.Id, updatedTask.CreatedBy.Id))
                    {
                        template = (NotificationEmail)NotificationEmailTemplate.TASK_UPDATED.Clone();
                        receiverName = updatedTask.CreatedBy.Name;
                        receiverEmail = updatedTask.CreatedBy.Email;
                        BuildTaskContent(updatedTask, $"{homepage}/start/{updatedTask.Id}/view", template, receiverName, currentUser);
                        SendEmail(receiverName, receiverEmail, template);
                    }

                    var assigned = updatedTask.AssignedUsers.Where(p => p.Email != receiverEmail).ToList().Distinct().ToList();
                    foreach (var user in FilterUnfollower(assigned, updatedTask.Id))
                    {
                        template = (NotificationEmail)NotificationEmailTemplate.TASK_UPDATED.Clone();
                        receiverName = user.Name;
                        receiverEmail = user.Email;
                        BuildTaskContent(updatedTask, $"{homepage}/start/{updatedTask.Id}/view", template, receiverName, currentUser);
                        SendEmail(receiverName, receiverEmail, template);
                    }
                }
                else
                {
                    var send = new List<string>();
                    // created by
                    template = (NotificationEmail)NotificationEmailTemplate.TASK_UPDATED.Clone();
                    if (updatedTask.CreatedBy != null)
                    {
                        if (_taskService.IsFollowingTask(updatedTask.Id, updatedTask.CreatedBy.Id))
                        {
                            receiverName = updatedTask.CreatedBy.Name;
                            receiverEmail = updatedTask.CreatedBy.Email;
                            BuildTaskContent(updatedTask, $"{homepage}/start/{updatedTask.Id}/view", template, receiverName, currentUser);
                            SendEmail(receiverName, receiverEmail, template);
                            send.Add(receiverEmail);
                        }
                    }
                    // member
                    if (updatedTask.Story != null)
                    {
                        if (updatedTask.Story.User.Email != updatedTask.CreatedBy?.Email && _taskService.IsFollowingTask(updatedTask.Id, updatedTask.Story.User.Id))
                        {
                            template = (NotificationEmail)NotificationEmailTemplate.TASK_UPDATED.Clone();
                            receiverName = updatedTask.Story.User.Name;
                            receiverEmail = updatedTask.Story.User.Email;
                            BuildTaskContent(updatedTask, $"{homepage}/start/{updatedTask.Id}/view", template, receiverName, currentUser);
                            SendEmail(receiverName, receiverEmail, template);
                            send.Add(receiverEmail);
                        }
                    }
                    var assigned = updatedTask.AssignedUsers.Where(p => !send.Contains(p.Email)).ToList().Distinct().ToList();
                    // assigned to
                    foreach (var user in FilterUnfollower(assigned, updatedTask.Id))
                    {
                        template = (NotificationEmail)NotificationEmailTemplate.TASK_UPDATED.Clone();
                        receiverName = user.Name;
                        receiverEmail = user.Email;
                        BuildTaskContent(updatedTask, $"{homepage}/start/{updatedTask.Id}/view", template, receiverName, currentUser);
                        SendEmail(receiverName, receiverEmail, template);
                        send.Add(receiverEmail);
                    }
                }
            });
        }

        private void BuildTaskContent(Model.Task task, string homepage, NotificationEmail template, string receiverName, Model.User currentUser)
        {
            template.subject = template.subject.Replace("{{task_category}}", MapCategoryTitle(task.Category));
            template.subject = template.subject.Replace("{{task_name}}", task.Title);
            template.htmlContent = template.htmlContent.Replace("{{task_category}}", MapCategoryTitle(task.Category));
            template.htmlContent = template.htmlContent.ReplaceWithBold("{{user}}", receiverName);
            template.htmlContent = template.htmlContent.ReplaceWithBold("{{currentuser}}", currentUser != null ? currentUser.Name : "Anonymous");
            template.htmlContent = template.htmlContent.ReplaceWithBold("{{task_name}}", task.Title);
            template.htmlContent = template.htmlContent.Replace("{{homepage}}", homepage);
        }
        private void BuildCommentNotificationContent(string taskName, string relatedPage, string commentContent, Model.User relatedUser, Model.User commenter, NotificationEmail template)
        {
            template.subject = template.subject.Replace("{{task_name}}", taskName);
            template.htmlContent = template.htmlContent.Replace("{{user}}", relatedUser.Name);
            template.htmlContent = template.htmlContent.Replace("{{task_name}}", taskName);
            template.htmlContent = template.htmlContent.Replace("{{commenter_name}}", commenter.Name);
            template.htmlContent = template.htmlContent.Replace("{{content}}", commentContent);
            template.htmlContent = template.htmlContent.Replace("{{homepage}}", relatedPage);
        }
        private string MapCategoryTitle(Category category)
        {
            if (category == Category.Todo) return "To do";
            if (category == Category.Support_Request) return "Support Request";
            if (category == Category.Suggestion) return "Suggestion";
            if (category == Category.Feedback) return "Feedback";
            return category.ToString();
        }

        public void TaskNewCommentAdded(IEnumerable<Model.User> relatedUsers, string taskName, string commentContent, int taskId)
        {
            var currentUser = this.applicationContext.CurrentUser;
            using IServiceScope scope = serviceProvider.CreateScope();
            IBackgroundTaskQueue taskQueue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            taskQueue.EnqueueTask(async (IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken) =>
            {
                string homepage = ConfigurationManager.AppSettings["HomepageUrl"];
                NotificationEmail template;
                string receiverName;
                string receiverEmail = string.Empty;
                foreach (var user in FilterUnfollower(relatedUsers, taskId))
                {
                    template = (NotificationEmail)NotificationEmailTemplate.COMMENT_ADDED.Clone();
                    receiverName = user.Name;
                    receiverEmail = user.Email;
                    BuildCommentNotificationContent(taskName, $"{homepage}/start/{taskId}/view", commentContent, user, currentUser, template);
                    SendEmail(receiverName, receiverEmail, template);
                }
            });
        }
        public IEnumerable<Model.User> FilterUnfollower(IEnumerable<Model.User> users, int taskId)
        {
            List<Model.User> refinedUsers = new List<Model.User>();
            foreach (var user in users)
            {
                if (_taskService.IsFollowingTask(taskId, user.Id))
                {
                    refinedUsers.Add(user);
                }
            }
            return refinedUsers;
        }

        public void FeedbackAddedNotification(Model.Task feedBack, bool isAnonymous)
        {
            var currentUser = applicationContext.CurrentUser;
            using IServiceScope scope = serviceProvider.CreateScope();
            IBackgroundTaskQueue taskQueue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            taskQueue.EnqueueTask(async (IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken) =>
            {
                string homepage = ConfigurationManager.AppSettings["HomepageUrl"];
                NotificationEmail template;
                string receiverName;
                string receiverEmail;
                var send = new List<string>();
                if (feedBack.Story?.Id != null)
                {
                    template = (NotificationEmail)NotificationEmailTemplate.TASK_ADDED_TO_STORY.Clone();
                    receiverName = feedBack.Story.User.Name;
                    receiverEmail = feedBack.Story.User.Email;
                    BuildTaskContent(feedBack, $"{homepage}/start/{feedBack.Id}/view", template, receiverName, !isAnonymous ? currentUser : null);
                    SendEmail(receiverName, receiverEmail, template);
                    send.Add(receiverEmail);
                }
                var assigned = feedBack.AssignedUsers.Where(p => !send.Contains(p.Email)).ToList().Distinct().ToList();
                foreach (var user in assigned)
                {
                    template = (NotificationEmail)NotificationEmailTemplate.TASK_ADDED.Clone();
                    receiverName = user.Name;
                    receiverEmail = user.Email;
                    BuildTaskContent(feedBack, $"{homepage}/start/{feedBack.Id}/view", template, receiverName, !isAnonymous ? currentUser : null);
                    SendEmail(receiverName, receiverEmail, template);
                    send.Add(receiverEmail);
                }
            });
        }

        private void SendEmailReminder(string receiverName, string receiverEmail, NotificationEmail template)
        {
            var senderEmail = ConfigurationManager.AppSettings["NotificationEmailSender"];
            var senderPass = ConfigurationManager.AppSettings["NotificationEmailSenderPass"];
            var senderName = ConfigurationManager.AppSettings["NotificationEmailSenderName"];
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010_SP1);
            service.UseDefaultCredentials = false;
            service.Credentials = new WebCredentials(senderEmail, senderPass);
            string serviceUrl = "https://outlook.office365.com/ews/exchange.asmx";
            service.Url = new Uri(serviceUrl);
            EmailMessage emailMsg = new EmailMessage(service);
            emailMsg.From = new Microsoft.Exchange.WebServices.Data.EmailAddress(senderName, senderEmail);
            emailMsg.ToRecipients.Add(new Microsoft.Exchange.WebServices.Data.EmailAddress(receiverName, receiverEmail));
            emailMsg.Subject = template.subject;
            emailMsg.Body = template.htmlContent;

            emailMsg.Send();
        }

        public void TaskReminder(Model.Task task, int numberOfSends, int days)
        {
            string homepage = ConfigurationManager.AppSettings["HomepageUrl"];
            NotificationEmail template;
            string receiverName;
            string receiverEmail;

            List<Model.User> users = new List<Model.User>();
            users.AddRange(task.AssignedUsers);
            if (task.CreatedBy != null)
            {
                users.Add(task.CreatedBy);
            }
            if (task.Story != null)
            {
                users.Add(task.Story.User);
            }
            users = users.Where(u => !string.IsNullOrEmpty(u.Email))
                .GroupBy(u => u.Email).Select(u => u.FirstOrDefault()).ToList();

            foreach (var user in users)
            {
                receiverName = user.Name;
                receiverEmail = user.Email;
                if (numberOfSends == NumberOfSend.SendFirstTime)
                {
                    template = (NotificationEmail)NotificationEmailTemplate.REMINDER_TASK.Clone();
                    BuildTaskContentReminder(task, homepage + "/start/" + task.Id + "/view", template, receiverName, days);
                }
                else
                {
                    template = (NotificationEmail)NotificationEmailTemplate.REMINDER_TASK_FINISHED.Clone();
                    BuildTaskContentReminder(task, homepage + "/start/" + task.Id + "/view", template, receiverName, days);
                }
                SendEmailReminder(receiverName, receiverEmail, template);
            }

            taskRepository.UpdateTaskSentEmail(task.Id, numberOfSends);
        }

        private void BuildTaskContentReminder(Model.Task task, string homepage, NotificationEmail template, string receiverName, int days)
        {
            string day_remains = days.ToString();
            template.subject = template.subject.Replace("{{task_category}}", MapCategoryTitle(task.Category));
            template.subject = template.subject.Replace("{{task_name}}", task.Title);
            template.htmlContent = template.htmlContent.Replace("{{task_category}}", MapCategoryTitle(task.Category));
            template.htmlContent = template.htmlContent.ReplaceWithBold("{{user}}", receiverName);
            template.htmlContent = template.htmlContent.ReplaceWithBold("{{task_name}}", task.Title);
            template.htmlContent = template.htmlContent.Replace("{{homepage}}", homepage);
            template.htmlContent = template.htmlContent.Replace("{{day_remain}}", day_remains);
        }

        public void KudoGiveNotification(Model.User receiver, string message)
        {
            var currentUser = applicationContext.CurrentUser;
            using IServiceScope scope = serviceProvider.CreateScope();
            IBackgroundTaskQueue taskQueue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
            taskQueue.EnqueueTask(async (IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken) =>
            {
                NotificationEmail template = (NotificationEmail)NotificationEmailTemplate.KUDO_GIVE.Clone();
                template.htmlContent = template.htmlContent.Replace("{{message}}", message);
                template.htmlContent = template.htmlContent.Replace("{{sender}}", currentUser.Name);
                SendEmail(receiver.Name, receiver.Email, template);
            });
        }
    }
}
