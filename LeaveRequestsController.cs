using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PAS.Model.HRM;
using PAS.Model.Mapping;
using PAS.Services.HRM;
using PAS.Services;

namespace PAS.API.Controllers
{
    [Route("api/leave-requests")]
    [ApiController]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly IApplicationContext _applicationContext;
        private readonly IUserLeaveTrackService _userLeaveTrackService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IUserLeaveYearService _userLeaveYearService;
        private readonly PAS.Services.HRM.ILeaveService _leaveService;
        private readonly PAS.Services.IListingLeaveService _listingLeaveService;

        public LeaveRequestsController(IUserService userService, IPermissionService permissionService,
                                        IUserLeaveTrackService userLeaveTrackService, IUserLeaveYearService userLeaveYearService,
                                        ILeaveTypeService leaveTypeService, ILeaveService leaveService, IListingLeaveService listingLeaveService,
                                        IApplicationContext applicationContext)
        {
            this._userService = userService;
            this._permissionService = permissionService;
            this._userLeaveTrackService = userLeaveTrackService;
            this._userLeaveYearService = userLeaveYearService;
            this._leaveTypeService = leaveTypeService;
            this._leaveService = leaveService;
            this._listingLeaveService = listingLeaveService;
            this._applicationContext = applicationContext;
        }

        //// GET: api/<leave-request/id>
        //[HttpGet("{id}")]
        //public List<LeaveRequest> GetLeaveRequestsBatch(int id)
        //{
        //    return this._leaveService.GetLeaveRequestByUserIdBatch(id);
        //}

        // GET api/<ValuesController1>/5
        [HttpPost("tracks")]
        public List<UserLeaveTrack> GetUserLeaveTrackFilter([FromBody] PAS.Model.Dto.LeaveRequestFilterDto filter)
        {
            var userId = this._applicationContext.CurrentUser.Id;

            // var currentUserProfile = this._userService.GetUserById(userId);

            // List<UserLeaveTrack> userLeaveTrack = new();

            // if (this._permissionService.HasRightToViewAllLeaveRequestAndTrack(currentUserProfile))
            // {
            //     userLeaveTrack = this._userLeaveTrackService.GetUserLeaveTrackByYear(year);
            // }
            // else
            // {
            //     userLeaveTrack = this._userLeaveTrackService.GetUserLeaveTrackByUserAndYear(userId, year);

            //     var userLeaveTrackThisUserIsManager = this._userLeaveTrackService.GetUserLeaveTrackByYearAndManagerId(year, userId);

            //     userLeaveTrack.AddRange(userLeaveTrackThisUserIsManager);
            // }

            // foreach (var track in userLeaveTrack)
            // {
            //     track.User = this._userService.GetUserById(track.UserId);
            //     track.UserLeaveYear = this._userLeaveYearService.GetUserLeaveYearByYear(Guid.NewGuid(), year); // dummy guids
            //     track.LeaveType = this._leaveTypeService.GetLeaveTypeById(track.LeaveTypeId);
            // }

            return this._listingLeaveService.GetUserWithUserLeaveTrackByFilter(userId, filter);
        }

        // GET api/leave-requests/types
        [HttpGet("types")]
        public List<LeaveType> GetLeaveTypes()
        {
            return this._leaveTypeService.GetAllLeaveTypeWithoutAuthoringSiteId();
        }

        // GET api/leave-requests/years
        [HttpGet("years")]
        public List<UserLeaveYear> GetLeaveYears()
        {
            return this._userLeaveYearService.GetUserLeaveYearByAuthoringSite(Guid.NewGuid());
        }

        // GET api/leave-requests/{pageNumber}/{pageSize}
        [HttpPost("{pageNumber}/{pageSize}")]
        public LeavePaging GetLeaveRequestPagingWithFilter(int pageNumber, int pageSize, [FromBody] PAS.Model.Dto.LeaveRequestFilterDto filter)
        {
            var userId = this._applicationContext.CurrentUser.Id;
            return this._listingLeaveService.GetLeaveRequestByUserId(userId, pageNumber, pageSize, filter);
        }
    }
}
