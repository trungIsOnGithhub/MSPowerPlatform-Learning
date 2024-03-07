namespace PAS.Services
{
    public interface IStorySecurityService
    {
        bool HasPermisisonToCreateStory(Model.User memeber, Model.User author);
        bool HasPermisisonOnStory(Model.User currentUser, Model.User storyOwner, Model.User storyCoach);
    }
}
