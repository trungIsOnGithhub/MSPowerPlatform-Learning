using PAS.Common;
using PAS.Model.Mapping;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services
{
    public class PersonalNoteService : IPersonalNoteService
    {
        private readonly IPersonalNoteSharepointService _personalNoteSharepointService;
        private readonly IPersonalNoteDtoMapper _dtoMapper;
        private readonly IStoryService _storyService;
        private readonly IPersonalNoteRepository _personalNoteRepository;
        private readonly IApplicationContext _appContext;
        private readonly IPersonalNoteSecurityService _personalNoteSecurityService;

        public PersonalNoteService(
            IPersonalNoteSharepointService personalNoteSharepointService,
            IPersonalNoteDtoMapper dtoMapper,
            IStoryService storyService,
            IPersonalNoteRepository personalNoteRepository,
            IApplicationContext appContext,
            IPersonalNoteSecurityService personalNoteSecurityService
            )
        {
            _personalNoteRepository = personalNoteRepository;
            _personalNoteSharepointService = personalNoteSharepointService;
            _dtoMapper = dtoMapper;
            _storyService = storyService;
            _personalNoteSecurityService = personalNoteSecurityService;
            _appContext = appContext;
        }

        public List<Model.Dto.PersonalNote> GetPersonalNotes(int storyId)
        {
            var user = _appContext.CurrentUser;
            if (user == null)
            {
                throw new System.Exception("No current user");
            }

            var notes = _personalNoteRepository.GetPersonalNotesByStory(storyId, user.Id);

            if (notes.Count != 0)
            {
                notes = _personalNoteSharepointService.GetPersonalNotesById(notes.ToList());
            }

            return notes.Select(note =>
            {
                return _dtoMapper.ToDto(note);
            }).ToList();
        }

        public Model.PersonalNote SavePersonalNote(Model.Dto.PersonalNote personalNoteDto, int userId)
        {
            var model = _dtoMapper.ToModel(personalNoteDto);

            var currentUser = _appContext.CurrentUser;
            model.CreatedBy = currentUser;
            model.ModifiedBy = currentUser;
            model.CreatedAt = DateTime.UtcNow;
            model.ModifiedAt = DateTime.UtcNow;

            // get storyId
            var story = _storyService.GetStoryLightByUserId(userId);

            model.Story = story;

            model = _personalNoteSharepointService.SavePersonalNote(model);

            model = _personalNoteRepository.SavePersonalNote(model);

            return model;
        }

        public Model.Dto.PersonalNote UpdatePersonalNotes(int id, Model.Dto.PersonalNote personalNoteDto)
        {
            var targetNote = _personalNoteRepository.GetPersonalNoteById(id);
            if (targetNote == null)
            {
                throw new System.Exception($"Couldn't find Private Note ID={id}");
            }

            var currentUser = _appContext.CurrentUser;

            bool hasPermission = _personalNoteSecurityService.HasPermissionOnPeronalNote(targetNote, currentUser);
            if (!hasPermission)
            {
                throw new UnauthorizedException($"You don't have permission on Private Note ID={personalNoteDto.Id}");
            }

            var model = _dtoMapper.ToModel(personalNoteDto);
            model.CreatedBy = targetNote.CreatedBy;
            model.CreatedAt = targetNote.CreatedAt;
            model.Story = targetNote.Story;
            model.ModifiedBy = currentUser;
            model.ModifiedAt = DateTime.UtcNow;

            model = _personalNoteSharepointService.UpdatePersonalNote(targetNote.SharepointId, model);
            _personalNoteRepository.Update(id, model);
            personalNoteDto = _dtoMapper.ToDto(model);
            return personalNoteDto;
        }

        public void DeletePersonalNote(int id)
        {
            var noteToDelete = _personalNoteRepository.GetPersonalNoteById(id);
            if (noteToDelete == null)
            {
                throw new System.Exception($"Couldn't find Private Note ID={id}");
            }

            var currentUser = _appContext.CurrentUser;
            bool hasPermission = _personalNoteSecurityService.HasPermissionOnPeronalNote(noteToDelete, currentUser);
            if (!hasPermission)
            {
                throw new UnauthorizedException($"You don't have permission on Private Note ID={noteToDelete.Id}");
            }

            // delete on sharepoint
            _personalNoteSharepointService.DeletePersonalNote(noteToDelete);

            // delete on repository
            _personalNoteRepository.DeletePersonalNote(noteToDelete);
        }
    }
}
