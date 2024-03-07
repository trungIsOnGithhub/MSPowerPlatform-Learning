using PAS.Common;
using PAS.Model.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface ISharepointFileService
    {
        Task<List<SharepointImage>> GetAllFolderFiles(string folderRelativeUrl);
        Task<SharepointImage> ReadSharepointFile(string fileRelativeUrl);
    }
}
