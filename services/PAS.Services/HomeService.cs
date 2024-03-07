using PAS.Common;
using PAS.Model;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IHomeService
    {
        Task<SharePointInfo> GetSharePointInfo();
    }

    public class HomeService : IHomeService
    {
        protected readonly ISharepointContextProvider _ctxProvider;

        public HomeService(ISharepointContextProvider ctxProvider)
        {
            _ctxProvider = ctxProvider;
        }

        public async Task<SharePointInfo> GetSharePointInfo()
        {
            var ctx = await _ctxProvider.GetClientContext();
            ctx.Load(ctx.Site.RootWeb, w => w.Title, w => w.Url);
            ctx.ExecuteQuery();
            var result = new SharePointInfo();
            result.Title = ctx.Site.RootWeb.Title;
            result.Url = ctx.Site.RootWeb.Url;
            return result;
        }
    }
}
