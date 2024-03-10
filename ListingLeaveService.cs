using AngleSharp.Io;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using PAS.Model.Dto;
using PAS.Model.Enum.HRM;
// using PAS.Model.HRM;
using PAS.Repositories.DataModel;
using PAS.Repositories.HRM;
using PAS.Services.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tavis.UriTemplates;

namespace PAS.Services
{
    public interface IListingLeaveService
    {
        LeavePaging GetLeaveRequestByUserId(int userId, int page, int itemsPerPage, LeaveRequestFilterDto filter);
    }

    public class ListingLeaveService : IListingLeaveService
    {
        // private readonly IUserLeaveYearService _userLeaveYearService;
        private readonly IUserLeaveTrackService _userLeaveTrackService;
        private readonly ILeaveService _leaveService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IWorkHistoryService _workHistoryService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IHRMUnitOfWork _unitOfWork;
        public ListingLeaveService(IHRMUnitOfWork unitOfWork, IUserLeaveTrackService userLeaveTrackService, IWorkHistoryService workHistoryService,
                            ILeaveService leaveService, IUserService userService, ILeaveTypeService leaveTypeService, IMapper mapper)
        {
            _userLeaveTrackService = userLeaveTrackService;
            // _userLeaveYearService = userLeaveYearService;
            _leaveTypeService = leaveTypeService;
            _leaveService = leaveService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _workHistoryService = workHistoryService;
        }


        // MY LEAVE REQUEST FEATURE
        public LeavePaging GetLeaveRequestByUserId(int userId, int page, int itemsPerPage, LeaveRequestFilterDto filter)
        {
            // var listItemForApprovalStep = this._enumService.ToListItem(typeof(ApprovalStep));
            var dateFormatString = "dd/MM/yyyy";

            var query = from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                        where leaveRequest.RequestForUserId == userId
                        select leaveRequest;

            query = query
                .Include(request => request.LeaveType.LeaveTypeLocalizations)
                .Include(request => request.Requester)
                .Include(request => request.LeaveRequestApprovers)
                .ThenInclude(approvers => approvers.User);

            // start filtering
            if (filter.FilterLeaveTypeId >= 0)
            {
                query = query.Where(request => request.LeaveTypeId == filter.FilterLeaveTypeId);
            }

            if (filter.FilterStatus >= 0)
            {
                query = query.Where(request => (int)request.Status == filter.FilterStatus);
            }


            if (!string.IsNullOrEmpty(filter.FilterStartDate))
            {
                var startDate = this.StringToDateFormat(filter.FilterStartDate);
                query = query.Where(request => request.StartDate.Date >= startDate.Date);
            }
            if (!string.IsNullOrEmpty(filter.FilterEndDate))
            {
                var endDate = this.StringToDateFormat(filter.FilterEndDate);
                query = query.Where(request => request.EndDate.Date <= endDate.Date);
            }

            if (filter.SortOrder == "asc")
            {
                if (filter.OrderBy == "startDate")
                    query = query.OrderBy(request => request.StartDate);
                else if (filter.OrderBy == "endDate")
                    query = query.OrderBy(request => request.EndDate);
            }
            else if (filter.SortOrder == "desc")
            {
                if (filter.OrderBy == "startDate")
                    query = query.OrderByDescending(request => request.StartDate);
                else if (filter.OrderBy == "endDate")
                    query = query.OrderByDescending(request => request.EndDate);
            }
            else
            {
                query.OrderByDescending(request => request.Modified);
            }

            return BuildLeaveRequestPaging(page, itemsPerPage, query.ToList());
        }
        // MY LEAVE REQUEST FEATURE


        // LEAVE SUMMARY FEATURE
        public List<UserLeaveTrack> GetUserWithUserLeaveTrackByFilter(int userId, LeaveRequestFilterDto leaveTrackFilter)
        {
            var users = this.GetUsersThatCanBeViewedByPermission(userId);

            // Filter By current year
            var years = GetYearsListByDateRangeFromFilter(leaveTrackFilter);

            if (years.Count < 1)
            { return new List<UserLeaveTrack>(); }

            return MapToUserWithUserLeaveTracks(users, years, leaveTrackFilter);
        }
        private List<UserLeaveTrack> MapToUserWithUserLeaveTracks(List<User> users, List<int> years, LeaveRequestFilter leaveTrackFilter)
        {
            var mappedUserLeaveTracks = new List<UserLeaveTrack>();

            foreach (var user in users)
            {
                // get user leave track list in year range
                var userLeaveTrackList = this._userLeaveTrackService
                                            .GetUserLeaveTrackByUserAndYearRange(user.Id, years);

                for (var i = 0; i < userLeaveTrackList.Count; i++)
                {
                    userLeaveTrackList[i].User = this._userService.GetUserById(userLeaveTrackList[i].UserId);
                    userLeaveTrackList[i].LeaveType = this._leaveTypeService.GetLeaveTypeById(userLeaveTrackList[i].LeaveTypeId);
                    // get leave type name
                    // var leaveTypeOfThisTrack = this._leaveTypeService
                    //                     .GetLeaveTypeById(userLeaveTrack.LeaveTypeId);
                    
                    // if (leaveTypeOfThisTrack == null)
                    //     continue;
                    var leaveRequestFilter = new LeaveRequestFilterDto {
                        FilterLeaveTypeId = userLeaveTrackList[i].LeaveTypeId,
                        FilterStartDate = leaveTrackFilter.FilterStartDate,
                        FilterEndDate = leaveTrackFilter.FilterEndDate,
                    };

                    // Calculate Remain leave days by requests
                    var leaveRequests = this.GetLeaveRequestByUserIdNoPagingNoApprovers(user.Id, filter);
                    // Total leave days varied with WFH
                    if (userLeaveTrackList[i].LeaveType.Code?.ToLower() == Constants.WorkFromHomeCode)
                    {
                        userLeaveTrackList[i].TotalLeaveDay = 0;
                        var firstDayFilterStartDate = new DateTime(filter.FilterStartDate.Value.Year, filter.FilterStartDate.Value.Month, 1);

                        while (firstDayFilterStartDate < filter.FilterEndDate.Value)
                        {
                            // 6 days per month for people have 5 years of service or more
                            if (user.JoinedDate.GetValueOrDefault().AddYears(5) < firstDayFilterStartDate)
                            {
                                userLeaveTrackList[i].TotalLeaveDay += 6;
                            }
                            // 4 days per month for people have 2 years of service or more
                            else if (user.JoinedDate.GetValueOrDefault().AddYears(2) < firstDayFilterStartDate)
                            {
                                userLeaveTrackList[i].TotalLeaveDay += 4;
                            }

                            firstDayFilterStartDate = firstDayFilterStartDate.AddMonths(1);
                        }
                    }

                    if (leaveRequests != null)
                    {
                        var totalRequestDay = leaveRequests
                                                .Where(x => x.Status != ApprovalStep.Rejected && !x.IsRemoved)
                                                .Sum(x => x.NumberOfDay);
                        /// re-caculate total leave days
                        userLeaveTrackList[i].RemainLeaveDay = userLeaveTrackList[i].TotalLeaveDay - totalRequestDay;
                    }
                    else
                    {
                        userLeaveTrackList[i].RemainLeaveDay = userLeaveTrackList[i].TotalLeaveDay;
                    }

                    mappedUserLeaveTracks.Add(userLeaveTrackList[i]);
                }
            }

            // default order ascending by name
            return mappedUserLeaveTracks;
        }

        public List<LeaveRequest> GetLeaveRequestByUserIdNoPagingNoApprovers(int userId, LeaveRequestFilterDto filter)
        {
            var query = (from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                        where leaveRequest.RequestForUserId == userId
                        select leaveRequest);
                            // .Include(request => request.LeaveType);
            // start filter
            if (filter.FilterLeaveTypeId >= 0)
            {
                query = query.Where(request => request.LeaveTypeId == filter.FilterLeaveTypeId);
            }
            if (!string.IsNullOrEmpty(filter.FilterStartDate))
            {
                var startDate = this.StringToDateFormat(filter.FilterStartDate);
                query = query.Where(request => request.StartDate.Date >= startDate.Date);
            }
            if (!string.IsNullOrEmpty(filter.FilterEndDate))
            {
                var endDate = this.StringToDateFormat(filter.FilterEndDate);
                query = query.Where(request => request.EndDate.Date <= endDate.Date);
            }
            // end filter

            return query.ToList();
        }
        public List<User> GetUsersThatCanBeViewedByPermission(int userId)
        {
            var currentUser = this._userService.GetUserById(userId);

            IQueryable<User> getUsersQuery;

            if (this._permissionService.HasRightToViewAllLeaveRequestAndTrack(currentUserProfile))
            {
                getUsersQuery = (from user in _unitOfWork.UserRepository.DbSet()
                                    where user.IsActive == true
                                select user);
            }
            else
            {
                getUsersQuery = (from user in _unitOfWork.UserRepository.DbSet()
                                    where user.IsActive == true &&
                                        ( (user.ManagerId != null && user.ManagerId == userId) || user.Id == userId )
                                select user);
            }

            return getUsersQuery.ToList();
        }


        #region HelperMethod
        public int CalculateYearOfServiceBeforeDateOrDefault(int userId, DateTime userJoinedDate, DateTime beforeDate)
        {
            try
            {
                return this._workHistoryService.CalculateYearOfServiceBeforeDate(userId, beforeDate);
            }
            catch (Exception ex)
            {
                return beforeDate.Year - userJoinedDate.Year;
            }
        }
        public LeavePaging BuildLeaveRequestPaging(int page, int itemsPerPage, List<PAS.Repositories.DataModel.LeaveRequest> result)
        {
            var leavePaging = new LeavePaging();

            var totalCount = result.Count();

            leavePaging.TotalItems = totalCount;
            leavePaging.TotalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage);

            if (page <= leavePaging.TotalPages && page >= 1)
            {
                if (totalCount > itemsPerPage)
                {
                    var leaves = result.OrderByDescending(t => t.Modified).Skip(itemsPerPage * (page - 1))
                        .Take(itemsPerPage)
                        .ToList();

                    leavePaging.LeaveRequests = _mapper.Map<List<PAS.Model.HRM.LeaveRequest>>(leaves);
                }
                else
                {
                    var leaves = result.ToList();

                    leavePaging.LeaveRequests = _mapper.Map<List<PAS.Model.HRM.LeaveRequest>>(leaves);
                }

                foreach (var leave in leavePaging.LeaveRequests)
                    foreach (var leaveLocalization in leave.LeaveType.LeaveTypeLocalizations)
                    {
                        leave.LeaveType.Name = leave.LeaveType.LeaveTypeLocalizations[0].Name;
                        leave.LeaveType.Description = leave.LeaveType.LeaveTypeLocalizations[0].Description;
                    }
            }

            return leavePaging;
        }
        private DateTime StringToDateFormat(string dateString)
        {
            var dateFormatString = "dd/MM/yyyy";
            // wrong format will get back to DateTime.Now
            try
            {
                return DateTime.ParseExact(dateString, dateFormatString, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }
        private List<int> GetYearsListByDateRangeFromFilter(LeaveRequestFilterDto leaveTrackFilter)
        {
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(leaveTrackFilter.FilterStartDate))
            {
                startDate = StringToDateFormat(leaveTrackFilter.FilterStartDate);
            }

            if (!string.IsNullOrEmpty(leaveTrackFilter.FilterEndDate))
            {
                endDate = StringToDateFormat(leaveTrackFilter.FilterEndDate);
            }
			
			if (endDate.Date < startDate.Date)
				return new List<int>();

            return Enumerable.Range(startDate.Year, endDate.Year - startDate.Year + 1).ToList();
        }
        #endregion
    }
}
