using PAS.Model;
using PAS.Model.Enum;

namespace PAS.Services.Story
{
    public class StorySecurityService : IStorySecurityService
    {
        public bool HasPermisisonOnStory(User currentUser, User storyOwner, User storyCoach)
        {
            if (currentUser.Role == Role.BOD || currentUser.Role == Role.Admin)
            {
                return true;
            }

            if (storyCoach != null)
            {
                if (storyCoach.Id == currentUser.Id && (storyOwner.Manager == null || storyOwner.Manager.Id == storyCoach.Id))
                {
                    return true;
                }
            }

            if (storyOwner != null)
            {
                if (storyOwner.Id == currentUser.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasPermisisonToCreateStory(User member, User author)
        {
            bool hasPermission = false;

            if (author.Role == Role.BOD || author.Role == Role.Admin)
            {
                hasPermission = true;
            }

            if (member.Manager != null)
            {
                if (member.Manager.Id == author.Id)
                {
                    hasPermission = true;
                }
            }

            if (member.Id == author.Id)
            {
                hasPermission = true;
            }

            return hasPermission;
        }
    }
}
