using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace PAS.Common
{
    public interface ISharepointContextProvider
    {
        Task<ClientContext> GetClientContext(string webUrl = null);
        Task<ClientContext> GetAppOnlyContext(string webUrl = null);
        ClientContext GetUserContext(string webUrl = null);
    }
}
