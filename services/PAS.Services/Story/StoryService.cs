using PAS.Common;
using PAS.Model.Mapping;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class StoryService : IStoryService
    {
        private IStoryRepository _storyRepository;
        
        private IStorySharepointService _storySharepointService;
        private readonly IStorySecurityService _storySecurityService;
        private readonly IUserService _userService;
        private IStoryDtoMapper _storyDtoMapper;
        private readonly IApplicationContext _appContext;
        private readonly IEmailService _emailService;

        public StoryService(
            IStoryRepository storyRepository,
            IStorySharepointService storySharepointService,
            IStoryDtoMapper storyDtoMapper,
            IStorySecurityService storySecurityService,
            IUserService userService,
            IApplicationContext appContext,
            IEmailService emailService
            )
        {
            _storyRepository = storyRepository;
            _storySharepointService = storySharepointService;
            _storyDtoMapper = storyDtoMapper;
            _storySecurityService = storySecurityService;
            _userService = userService;
            _appContext = appContext;
            _emailService = emailService;
        }

        public Model.Story GetStoryById(int id)
        {
            var story = _storyRepository.GetStoryById(id);
            return story;
        }

        public Model.Story GetStoryLightByUserId(int userId)
        {
            return _storyRepository.GetStoryByUserId(userId);
        }

        public async Task<Model.Story> GetStoryByUserId(int userId)
        {
            var story = _storyRepository.GetStoryByUserId(userId);
            var currentUser = _appContext.CurrentUser;
            if (story == null)
            {
                var user = _userService.GetUserById(userId);
                if (user == null)
                {
                    throw new NotFoundException($"Coudn't not find user id={userId}");
                }
                story = new Model.Story
                {
                    Id = 0,
                    HasPermission = _storySecurityService.HasPermisisonOnStory(currentUser, user, user.Manager)
                };
            }
            else
            {
                var storyOwner = _userService.GetUserById(story.User.Id);
                story.HasPermission = _storySecurityService.HasPermisisonOnStory(currentUser, storyOwner, storyOwner.Manager);
                
                // When changing coach for a user
                // Check if Manager of latest meeting in SharePoint vs current Manager of user is changed
                if (story.Manager != null && storyOwner.Manager != null && story.Manager.Id != storyOwner.Manager.Id 
                    && !_storySecurityService.HasPermisisonOnStory(storyOwner.Manager, storyOwner, story.Manager)) 
                {
                    if (currentUser.Id == storyOwner.Id)
                    {
                        story.WarningMessage = $"Your new coach is {storyOwner.Manager.Name}!<br/>{storyOwner.Manager.Name} can't see the latest 1-on-1 meeting of previous coach.<br/>To work with your new coach, please create a new meeting to share the previous content.";
                    }
                    else if (currentUser.Id == storyOwner.Manager.Id)
                    {
                        story.WarningMessage = $"You has been assigned to be as a new coach of {storyOwner.Name}!<br/>You can't see the latest 1-on-1 meeting of previous coach.<br/>Please ask {storyOwner.Name} to create a new meeting to share the previous content with you.";
                    }
                }

                story.HasPermissionOnCurrentMeetingNote = _storySecurityService.HasPermisisonOnStory(currentUser, storyOwner, story.Manager);
                if (story.HasPermissionOnCurrentMeetingNote && story.SharepointID.HasValue && story.SharepointID.Value != 0)
                {
                    story = await _storySharepointService.GetStory(story);
                }
                story.User = storyOwner;
            }

            return story;
        }

        public async Task<List<Model.Dto.Story>> GetOldStories(int userId)
        {
            var result = await _storySharepointService.GetOldStories(userId);
            return result;
        }

        public async Task<Model.Story> CreateStory(Model.Dto.Story story, int userId)
        {
            var model = _storyDtoMapper.ToModel(story);

            var member = _userService.GetUserById(userId);
            if (member == null)
            {
                throw new NotFoundException($"Coudn't not find user id={userId}");
            }

            var manager = member.Manager;
            if (manager == null)
            {
                manager = member;
            }

            var author = _appContext.CurrentUser;

            if (!_storySecurityService.HasPermisisonToCreateStory(member, author))
            {
                throw new UnauthorizedException($"You don't have permission to create story for member ID={member.LoginName}");
            }
            
            model.User = member;
            model.Manager = manager;
            model.Author = author;
            model.ModifiedBy = author;
            model.CreatedAt = DateTime.UtcNow;
            model.ModifiedAt = DateTime.UtcNow;

            int storySPId = await _storySharepointService.CreateStory(model);
            model.SharepointID = storySPId;

            var  result = _storyRepository.CreateStory(model);
            
            result.Title = story.Title;
            result.Description = story.Description;

            _emailService.StoryAddedNotification(result);

            return result;
        }

        public Model.Story UpdateStory(int storyID, Model.Dto.Story storyDto)
        {
            var targetStory = _storyRepository.GetStoryById(storyID);

            var currentUser = _appContext.CurrentUser;
            if (currentUser != null)
            {
                var targetStoryOwner = _userService.GetUserById(targetStory.User.Id);
                targetStory.HasPermission = _storySecurityService.HasPermisisonOnStory(currentUser, targetStoryOwner, targetStory.Manager);
                if (targetStory.HasPermission)
                {
                    var storyToUpdate = _storyDtoMapper.ToModel(storyDto);
                    storyToUpdate.Author = targetStory.Author;
                    storyToUpdate.Manager = targetStory.Manager;
                    storyToUpdate.CreatedAt = targetStory.CreatedAt;
                    storyToUpdate.SharepointID = targetStory.SharepointID;

                    storyToUpdate.ModifiedBy = currentUser;
                    storyToUpdate.ModifiedAt = DateTime.UtcNow;

                    targetStory = _storySharepointService.UpdateStory(storyToUpdate);
                    targetStory = _storyRepository.UpdateStory(storyToUpdate);
                }
                else
                {
                    throw new UnauthorizedException($"You don't have permission on story ID={storyID}");
                }
            }
           
            return targetStory;
        }

        public bool DeleteOldStory(int storySPId)
        {
            var result = _storySharepointService.DeleteOldStory(storySPId);
            return result;
        }
    }
}
