using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IPersonalNoteService
    {
        Model.PersonalNote SavePersonalNote(Model.Dto.PersonalNote personalNoteDto, int userId);
        List<Model.Dto.PersonalNote> GetPersonalNotes(int storyId);
        Model.Dto.PersonalNote UpdatePersonalNotes(int id, Model.Dto.PersonalNote personalNoteDto);
        void DeletePersonalNote(int id);
    }
}
