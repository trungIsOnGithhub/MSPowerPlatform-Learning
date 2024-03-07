using Microsoft.AspNetCore.Http;
using PAS.Model.Enum;
using PAS.Repositories;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace PAS.Services
{
    public interface IApplicationContext
    {
        Model.User CurrentUser { get; }
    }

    public class ApplicationContext : IApplicationContext
    {
        private IUserRepository _userRepository;
        private IHttpContextAccessor _httpContextAccessor { get; }
        public ApplicationContext(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public Model.User CurrentUser => GetCurrentUser();

        private Model.User GetCurrentUser()
        {
            Model.User result = null;
            if (_httpContextAccessor.HttpContext.User != null)
            {
                var userPrincipal = _httpContextAccessor.HttpContext.User as ClaimsPrincipal;

                if (userPrincipal != null && userPrincipal.Claims.Count() > 0)
                {
                    var loginName = userPrincipal.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn").Select(c => c.Value).SingleOrDefault();
                    result = _userRepository.GetUserByLoginNameLight(loginName);
                }
            }
            return result;
        }
    }
}
