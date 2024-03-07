using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IPersonalNoteSharepointService
    {
        Model.PersonalNote SavePersonalNote(Model.PersonalNote model);
        List<Model.PersonalNote> GetPersonalNotesById(List<Model.PersonalNote> list);
        Model.PersonalNote UpdatePersonalNote(int SharepointId, Model.PersonalNote newContent);
        void DeletePersonalNote(Model.PersonalNote model);
    }
}
