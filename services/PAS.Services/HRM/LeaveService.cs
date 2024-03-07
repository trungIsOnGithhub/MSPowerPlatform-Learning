using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PAS.Model.Enum.HRM;
using PAS.Model.HRM;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
namespace PAS.Services.HRM
{
    public interface ILeaveService : IBaseService
    {
        LeaveRequest AddLeaveRequest(LeaveRequestPost leaveRequest, int year);
        LeaveRequestApprover AddLeaveRequestApprover(LeaveRequestApprover leaveRequestApprover);
        List<User> GetApproverListByLeaveRequestId(Guid leaveRequestId);
        UserLeaveTrack GetUserLeaveTrack(int userId, int leaveTypeId);
        LeavePaging GetLeaveRequestByUserId(int userId, int page, int itemsPerPage);

        List<LeaveRequest> GetLeaveRequestByUserIdBatch(int userId);


        LeaveRequestHistory GetLatestHistoryByLeaveId(int userId, Guid leaveId);
        LeaveRequestHistory GetLatestHistoryByLeaveIdPublic(int userId, Guid leaveId);
        List<LeaveRequestHistory> GetHistoryOfLeaveRequest(Guid leaveId, int currentUserId);
        List<LeaveRequestHistory> GetHistoryOfLeaveRequestPublic(Guid leaveId, int currentUserId);
        LeaveRequest ApproveLeaveRequest(LeaveRequestHistoryAdd leaveRequestHistoryAdd);
        LeaveRequest RejectLeaveRequest(LeaveRequestHistoryAdd leaveRequestHistoryAdd);
        LeaveRequest RemoveLeaveRequest(LeaveRequestHistoryAdd leaveRequestHistoryAdd);
        LeaveRequest GetLeaveRequestById(Guid id);
        LeaveRequestForTeam GetLeaveDetailByLeaveId(Guid leaveId, int currentUserId);
        LeaveRequestForTeam GetLeaveDetailByLeaveIdPublic(Guid leaveId, int currentUserId);
        LeavePaging GetApprovalLeaveListByFilter(int userId, int page, int itemsPerPage, LeavesFilter filter);
        List<LeaveRequestHistory> GetApproverAction(Guid leaveId, int currentUserId);
        List<LeaveRequestHistory> GetApproverActionPublic(Guid leaveId);
        LeavePaging GetMyLeaveListByFilter(int userId, int page, int itemsPerPage, LeavesFilter filter);
        void AddLeaveRequestExtraEmail(IList<ExtraEmail> emails, Guid leaveId);
        List<LeaveRequest> FilterTeamLeave(List<LeaveRequest> listRequest, LeaveNotificationFilter filter);
        List<LeaveRequest> FilterTeamLeavePublic(List<LeaveRequest> listRequest, LeaveNotificationFilter filter);
        List<LeaveRequestExtraEmail> GetExternalEmailByLeaveId(Guid leaveId);
        List<UserBaseModel> GetApproverLeaveList(Guid leaveRequestId);
        bool GetAllPendingLeaveRequestOfDeletedLeaveType(int leaveTypeId);
        List<Group> GetGroupByLeaveId(Guid leaveId);
        List<User> GetApproverReminderListByLeaveRequest(LeaveRequest model);
        bool IsApproverOfCurrentLeaveRequest(Guid leaveRequestId, int approverId);
        LeaveRequestHistory AddLeaveRequestHistory(LeaveRequestHistory leaveRequestHistory,
            UserAction userAction);
        bool RemoveLeaveRequestApprover(LeaveRequestApprover leaveRequest);
        bool UpdateLeaveRequest(LeaveRequestPost leaveRequest, int currentUserId, Guid authoringSiteId);
        bool UpdateNotify(LeaveRequestPost leaveRequest);
        void RemoveLeaveRequestGroups(IList<Group> groups, Guid leaveId);
        LeaveRequestHistory AddLeaveRequestHistory(LeaveRequestPost leaveRequest, UserAction userAction);
        List<LeaveType> GetLeaveTypeListByUserId(int currentUserId);
        UserLeaveTrack GetUserLeaveTrackByYear(int userId, int leaveTypeId, int year);
        List<PAS.Model.HRM.UserLeaveTrack> GetListUserLeaveTrack(int userId, int leaveTypeId, int numberOfItem);
    }

    public class LeaveService : BaseService, ILeaveService
    {
        private readonly IUserLeaveYearService _userLeaveYearService;
        private readonly IUserLeaveTrackService _userLeaveTrackService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IMapper _mapper;
        public LeaveService(IHRMUnitOfWork unitOfWork, IUserLeaveYearService userLeaveYearService,
                            IUserLeaveTrackService userLeaveTrackService, ILeaveTypeService leaveTypeService,
                            IMapper mapper) : base(unitOfWork)
        {
            _userLeaveTrackService = userLeaveTrackService;
            _userLeaveYearService = userLeaveYearService;
            _leaveTypeService = leaveTypeService;
            _mapper = mapper;
        }

        public LeavePaging GetLeaveRequestByUserId(int userId, int page, int itemsPerPage)
        {

            var leavePaging = new LeavePaging();
            var query = from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                        where leaveRequest.RequestForUserId == userId
                        select leaveRequest;
            var result = query.Include(t => t.LeaveType)
                .Include(t => t.LeaveType.LeaveTypeLocalizations)
                .Include(t => t.RequestForUser)
                .OrderByDescending(t => t.Modified).ToList();

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
                    leavePaging.LeaveRequests = _mapper.Map<List<LeaveRequestForTeam>>(leaves);
                }
                else
                {
                    var leaves = result.ToList();
                    leavePaging.LeaveRequests = _mapper.Map<List<LeaveRequestForTeam>>(leaves);
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



        public LeaveRequestForTeam GetLeaveDetailByLeaveId(Guid leaveId, int currentUserId)
        {
            try
            {
                var result = GetLeaveDetailByLeaveIdContent(leaveId, currentUserId);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeaveRequestForTeam GetLeaveDetailByLeaveIdPublic(Guid leaveId, int currentUserId)
        {
            var result = GetLeaveDetailByLeaveIdContent(leaveId, currentUserId);
            return result;
        }

        public List<Model.HRM.LeaveRequest> GetLeaveRequestByUserIdBatch(int userId)
        {
            var subquery1 = from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet().Include(t => t.RequestForUser)
                            where leaveRequest.RequestForUserId == userId
                            select leaveRequest;

            var subquery2 = from leaveRequestApproverMap in _unitOfWork.LeaveRequestApproverRepository.DbSet()
                            select leaveRequestApproverMap;


            var subquery1Included = subquery1
                    //.Include(t => t.Requester)
                    .Include(t => t.LeaveType).ToList();

            var subquery2Included = subquery2.Include(t => t.UserApproved).ToList();


            foreach (var leaveRequest in subquery1Included)
            {
                leaveRequest.LeaveRequestApprovers = subquery2Included.Where(element => element.LeaveRequestId == leaveRequest.Id).ToList();
            }

            var result = this._mapper.Map<List<Model.HRM.LeaveRequest>>(subquery1Included);

            return result;
        }

        public List<UserBaseModel> GetApproverLeaveList(Guid leaveRequestId)
        {
            try
            {
                var approverList = GetAllApproverByLeaveId(leaveRequestId);
                return approverList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public LeaveRequestHistory GetLatestHistoryByLeaveId(int userId, Guid leaveId)
        {
            try
            {
                var result = GetLatestHistoryByLeaveIdContent(userId, leaveId);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeaveRequestHistory GetLatestHistoryByLeaveIdPublic(int userId, Guid leaveId)
        {
            var result = GetLatestHistoryByLeaveIdContent(userId, leaveId);
            return result;
        }

        public List<LeaveRequestHistory> GetHistoryOfLeaveRequest(Guid leaveId, int currentUserId)
        {
            try
            {
                var result = GetHistoryOfLeaveRequestContent(leaveId, currentUserId);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<LeaveRequestHistory> GetHistoryOfLeaveRequestPublic(Guid leaveId, int currentUserId)
        {
            var result = GetHistoryOfLeaveRequestContent(leaveId, currentUserId);
            return result;
        }

        public LeaveRequest AddLeaveRequest(LeaveRequestPost leaveRequest, int year)
        {
            try
            {
                var currentLeaveType = _leaveTypeService.GetLeaveTypeById(leaveRequest.LeaveTypeId);
                var newLeaveRequest = AddLeaveRequestDataMapping(
                                       leaveRequest
                                       , year
                                       , currentLeaveType.IsApprovalRequired ? ApprovalStep.Pending : ApprovalStep.Approved);
                var leaveRequestEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveRequest>(newLeaveRequest);
                leaveRequestEntity.Created = DateTime.UtcNow;
                leaveRequestEntity.Modified = DateTime.UtcNow;
                _unitOfWork.LeaveRequestRepository.Add(leaveRequestEntity);
                _unitOfWork.Save();

                AddLeaveRequestHistory(newLeaveRequest, UserAction.Create);

                if (leaveRequest.ExtraEmails != null || leaveRequest.ExtraEmails.Count > 0)
                {
                    AddLeaveRequestExtraEmail(leaveRequest.ExtraEmails, leaveRequestEntity.Id);
                }

                if (leaveRequest.Groups != null && leaveRequest.Groups.Count > 0)
                {
                    AddLeaveRequestGroup(leaveRequest.Groups, leaveRequestEntity.Id);
                }

                /// auto-approve 
                if (!currentLeaveType.IsApprovalRequired)
                {
                    AddLeaveRequestHistoryForNonApprovalRequire(newLeaveRequest, UserAction.Approve);
                }

                newLeaveRequest.LeaveType = currentLeaveType;
                return newLeaveRequest;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeaveRequestApprover AddLeaveRequestApprover(LeaveRequestApprover leaveRequestApprover)
        {
            try
            {
                var leaveRequestApproverEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveRequestApprover>(leaveRequestApprover);
                _unitOfWork.LeaveRequestApproverRepository.Add(leaveRequestApproverEntity);
                _unitOfWork.Save();

                return leaveRequestApprover;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public List<User> GetApproverReminderListByLeaveRequest(LeaveRequest model)
        {
            try
            {
                var historyUserId = model.LeaveRequestHistories.Where(x => x.Action != UserAction.Create).Select(x => x.UserId);

                var leaveRequestApproverList = _unitOfWork.LeaveRequestApproverRepository
                    .Find(t => !historyUserId.Contains(t.UserId) && t.LeaveRequestId == model.Id).Select(x => x.UserId)
                    .ToList();

                var result = _unitOfWork.UserRepository
                    .Find(t => leaveRequestApproverList.Contains(t.Id));
                return _mapper.Map<List<User>>(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public List<User> GetApproverListByLeaveRequestId(Guid leaveRequestId)
        {
            try
            {
                var leaveRequestApproverList = _unitOfWork.LeaveRequestApproverRepository
                    .Find(t => t.LeaveRequestId == leaveRequestId)
                    .ToList();
                var result = new List<User>();
                // because there is no relation between LeaveRequestApprover <-> User
                foreach (var leaveRequestApprover in leaveRequestApproverList)
                {
                    var foundUser = _unitOfWork.UserRepository
                        .Find(t => t.Id == leaveRequestApprover.UserId, includeProperties: "UserInformation")
                        .FirstOrDefault();
                    if (foundUser != null)
                    {
                        result.Add(_mapper.Map<User>(foundUser));
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public UserLeaveTrack GetUserLeaveTrack(int userId, int leaveTypeId)
        {
            try
            {
                var userLeaveTrackEntity = _unitOfWork.UserLeaveTrackRepository
                    .Find(t => t.UserId == userId && t.LeaveTypeId == leaveTypeId)
                    .FirstOrDefault();
                return _mapper.Map<UserLeaveTrack>(userLeaveTrackEntity);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public UserLeaveTrack GetUserLeaveTrackByYear(int userId, int leaveTypeId, int year)
        {
            try
            {
                var userLeaveTrackEntity = _unitOfWork.UserLeaveTrackRepository
                    .Find(t => t.UserId == userId && t.LeaveTypeId == leaveTypeId)
                    .FirstOrDefault();
                return _mapper.Map<UserLeaveTrack>(userLeaveTrackEntity);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<UserLeaveTrack> GetListUserLeaveTrack(int userId, int leaveTypeId, int numberOfItem)
        {
            try
            {
                var userLeaveTrackEnitity = _unitOfWork.UserLeaveTrackRepository.Find(u => u.UserId == userId && u.LeaveTypeId == leaveTypeId).OrderByDescending(u => u.UserLeaveYear.Year).Take(numberOfItem).ToList();
                return _mapper.Map<List<UserLeaveTrack>>(userLeaveTrackEnitity);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeaveRequest ApproveLeaveRequest(LeaveRequestHistoryAdd leaveRequestHistoryAdd)
        {
            try
            {

                // update leave request status
                var leaveRequestEntity = _unitOfWork.LeaveRequestRepository.Find(t => t.Id == leaveRequestHistoryAdd.LeaveRequestId, includeProperties: "LeaveType").FirstOrDefault();

                var checkUserIsAlreadyApproved = _unitOfWork.LeaveRequestHistoryRepository.Find(x => x.LeaveRequestId == leaveRequestHistoryAdd.LeaveRequestId && x.UserId == leaveRequestHistoryAdd.UserId && x.Action == UserAction.Approve).Any();

                if (leaveRequestEntity.Status == ApprovalStep.Pending && !checkUserIsAlreadyApproved)
                {
                    var leaveRequestHistory = _mapper.Map<LeaveRequestHistory>(leaveRequestHistoryAdd);
                    AddLeaveRequestHistory(leaveRequestHistory, UserAction.Approve);

                    leaveRequestEntity.Status = ApprovalStep.Approved;

                    leaveRequestEntity.Modified = DateTime.UtcNow;
                    _unitOfWork.Save();

                    var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestEntity);
                    leaveRequest.CurrentApprover = new User { Id = leaveRequestHistoryAdd.UserId };
                    return leaveRequest;
                }
                return null;



            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeaveRequest RejectLeaveRequest(LeaveRequestHistoryAdd leaveRequestHistoryAdd)
        {
            try
            {     // update Leave Request Status
                var leaveRequestEntity = _unitOfWork.LeaveRequestRepository
                    .FindById(leaveRequestHistoryAdd.LeaveRequestId);
                var checkUserIsAlreadyRejected = _unitOfWork.LeaveRequestHistoryRepository.Find(x => x.LeaveRequestId == leaveRequestHistoryAdd.LeaveRequestId && x.UserId == leaveRequestHistoryAdd.UserId && x.Action == UserAction.Reject).Any();
                if (leaveRequestEntity.Status == ApprovalStep.Pending && !checkUserIsAlreadyRejected)
                {
                    // history
                    var leaveRequestHistory = _mapper.Map<LeaveRequestHistory>(leaveRequestHistoryAdd);
                    AddLeaveRequestHistory(leaveRequestHistory, UserAction.Reject);

                    leaveRequestEntity.Status = ApprovalStep.Rejected;
                    leaveRequestEntity.Modified = DateTime.UtcNow;
                    _unitOfWork.Save();
                    var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestEntity);
                    leaveRequest.CurrentApprover = new User { Id = leaveRequestHistoryAdd.UserId };
                    return leaveRequest;
                }
                return null;


            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeaveRequest RemoveLeaveRequest(LeaveRequestHistoryAdd leaveRequestHistoryAdd)
        {
            try
            {
                // history
                var leaveRequestHistory = _mapper.Map<LeaveRequestHistory>(leaveRequestHistoryAdd);
                AddLeaveRequestHistory(leaveRequestHistory, UserAction.Remove);
                // update Leave Request Status
                var leaveRequestEntity = _unitOfWork.LeaveRequestRepository
                    .FindById(leaveRequestHistoryAdd.LeaveRequestId);
                leaveRequestEntity.IsRemoved = true;
                leaveRequestEntity.Modified = DateTime.UtcNow;
                _unitOfWork.Save();

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestEntity);
                leaveRequest.CurrentApprover = new User { Id = leaveRequestHistoryAdd.UserId };
                return leaveRequest;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeaveRequest RejectLeaveRequestOfDeletedLeaveType(LeaveRequestHistoryAdd leaveRequestHistoryAdd)
        {
            try
            {
                // history
                var leaveRequestHistory = _mapper.Map<LeaveRequestHistory>(leaveRequestHistoryAdd);
                AddLeaveRequestHistoryOfDeletedLeaveType(leaveRequestHistory, UserAction.Reject);
                // update Leave Request Status
                var leaveRequestEntity = _unitOfWork.LeaveRequestRepository
                    .FindById(leaveRequestHistoryAdd.LeaveRequestId);
                leaveRequestEntity.Status = ApprovalStep.Rejected;
                leaveRequestEntity.Modified = DateTime.UtcNow;
                _unitOfWork.Save();

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestEntity);
                leaveRequest.CurrentApprover = new User { Id = leaveRequestHistoryAdd.UserId };
                return leaveRequest;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public LeaveRequest GetLeaveRequestById(Guid id)
        {
            try
            {
                var leaveRequestEntity = _unitOfWork.LeaveRequestRepository.Find(t => t.Id == id,
                        includeProperties:
                        "Requester, RequestForUser, LeaveType, LeaveRequestExtraEmails, LeaveRequestApprovers")
                    .FirstOrDefault();
                return _mapper.Map<LeaveRequest>(leaveRequestEntity);
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public List<LeaveRequestHistory> GetApproverAction(Guid leaveId, int currentUserId)
        {
            try
            {
                var leaveRequest = _unitOfWork.LeaveRequestRepository.Find(t => t.Id == leaveId, includeProperties: "LeaveType").FirstOrDefault();

                var leaveApproverQuery = (from approver in _unitOfWork.LeaveRequestApproverRepository.DbSet()
                                          join users in _unitOfWork.UserRepository.DbSet()
                                          on approver.UserId equals users.Id
                                          where approver.LeaveRequestId == leaveId
                                          select users).Include("UserInformation").AsNoTracking();
                var leaveHistoryQuery = (from leaveRequestHistory in _unitOfWork.LeaveRequestHistoryRepository.DbSet()
                                         where leaveRequestHistory.LeaveRequestId == leaveId
                                         && leaveRequestHistory.Action != UserAction.Remove
                                         group leaveRequestHistory by leaveRequestHistory.UserId into groups
                                         select groups.OrderByDescending(t => t.Modified).FirstOrDefault())
                           .Include("User.UserInformation").AsNoTracking();

                var leaveApproverList = _mapper.Map<List<UserWithInformationModel>>(leaveApproverQuery);
                var leaveHistoryList = _mapper.Map<List<LeaveRequestHistory>>(leaveHistoryQuery);
                var isAlreadyApprove = _unitOfWork.LeaveRequestHistoryRepository.Find(l => l.LeaveRequestId == leaveId && l.Action == UserAction.Approve).Any();

                if (isAlreadyApprove)
                {
                    leaveHistoryList.ForEach((element =>
                    {
                        element.Action = UserAction.Approve;
                    }));
                }
                var approverHistory = leaveHistoryList.Where(t => leaveApproverList.Any(z => z.Id == t.UserId)).ToList();
                var otherApprover = leaveApproverList.Where(t => !approverHistory.Any(z => z.UserId == t.Id)).ToList();
                var result = new List<LeaveRequestHistory>();
                result.AddRange(approverHistory);
                //add approvers not in leave request history
                if (otherApprover != null)
                {
                    foreach (var approver in otherApprover)
                    {
                        result.Add(new LeaveRequestHistory
                        {
                            Id = Guid.NewGuid(),
                            UserId = approver.Id,
                            User = approver,
                            Action = 0,
                            LeaveRequestId = leaveId
                        });
                    }
                }

                var emailSentHistories = GetStatusEmail(leaveId, result.Select(item => item.User.UserInformation?.Email));

                foreach (var approverStatus in result)
                {
                    approverStatus.StatusEmail = emailSentHistories.FirstOrDefault(item => item.To == approverStatus.User.UserInformation.Email)?.Status;
                }


                if (currentUserId != leaveRequest.RequestForUserId && !otherApprover.Any(t => t.Id == currentUserId))
                {
                    result.ForEach(t => t.Comment = "");
                }
                //Convert userAction to (Create) Pending if request is non Approval Required
                if (!leaveRequest.LeaveType.IsApprovalRequired)
                {
                    result.ForEach(t => t.Action = UserAction.Create);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<EmailSentHistory> GetStatusEmail(Guid leaveId, IEnumerable<string> emails)
        {
            var result = new List<EmailSentHistory>();
            //todo
            return result;
        }

        public List<LeaveRequestHistory> GetApproverActionPublic(Guid leaveId)
        {
            var result = GetApproverActionContent(leaveId);
            return result;
        }

        public LeavePaging GetMyLeaveListByFilter(int userId, int page, int itemsPerPage, LeavesFilter filter)
        {
            try
            {
                var leavePaging = new LeavePaging();
                var query = from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                            where leaveRequest.RequestForUserId == userId /*&& leaveRequest.IsRemoved == (Constants.Filter.RemovedRequest == filter.Status)*/
                            select leaveRequest;
                var result = query.Include(t => t.LeaveType)
                    .Include(t => t.LeaveType.LeaveTypeLocalizations)
                    .Include(t => t.RequestForUser).ToList().AsQueryable();
                var approvedLeaveRequestHistoryListEntity = (from leaveRequest in _unitOfWork.LeaveRequestHistoryRepository.DbSet()
                                                             where leaveRequest.Action == UserAction.Approve && leaveRequest.UserId == userId
                                                             select leaveRequest).Include("LeaveRequest.LeaveType.LeaveTypeLocalizations");
                result = FilterLeaveList(result, approvedLeaveRequestHistoryListEntity, filter);
                leavePaging = Pagination(result, page, itemsPerPage);
                return leavePaging;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public LeavePaging GetApprovalLeaveListByFilter(int userId, int page, int itemsPerPage, LeavesFilter filter)
        {
            try
            {
                var result = (from leaveRequestApprover in _unitOfWork.LeaveRequestApproverRepository.DbSet()
                              join leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                                  on leaveRequestApprover.LeaveRequestId equals leaveRequest.Id
                              where leaveRequestApprover.UserId == userId
                              select leaveRequest);
                var approvedLeaveRequestHistoryListEntity = (from leaveRequest in _unitOfWork.LeaveRequestHistoryRepository.DbSet()
                                                             where leaveRequest.Action == UserAction.Approve && leaveRequest.UserId == userId
                                                             select leaveRequest);
                result = FilterLeaveList(result, approvedLeaveRequestHistoryListEntity, filter);
                var leavePaging = Pagination(result, page, itemsPerPage);
                return leavePaging;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<LeaveRequest> FilterTeamLeave(List<LeaveRequest> listRequest, LeaveNotificationFilter filter)
        {
            try
            {
                var result = FilterTeamLeaveContent(listRequest, filter);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<LeaveRequest> FilterTeamLeavePublic(List<LeaveRequest> listRequest, LeaveNotificationFilter filter)
        {
            var result = FilterTeamLeaveContent(listRequest, filter);
            return result;
        }

        public void AddLeaveRequestExtraEmail(IList<ExtraEmail> emails, Guid leaveId)
        {
            try
            {
                foreach (var email in emails)
                {
                    var leaveRequestExtraEmail = new PAS.Repositories.DataModel.LeaveRequestExtraEmail
                    {
                        Id = Guid.NewGuid(),
                        Email = email.Email,
                        LeaveRequestId = leaveId
                    };
                    _unitOfWork.LeaveRequestExtraEmailRepository.Add(leaveRequestExtraEmail);

                }
                _unitOfWork.Save();



            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<LeaveRequestExtraEmail> GetExternalEmailByLeaveId(Guid leaveId)
        {
            try
            {
                var externalEmail = from email in _unitOfWork.LeaveRequestExtraEmailRepository.DbSet()
                                    where email.LeaveRequestId == leaveId
                                    select email;
                var externalEmailList = _mapper.Map<List<LeaveRequestExtraEmail>>(externalEmail);
                return externalEmailList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Group> GetGroupByLeaveId(Guid leaveId)
        {
            try
            {
                var groups = _unitOfWork.LeaveRequestGroupRepository.Find(x => x.LeaveRequestId == leaveId).Select(x => x.Group);
                var groupDtoList = _mapper.Map<List<Group>>(groups);
                return groupDtoList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void AddLeaveRequestGroup(IList<Group> groups, Guid leaveRequestId)
        {
            try
            {
                foreach (var group in groups)
                {
                    var entity = new PAS.Repositories.DataModel.LeaveRequestGroup
                    {
                        Id = Guid.NewGuid(),
                        LeaveRequestId = leaveRequestId,
                        GroupId = group.Id
                    };
                    _unitOfWork.LeaveRequestGroupRepository.Add(entity);
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool GetAllPendingLeaveRequestOfDeletedLeaveType(int leaveTypeId)
        {
            try
            {
                //var query = (from leaves in _unitOfWork.LeaveRequestRepository.DbSet()
                //           where leaves.LeaveTypeId == leaveTypeId && leaves.Status == ApprovalStep.Pending
                //           select leaves).ToList();
                var query = (from leaves in _unitOfWork.LeaveRequestHistoryRepository.DbSet()
                             where leaves.LeaveRequest.LeaveTypeId == leaveTypeId && leaves.LeaveRequest.Status == ApprovalStep.Pending
                             select leaves).ToList();
                var test = _mapper.Map<List<LeaveRequestHistoryAdd>>(query);
                foreach (var leave in test)
                {
                    RejectLeaveRequestOfDeletedLeaveType(leave);
                }
                //query.ForEach(x => x.Status = ApprovalStep.Rejected);
                //_unitOfWork.Save();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #region Private Methods

        private List<LeaveRequestHistory> GetApproverActionContent(Guid leaveId)
        {
            var leaveHistoryQuery = (from leaveRequestApprover in _unitOfWork.LeaveRequestApproverRepository.DbSet()
                                     join leaveRequestHistory in _unitOfWork.LeaveRequestHistoryRepository.DbSet()
                                         on leaveRequestApprover.LeaveRequestId equals leaveRequestHistory.LeaveRequestId
                                     where leaveRequestApprover.LeaveRequestId == leaveId
                                     select leaveRequestHistory)
                .Include("User.UserInformation").AsNoTracking()
                .OrderByDescending(t => t.Modified);
            var leaveHistoryList = _mapper.Map<List<LeaveRequestHistory>>(leaveHistoryQuery);
            var result = new List<LeaveRequestHistory>();
            // do not take the LAST item who CREATED the request
            for (var i = 0; i < leaveHistoryList.Count - 1; i++)
            {
                // trim history list
                if (result.All(t => t.UserId != leaveHistoryList[i].UserId))
                {
                    result.Add(leaveHistoryList[i]);
                }
            }

            var leaveApproverQuery = (from approver in _unitOfWork.LeaveRequestApproverRepository.DbSet()
                                      join users in _unitOfWork.UserRepository.DbSet()
                                          on approver.UserId equals users.Id
                                      where approver.LeaveRequestId == leaveId
                                      select users)
                .Include("UserInformation").AsNoTracking();
            var leaveApproverList = _mapper.Map<List<UserWithInformationModel>>(leaveApproverQuery);
            var otherApprover = leaveApproverList
                .Where(t => leaveHistoryList.All(z => z.UserId != t.Id))
                .ToList();
            //add approvers not in leave request history
            foreach (var approver in otherApprover)
            {
                result.Add(new LeaveRequestHistory
                {
                    UserId = approver.Id,
                    User = approver,
                    Action = 0,
                    LeaveRequestId = leaveId
                });
            }
            return result;
        }

        private List<LeaveRequest> FilterTeamLeaveContent(List<LeaveRequest> listRequest,
            LeaveNotificationFilter filter)
        {
            if (filter.Status != null)
                listRequest = listRequest.Where(t => t.Status == filter.Status).ToList();
            if (filter.GroupListId != null && filter.GroupListId.Count > 0)
                listRequest = listRequest.Where(t => filter.GroupListId.Any(group => group == t.Group.Id)).ToList();
            return listRequest;
        }



        private List<LeaveRequestHistory> GetHistoryOfLeaveRequestContent(Guid leaveId, int currentUserId)
        {
            var leaveRequest = _unitOfWork.LeaveRequestRepository.Find(t => t.Id == leaveId, includeProperties: "LeaveType").FirstOrDefault();
            var leaveApprover = _unitOfWork.LeaveRequestApproverRepository.Find(t => t.LeaveRequestId == leaveId).ToList();

            var result = _unitOfWork.LeaveRequestHistoryRepository
                .Find(t => t.LeaveRequestId == leaveId)
                .OrderByDescending(t => t.Created)
                .ToList();

            if (!leaveRequest.LeaveType.IsApprovalRequired)
            {
                var item = result.FirstOrDefault(t => t.Action == UserAction.Approve);
                if (item != null)
                {
                    item.User = new PAS.Repositories.DataModel.User()
                    {
                    };
                }
            }
            else
            {
                var items = result.Where(t => (t.Action == UserAction.Approve || UserAction.Reject == t.Action) && t.Comment == Constants.LeaveHistory.SystemApproverRemoved).ToList();
                items.ForEach(x => x.User = new PAS.Repositories.DataModel.User()
                {
                });

                if (currentUserId != leaveRequest.RequestForUserId && !leaveApprover.Any(t => t.UserId == currentUserId))
                {
                    result.ForEach(t => t.Comment = "");
                }
            }



            return _mapper.Map<List<LeaveRequestHistory>>(result);
        }

        private LeaveRequestHistory GetLatestHistoryByLeaveIdContent(int userId, Guid leaveId)
        {
            var query = from leaveRequestHistory in _unitOfWork.LeaveRequestHistoryRepository.DbSet()
                        where leaveRequestHistory.LeaveRequestId == leaveId
                        select leaveRequestHistory;
            if (userId != null) query = query.Where(t => t.UserId == userId);
            var result = query.ToList();
            var latestAction = result.FirstOrDefault(a => a.Created == result.Max(t => t.Created));

            return _mapper.Map<LeaveRequestHistory>(latestAction);
        }

        private LeaveRequestForTeam GetLeaveDetailByLeaveIdContent(Guid leaveId, int currentUserId)
        {
            var query = from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                        where leaveRequest.Id == leaveId
                        select leaveRequest;
            var result = query.Include(t => t.RequestForUser).AsNoTracking()
                              .Include(t => t.LeaveType).AsNoTracking()
                              .Include(t => t.LeaveType.LeaveTypeLocalizations).AsNoTracking()
                              .Include(t => t.LeaveRequestHistories).AsNoTracking()
                              .FirstOrDefault();
            var leaveApprover = _unitOfWork.LeaveRequestApproverRepository.Find(t => t.LeaveRequestId == leaveId).ToList();

            if (currentUserId != result.RequestForUserId && !leaveApprover.Any(t => t.UserId == currentUserId))
            {
                result.Reason = "";
            }

            //var isApprove =  (from history in _unitOfWork.LeaveRequestHistoryRepository.DbSet()
            //                 where history.UserId == userId && history.Action == UserAction.Approve
            //                 select history);
            //if (result.Status == ApprovalStep.Pending && isApprove.Any())
            //{
            //    result.Status = ApprovalStep.Waiting;
            //}
            var leaveDetail = _mapper.Map<LeaveRequestForTeam>(result);
            leaveDetail.LeaveType.Name = leaveDetail.LeaveType.LeaveTypeLocalizations[0].Name;
            return leaveDetail;
        }

        private UserLeaveTrack GetUserLeaveTrackContent(int userId, int leaveTypeId)
        {
            var userLeaveTrackEntity = _unitOfWork.UserLeaveTrackRepository
                .Find(t => t.UserId == userId && t.LeaveTypeId == leaveTypeId)
                .FirstOrDefault();

            return _mapper.Map<UserLeaveTrack>(userLeaveTrackEntity);
        }

        private IQueryable<PAS.Repositories.DataModel.LeaveRequest> FilterLeaveList(IQueryable<PAS.Repositories.DataModel.LeaveRequest> result,
            IQueryable<PAS.Repositories.DataModel.LeaveRequestHistory> approvedLeaveRequestList, LeavesFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.Status) && filter.Status != Constants.Filter.AllRequest)
            {
                switch (filter.Status)
                {
                    case Constants.Filter.RemovedRequest:
                        {
                            result = from leave in result
                                     where leave.IsRemoved == true
                                     select leave;
                            break;
                        }
                    case Constants.Filter.ApproveRequest:
                        {
                            result = from leave in result
                                     where leave.Status == ApprovalStep.Approved && !leave.IsRemoved
                                     select leave;
                            break;
                        }
                    case Constants.Filter.RejectRequest:
                        {
                            result = from leave in result
                                     where leave.Status == ApprovalStep.Rejected && !leave.IsRemoved
                                     select leave;
                            break;
                        }
                    case Constants.Filter.PendingRequest:
                        {
                            result = from leave in result
                                     where leave.Status == ApprovalStep.Pending && !leave.IsRemoved
                                           && !(from approveLeave in approvedLeaveRequestList
                                                select approveLeave.LeaveRequestId).Contains(leave.Id)
                                     select leave;
                            break;
                        }
                    case Constants.Filter.WaitingRequest:
                        {
                            result = from leave in result
                                     join approved in approvedLeaveRequestList
                                         on leave.Id equals approved.LeaveRequestId
                                     where leave.Status == ApprovalStep.Pending && !leave.IsRemoved
                                     select leave;
                            break;
                        }
                }
            }
            // filter by LeaveType
            if (filter.LeaveType != null && filter.LeaveType.Code != null)
            {
                result = from leave in result
                         where leave.LeaveType.Code == filter.LeaveType.Code
                         select leave;
            }
            // filter by StartDate
            if (filter.StartDate.HasValue)
            {
                result = from leave in result
                         where leave.StartDate >= filter.StartDate
                         select leave;
            }
            // filter by EndDate
            if (filter.EndDate.HasValue)
            {
                result = from leave in result
                         where leave.EndDate <= filter.EndDate
                         select leave;
            }
            // filter by SearchString
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                result = from leave in result
                         where leave.RequestForUser.Name.ToLower()
                         .Contains(filter.SearchString.ToLower()) == true
                         select leave;
            }

            return result;
        }

        private LeaveRequest AddLeaveRequestDataMapping(LeaveRequestPost leaveRequest, int year, ApprovalStep defautStatus)
        {
            var authoringSiteId = _unitOfWork.LeaveTypeRepository.Find(t => t.Id == leaveRequest.LeaveTypeId)
                .FirstOrDefault()?.AuthoringSiteId;
            var userLeaveYear = _unitOfWork.UserLeaveYearRepository
                .Find(t => t.Year == year,
                    includeProperties: "Holidays")
                .FirstOrDefault();
            var holidayList = _mapper.Map<List<Holiday>>(userLeaveYear?.Holidays);
            var leaveRequestPeriodList = GetLeaveRequestPeriodsByUserId(leaveRequest.RequestForUserId)
                .Where(t => t.Id != leaveRequest.Id)
                .ToList(); // except current leave request id;

            var newLeaveRequest = _mapper.Map<LeaveRequest>(leaveRequest);
            newLeaveRequest.Id = Guid.NewGuid();
            newLeaveRequest.Status = defautStatus;
            newLeaveRequest.Created = DateTime.UtcNow;
            newLeaveRequest.Modified = DateTime.UtcNow;
            newLeaveRequest.DisplayId = Constants.EntityKeys.LeaveRequest + GetTimestamp(DateTime.UtcNow);
            //if(leaveRequest.DayLeaveType == DayLeaveType.FullDay)
            //{
            //    newLeaveRequest.NumberOfDay =
            //    CalculateNumberOfLeaveDays(newLeaveRequest.StartDate,
            //        newLeaveRequest.EndDate, holidayList, leaveRequestPeriodList,
            //        newLeaveRequest.DayLeaveType);
            //}
            //else
            //{
            //    newLeaveRequest.NumberOfDay = Constants.NumberOfDays.HalfDay;
            //}
            newLeaveRequest.NumberOfDay = leaveRequest.NumberOfDay;
            return newLeaveRequest;
        }

        private string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        public LeaveRequestHistory AddLeaveRequestHistory(LeaveRequestHistory leaveRequestHistory,
            UserAction userAction)
        {
            leaveRequestHistory.Id = Guid.NewGuid();
            leaveRequestHistory.Action = userAction;
            leaveRequestHistory.Created = DateTime.UtcNow;
            leaveRequestHistory.Modified = DateTime.UtcNow;

            var leaveRequestHistoryEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveRequestHistory>(leaveRequestHistory);
            _unitOfWork.LeaveRequestHistoryRepository.Add(leaveRequestHistoryEntity);
            _unitOfWork.Save();

            return leaveRequestHistory;
        }
        private LeaveRequestHistory AddLeaveRequestHistoryOfDeletedLeaveType(LeaveRequestHistory leaveRequestHistory,
            UserAction userAction)
        {
            leaveRequestHistory.Id = Guid.NewGuid();
            leaveRequestHistory.Action = userAction;
            leaveRequestHistory.Created = DateTime.UtcNow;
            leaveRequestHistory.Modified = DateTime.UtcNow;
            leaveRequestHistory.Comment = "This leave type has been removed";
            var leaveRequestHistoryEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveRequestHistory>(leaveRequestHistory);
            _unitOfWork.LeaveRequestHistoryRepository.Add(leaveRequestHistoryEntity);
            _unitOfWork.Save();

            return leaveRequestHistory;
        }
        private LeaveRequestHistory AddLeaveRequestHistory(LeaveRequest leaveRequest, UserAction userAction)
        {
            var leaveRequestHistory = new LeaveRequestHistory
            {
                Id = Guid.NewGuid(),
                UserId = leaveRequest.RequesterId,
                LeaveRequestId = leaveRequest.Id,
                Action = userAction,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };
            var leaveRequestHistoryEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveRequestHistory>(leaveRequestHistory);
            _unitOfWork.LeaveRequestHistoryRepository.Add(leaveRequestHistoryEntity);
            _unitOfWork.Save();

            return leaveRequestHistory;
        }

        private LeaveRequestHistory AddLeaveRequestHistoryForNonApprovalRequire(LeaveRequest leaveRequest, UserAction userAction)
        {
            var leaveRequestHistory = new LeaveRequestHistory
            {
                Id = Guid.NewGuid(),
                UserId = leaveRequest.RequesterId,
                LeaveRequestId = leaveRequest.Id,
                Comment = "System approved this",
                Action = userAction,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };
            var leaveRequestHistoryEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveRequestHistory>(leaveRequestHistory);
            _unitOfWork.LeaveRequestHistoryRepository.Add(leaveRequestHistoryEntity);
            _unitOfWork.Save();

            return leaveRequestHistory;
        }

        private bool IsApprovedByLeadersInGroup(Guid leaveRequestId, int requestForUserId, List<GroupDetail> groupDetails, bool? easyGroup)
        {
            var leaders = groupDetails.Where(item => item.UserId != requestForUserId && item.LeaveApprovalLevelId == Constants.UserRoleManagement.Manager);
            if (easyGroup.HasValue)
            {
                if (easyGroup.Value)
                {
                    if (leaders.Any())
                    {
                        foreach (var leader in leaders.ToList())
                        {
                            var isApproved = _unitOfWork.LeaveRequestHistoryRepository
                                .Find(i => i.UserId == leader.UserId && i.LeaveRequestId == leaveRequestId && i.Action == UserAction.Approve)
                                .Any();
                            if (isApproved) return true;
                        }
                        return false;
                    }
                    else return true;
                }
                else
                {
                    if (leaders.Any())
                    {
                        foreach (var leader in leaders.ToList())
                        {
                            var isApproved = _unitOfWork.LeaveRequestHistoryRepository
                                .Find(i => i.UserId == leader.UserId && i.LeaveRequestId == leaveRequestId && i.Action == UserAction.Approve)
                                .Any();
                            if (!isApproved) return false;
                        }
                    }
                    return true;
                }
            }
            else return true;
        }

        private List<LeaveRequest> GetApprovalLeaveList(int userId)
        {
            var query = (from leaveRequestApprover in _unitOfWork.LeaveRequestApproverRepository.DbSet()
                         join leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                             on leaveRequestApprover.LeaveRequestId equals leaveRequest.Id
                         where leaveRequestApprover.UserId == userId
                         select leaveRequest)
                .Include("RequestForUser.UserInformation")
                .Include("LeaveType.LeaveTypeLocalizations");
            var leaveRequestListEntity = query.ToList();
            return _mapper.Map<List<LeaveRequest>>(leaveRequestListEntity);
        }

        private LeavePaging Pagination(IQueryable<PAS.Repositories.DataModel.LeaveRequest> searchLeaveList, int page, int itemsPerPage)
        {
            var searchList = searchLeaveList.ToList();

            var leavePaging = new LeavePaging();
            var totalCount = searchLeaveList.Count();
            leavePaging.TotalItems = totalCount;
            leavePaging.TotalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage);
            if (page <= leavePaging.TotalPages && page >= 1)
            {
                if (totalCount > itemsPerPage)
                {
                    var leaves = searchLeaveList.OrderByDescending(t => t.Modified).Skip(itemsPerPage * (page - 1))
                        .Take(itemsPerPage)
                        .Include("RequestForUser.UserInformation")
                        .Include("LeaveType.LeaveTypeLocalizations")
                        .OrderByDescending(t => t.StartDate)
                        .ToList();
                    leavePaging.LeaveRequests = _mapper.Map<List<LeaveRequestForTeam>>(leaves);
                }
                else
                {
                    var leaves1 = searchLeaveList
                        .Include("RequestForUser.UserInformation")
                        .Include("LeaveType.LeaveTypeLocalizations")
                        .OrderByDescending(t => t.StartDate);
                    var leaves = searchLeaveList
                        .Include("RequestForUser.UserInformation")
                        .Include("LeaveType.LeaveTypeLocalizations")
                        .OrderByDescending(t => t.StartDate)
                        .ToList();
                    leavePaging.LeaveRequests = _mapper.Map<List<LeaveRequestForTeam>>(leaves);
                }
                // mapping leave type name with localization
                foreach (var leave in leavePaging.LeaveRequests)
                {
                    leave.LeaveType.Name = leave.LeaveType.LeaveTypeLocalizations[0].Name;
                    leave.LeaveType.Description = leave.LeaveType.LeaveTypeLocalizations[0].Description;
                }
            }
            return leavePaging;
        }


        private List<UserBaseModel> GetAllApproverByLeaveId(Guid leaveId)
        {
            var approverId = from leaveApprover in _unitOfWork.LeaveRequestApproverRepository.DbSet()
                             join user in _unitOfWork.UserRepository.DbSet()
                             on leaveApprover.UserId equals user.Id
                             where leaveApprover.LeaveRequestId == leaveId
                             select user;
            var approver = _mapper.Map<List<UserBaseModel>>(approverId);
            return approver;
        }
        public bool IsApproverOfCurrentLeaveRequest(Guid leaveRequestId, int approverId)
        {
            return _unitOfWork.LeaveRequestApproverRepository.Find(x => x.LeaveRequestId == leaveRequestId && x.UserId == approverId).Any();
        }


        public bool RemoveLeaveRequestApprover(LeaveRequestApprover leaveRequest)
        {
            try
            {
                var removeRow = _unitOfWork.LeaveRequestApproverRepository.Find(l => l.LeaveRequestId == leaveRequest.LeaveRequestId && l.UserId == leaveRequest.UserId).FirstOrDefault();
                if (removeRow != null)
                {
                    _unitOfWork.LeaveRequestApproverRepository.Delete(removeRow);
                    _unitOfWork.Save();
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateLeaveRequest(LeaveRequestPost leaveRequest, int currentUserId, Guid authoringSiteId)
        {
            try
            {
                LeaveRequest currentLeaveRequest = new LeaveRequest();
                UserLeaveTrack oldUserLeaveTrack = new UserLeaveTrack();
                UserLeaveTrack updateLeaveTrack = new UserLeaveTrack();
                var latestUserLeaveYear = _userLeaveYearService.GetLatestUserLeaveYear(authoringSiteId);
                var updateRow = _unitOfWork.LeaveRequestRepository.Find(l => l.Id == leaveRequest.Id).FirstOrDefault();
                if (updateRow != null)
                {
                    if (updateRow.LeaveTypeId != leaveRequest.LeaveTypeId)
                    {
                        var currentUserLeaveTrack = _unitOfWork.UserLeaveTrackRepository.Find(item => item.UserId == updateRow.RequestForUserId &&
                        item.LeaveTypeId == updateRow.LeaveTypeId && item.UserLeaveYearId == latestUserLeaveYear.Id).FirstOrDefault();
                        oldUserLeaveTrack = _mapper.Map<UserLeaveTrack>(currentUserLeaveTrack);
                        currentUserLeaveTrack.RemainLeaveDay += updateRow.NumberOfDay;
                        updateLeaveTrack = _mapper.Map<UserLeaveTrack>(currentUserLeaveTrack);
                        currentLeaveRequest.Id = leaveRequest.Id;
                        AddUserLeaveTrackHistory(oldUserLeaveTrack, updateLeaveTrack, currentLeaveRequest, UserAction.Edit, nameof(UserLeaveTrack.RemainLeaveDay), currentUserId);
                    }

                    updateRow.StartDate = leaveRequest.StartDate;
                    updateRow.EndDate = leaveRequest.EndDate;
                    updateRow.DayLeaveType = leaveRequest.DayLeaveType;
                    updateRow.Reason = leaveRequest.Reason;
                    updateRow.LeaveTypeId = leaveRequest.LeaveTypeId;
                    //updateRow.NumberOfDay = GetNumberOfDay(leaveRequest);
                    updateRow.NumberOfDay = leaveRequest.NumberOfDay;
                    _unitOfWork.Save();

                    UpdateNotify(leaveRequest);

                    updateLeaveTrack = _userLeaveTrackService.UpdateEditedRequest(leaveRequest, latestUserLeaveYear.Year);

                    oldUserLeaveTrack = GetUserLeaveTrack(leaveRequest.RequestForUserId, leaveRequest.LeaveTypeId);
                    currentLeaveRequest.Id = leaveRequest.Id;
                    AddUserLeaveTrackHistory(oldUserLeaveTrack, updateLeaveTrack, currentLeaveRequest, UserAction.Create, nameof(UserLeaveTrack.RemainLeaveDay), currentUserId);
                    if (updateLeaveTrack != null)
                    {
                        leaveRequest.RequestForUserId = updateRow.RequestForUserId;
                        AddLeaveRequestHistory(leaveRequest, UserAction.Edit);
                    }
                }
                else
                {
                    return false;
                }
                _unitOfWork.Save();
                return true;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void AddUserLeaveTrackHistory(UserLeaveTrack oldUserLeaveTrack, UserLeaveTrack newUserLeaveTrack, LeaveRequest leaveRequest, UserAction action, string entityColumn, int currentUserId)
        {
            var userLeaveTrackHistory = new UserLeaveTrackHistory()
            {
                Id = Guid.NewGuid(),
                UserLeaveTrackId = oldUserLeaveTrack.Id,
                ModifiedId = Guid.NewGuid(),
                LeaveRequestId = leaveRequest.Id,
                Action = action,
                Entity = entityColumn,
            };
            switch (entityColumn)
            {
                case nameof(UserLeaveTrack.TotalLeaveDay):
                    {
                        userLeaveTrackHistory.OldValue = oldUserLeaveTrack.TotalLeaveDay;
                        userLeaveTrackHistory.NewValue = newUserLeaveTrack.TotalLeaveDay;
                        break;
                    }
                case nameof(UserLeaveTrack.RemainLeaveDay):
                    {
                        userLeaveTrackHistory.OldValue = oldUserLeaveTrack.RemainLeaveDay;
                        userLeaveTrackHistory.NewValue = newUserLeaveTrack.RemainLeaveDay;
                        break;
                    }
            }

            _userLeaveTrackService.AddUserLeaveTrackHistory(userLeaveTrackHistory);
        }

        public bool UpdateNotify(LeaveRequestPost leaveRequest)
        {
            try
            {
                var oldEmails = GetExternalEmailByLeaveId(leaveRequest.Id);
                var oldGroups = GetGroupByLeaveId(leaveRequest.Id);

                var newEmails = leaveRequest.ExtraEmails.Where(e => !oldEmails.Any(o => e.Email == o.Email)).ToList();
                var newGroups = leaveRequest.Groups.Where(g => !oldGroups.Any(o => g.Id == o.Id)).ToList();

                var removeGroups = oldGroups.Where(o => !leaveRequest.Groups.Any(g => g.Id == o.Id)).ToList();
                var removeEmails = oldEmails.Where(o => !leaveRequest.ExtraEmails.Any(e => e.Email == o.Email)).ToList();

                if (newEmails != null || newEmails.Count > 0)
                {
                    AddLeaveRequestExtraEmail(newEmails, leaveRequest.Id);
                }

                if (newGroups != null && newGroups.Count > 0)
                {
                    AddLeaveRequestGroup(newGroups, leaveRequest.Id);
                }

                if (removeEmails != null || removeEmails.Count > 0)
                {
                    RemoveLeaveRequestExtraEmail(removeEmails, leaveRequest.Id);
                }

                if (removeGroups != null && removeGroups.Count > 0)
                {
                    RemoveLeaveRequestGroups(removeGroups, leaveRequest.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void RemoveLeaveRequestExtraEmail(IList<LeaveRequestExtraEmail> emails, Guid leaveId)
        {
            try
            {
                foreach (var email in emails)
                {
                    var removeRow = _unitOfWork.LeaveRequestExtraEmailRepository.Find(l => l.Email == email.Email && l.LeaveRequestId == leaveId).FirstOrDefault();
                    _unitOfWork.LeaveRequestExtraEmailRepository.Delete(removeRow);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void RemoveLeaveRequestGroups(IList<Group> groups, Guid leaveId)
        {
            try
            {
                foreach (var group in groups)
                {
                    var removeRow = _unitOfWork.LeaveRequestGroupRepository.Find(g => g.Group.Id == group.Id && g.LeaveRequestId == leaveId).FirstOrDefault();
                    _unitOfWork.LeaveRequestGroupRepository.Delete(removeRow);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public float GetNumberOfDay(LeaveRequestPost leaveRequest)
        {
            try
            {
                float numberOfDay = 0;
                // get holiday list
                var authoringSiteId = _unitOfWork.LeaveTypeRepository
                    .Find(t => t.Id == leaveRequest.LeaveTypeId)
                    .FirstOrDefault().AuthoringSiteId;
                var year = _userLeaveYearService.GetLatestUserLeaveYear(authoringSiteId).Year;

                var userLeaveYear = _unitOfWork.UserLeaveYearRepository
                    .Find(t => t.Year == year, includeProperties: "Holidays")
                    .FirstOrDefault();
                var holidayList = _mapper.Map<List<Holiday>>(userLeaveYear?.Holidays);
                var leaveRequestPeriodList = GetLeaveRequestPeriodsByUserId(leaveRequest.RequestForUserId)
                    .Where(t => t.Id != leaveRequest.Id)
                    .ToList(); // except current leave request id

                // calculate number of days again
                numberOfDay = CalculateNumberOfLeaveDays(leaveRequest.StartDate,
                    leaveRequest.EndDate, holidayList, leaveRequestPeriodList,
                    leaveRequest.DayLeaveType);
                return numberOfDay;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public LeaveRequestHistory AddLeaveRequestHistory(LeaveRequestPost leaveRequest, UserAction userAction)
        {
            var leaveRequestHistory = new LeaveRequestHistory();
            leaveRequestHistory.Id = Guid.NewGuid();
            leaveRequestHistory.UserId = leaveRequest.RequestForUserId;
            leaveRequestHistory.LeaveRequestId = leaveRequest.Id;
            leaveRequestHistory.Action = userAction;
            leaveRequestHistory.Created = DateTime.UtcNow;
            leaveRequestHistory.Modified = DateTime.UtcNow;
            if (userAction == UserAction.Edit)
            {
                leaveRequestHistory.Comment = leaveRequest.Comment;
            }

            var leaveRequestHistoryEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveRequestHistory>(leaveRequestHistory);
            _unitOfWork.LeaveRequestHistoryRepository.Add(leaveRequestHistoryEntity);
            _unitOfWork.Save();

            return leaveRequestHistory;
        }
        public List<LeaveType> GetLeaveTypeListByUserId(int currentUserId)
        {
            var leaveTypeList = (from lLeaveType in _unitOfWork.LeaveTypeRepository.DbSet()
                                 join lTrack in _unitOfWork.UserLeaveTrackRepository.DbSet()
                                     on lLeaveType.Id equals lTrack.LeaveTypeId
                                 join lLocal in _unitOfWork.LeaveTypeLocalizationRepository.DbSet() on lLeaveType.Id equals lLocal.Id
                                 where lTrack.UserId == currentUserId && lLeaveType.IsActive == true
                                 select lLeaveType).ToList();

            var leaveType = _mapper.Map<List<LeaveType>>(leaveTypeList);
            foreach (var item in leaveType)
            {
                item.Name = _unitOfWork.LeaveTypeLocalizationRepository.Find(t => t.Id == item.Id).ToList()[0].Name;
            }

            return leaveType;
        }
        #endregion
    }
}