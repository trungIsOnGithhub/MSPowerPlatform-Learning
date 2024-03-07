using PAS.Model;

namespace PAS.Services
{
    public interface IPersonalNoteSecurityService
    {
        bool HasPermissionOnPeronalNote(PersonalNote personalNote, User currentUser);
    }

    public class PersonalNoteSecurityService : IPersonalNoteSecurityService
    {
        public bool HasPermissionOnPeronalNote(PersonalNote personalNote, User currentUser)
        {
            bool hasPermission = false;
            if (personalNote.CreatedBy.Id == currentUser.Id)
            {
                hasPermission = true;
            }
            return hasPermission;
        }
    }
}
