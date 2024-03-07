using PAS.Model.Domain;
using PAS.Services;
using System.Net;
using System.Web.Http;

namespace PAS.API.Controllers
{
    public class BaseController : ApiController
    {
        protected readonly IPermissionService permissionService;

        public BaseController(IPermissionService permissionService)
        {
            this.permissionService = permissionService;
        }

        public void CheckAdminPagePermission()
        {
            if (!permissionService.HasRightToViewAdminPage())
                throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        public void CheckManagerPagePermission()
        {
            if (!permissionService.HasRightToViewManagerPage())
                throw new HttpResponseException(HttpStatusCode.Forbidden);
        }
        public void CheckResumePagePermission(Resume resume)
        {
            if (!permissionService.HasRightToViewResumePage(resume))
                throw new HttpResponseException(HttpStatusCode.Forbidden);
        }
    }
}
