using PAS.Model.Domain;
using System.Collections.Generic;

namespace PAS.Services
{
    public interface IResumeService
    {
        void AddResume(Resume resume);
        void UpdateResume(Resume resume);
        void DeleteResume(int resumeId);
        void CopyResume(int resumeId, Resume resume);
        void AddResumeItem(ProfileItem resumeItem, List<Resume> resumes);
        void UpdateResumeItem(ProfileItem resumeItem);
        void UpdateResumeItems(ProfileItem resumeItem, List<Resume> resumes);
        void DeleteResumeItem(int parentItemId);
        List<Resume> GetUserResumes(int userId);
        List<Model.Domain.ProfileItem> GetResumeItems(int resumeId);
        Resume GetResume(int resumeId);
    }
}