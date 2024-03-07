using PAS.Model.Domain;

namespace PAS.Services
{
    public interface IPermissionService
    {
        bool HasRightToViewAdminPage();
        bool HasRightToViewManagerPage();
        bool HasRightToViewResumePage(Resume resume);
        bool HasRightToViewTechnicalLevel(Model.User userProfile);
        public bool HasRightToViewAllLeaveRequestAndTrack(Model.User userProfile);
    }
}