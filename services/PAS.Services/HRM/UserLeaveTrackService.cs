using AutoMapper;
using PAS.Model.Enum.HRM;
using PAS.Model.HRM;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PAS.Services.HRM
{
    public interface IUserLeaveTrackService : IBaseService
    {
        void UpdateUserLeaveTrackByPreviousRemainLeaveDay(int userId, int year);
        UserLeaveTrack UpdateUserLeaveTrackBy(LeaveRequest leaveRequest, UserAction userAction, int year);
        Guid AddUserLeaveTrackByLeaveTypeId(int userId, int year, int leaveTypeId);
        List<Guid> AddUserLeaveTracks(int userId, int year);
        UserLeaveTrack GetUserLeaveTrackByUserAndYearAndLeaveType(int userId, int yearId, int leaveTypeId);
        UserLeaveTrack GetUserLeaveTrackByUserAndYearAndLeaveTypePublic(int userId, int yearId, int leaveTypeId);
        UserLeaveTrack GetUserLeaveTrackById(Guid userLeaveTrackId);
        UserLeaveTrack UpdateUserLeaveTrackTotalLeaveDay(UserLeaveTrackPost userLeaveTrackPost,
            Guid actionUserId, Guid modifiedId);
        float GetRemainLeaveDayOfYear(int userId, int leaveTypeId, int year);
        List<UserLeaveTrack> GetUserLeaveTrackByUserAndYear(int userId, int yearId);
        Guid AddUserLeaveTrackHistory(UserLeaveTrackHistory userLeaveTrackHistory);
        UserLeaveTrack UpdateEditedRequest(LeaveRequestPost leaveRequest, int year);
        void UpdateUserLeaveTrackBySeniorityPolicy(int userId, int year);
        List<UserLeaveTrackHistory> GetUserLeaveTrackHistoryByLeaveRequestId(Guid leaveRequestId);
        List<UserLeaveTrack> UpdateOldLeaveBetweenResetAndApplyDate(List<UserLeaveTrackHistory> listOldUserLeaveTrackHistories, LeaveRequestPost leaveRequest);

        List<UserLeaveTrack> GetFullUserLeaveTrackByUserAndYear(int userId, int yearId);

        List<UserLeaveTrack> GetUserLeaveTrackByYear(int year);

        List<UserLeaveTrack> GetUserLeaveTrackByYearAndManagerId(int year, int managerId);
    }

    public class UserLeaveTrackService : BaseService, IUserLeaveTrackService
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UserLeaveTrackService(IHRMUnitOfWork unitOfWork, IMapper mapper,
                                        IUserService userService) : base(unitOfWork)
        {
            this._mapper = mapper;
            this._userService = userService;
        }

        public void UpdateUserLeaveTrackByPreviousRemainLeaveDay(int userId, int year)
        {

            var userLeaveYearId = _unitOfWork.UserLeaveYearRepository.Find(t => t.Year == year)?.FirstOrDefault()?.Id;
            var leaveTypeEntityList = _unitOfWork.LeaveTypeRepository.Find().ToList();
            var leaveTypeList = _mapper.Map<List<LeaveType>>(leaveTypeEntityList);

            foreach (var leaveType in leaveTypeList)
            {
                var userLeaveTrackEntity = _unitOfWork.UserLeaveTrackRepository
                    .Find(t => t.UserLeaveYearId == userLeaveYearId
                               && t.UserId == userId
                               && t.LeaveTypeId == leaveType.Id)
                    .FirstOrDefault();
                // update current remaining leave day
                if (userLeaveTrackEntity != null)
                {
                    var previousRemainingLeaveDay = GetRemainLeaveDayOfYear(userId, leaveType.Id, year - 1);

                    userLeaveTrackEntity.RemainLeaveDay =
                        CalculateLeaveDayBaseOnPreviousRemaining(
                            leaveType,
                            userLeaveTrackEntity.RemainLeaveDay,
                            previousRemainingLeaveDay);
                    userLeaveTrackEntity.Modified = DateTime.UtcNow;
                    _unitOfWork.Save();
                }
            }

        }

        public UserLeaveTrack UpdateUserLeaveTrackBy(LeaveRequest leaveRequest, UserAction userAction, int year)
        {

            // get holiday list
            var authoringSiteId = _unitOfWork.LeaveTypeRepository
                .Find(t => t.Id == leaveRequest.LeaveTypeId)
                .FirstOrDefault()?.AuthoringSiteId;
            var userLeaveYear = _unitOfWork.UserLeaveYearRepository
                .Find(t => t.Year == year, includeProperties: "Holidays")
                .FirstOrDefault();
            var holidayList = _mapper.Map<List<Holiday>>(userLeaveYear?.Holidays);
            var leaveRequestPeriodList = GetLeaveRequestPeriodsByUserId(leaveRequest.RequestForUserId)
                .Where(t => t.Id != leaveRequest.Id)
                .ToList(); // except current leave request id

            // calculate number of days again
            if (leaveRequest.DayLeaveType == DayLeaveType.FullDay)
            {
                //leaveRequest.NumberOfDay = CalculateNumberOfLeaveDays(leaveRequest.StartDate,
                //leaveRequest.EndDate, holidayList, leaveRequestPeriodList,
                //leaveRequest.DayLeaveType);

                leaveRequest.NumberOfDay = leaveRequest.NumberOfDay;
            }
            else
            {
                leaveRequest.NumberOfDay = Constants.NumberOfDays.HalfDay;
            }
            UserLeaveTrack userLeaveTrack;
            // update 
            var foundUserLeaveTrack = _unitOfWork.UserLeaveTrackRepository
                .Find(t => t.UserId == leaveRequest.RequestForUserId
                           && t.LeaveTypeId == leaveRequest.LeaveTypeId
                           && t.UserLeaveYearId == userLeaveYear.Id)
                .FirstOrDefault();
            if (foundUserLeaveTrack != null)
            {
                foundUserLeaveTrack.RemainLeaveDay =
                    (userAction != UserAction.Reject && userAction != UserAction.Remove)
                    ? foundUserLeaveTrack.RemainLeaveDay - leaveRequest.NumberOfDay
                    : foundUserLeaveTrack.RemainLeaveDay + leaveRequest.NumberOfDay;
                foundUserLeaveTrack.Modified = leaveRequest.Modified;
                foundUserLeaveTrack.ModifiedBy = leaveRequest.ModifiedBy;

                _unitOfWork.Save();
                userLeaveTrack = _mapper.Map<UserLeaveTrack>(foundUserLeaveTrack);
            }
            else
            // add TODO: do we need this?
            {
                var leaveTypeBaseNumberOfDay = _unitOfWork.LeaveTypeRepository
                    .Find(t => t.Id == leaveRequest.LeaveTypeId)?.FirstOrDefault()?.BaseNumberOfDay;

                userLeaveTrack = new UserLeaveTrack
                {
                    Id = Guid.NewGuid(),
                    UserId = leaveRequest.RequestForUserId,
                    UserLeaveYearId = userLeaveYear.Id,
                    LeaveTypeId = leaveRequest.LeaveTypeId,
                    TotalLeaveDay = leaveTypeBaseNumberOfDay.GetValueOrDefault(),
                    RemainLeaveDay = leaveTypeBaseNumberOfDay.GetValueOrDefault() - leaveRequest.NumberOfDay,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow,
                };

                var userLeaveTrackEntity = _mapper.Map<PAS.Repositories.DataModel.UserLeaveTrack>(userLeaveTrack);
                _unitOfWork.UserLeaveTrackRepository.Add(userLeaveTrackEntity);
                _unitOfWork.Save();
            }

            return userLeaveTrack;

        }

        public Guid AddUserLeaveTrackByLeaveTypeId(int userId, int year, int leaveTypeId)
        {

            var userLeaveYearId = _unitOfWork.UserLeaveYearRepository.Find(t => t.Year == year).FirstOrDefault()?.Id;
            var leaveTypeEntity = _unitOfWork.LeaveTypeRepository
                .Find(t => t.Id == leaveTypeId && t.IsActive == true)
                .Include(t => t.LeaveTypeParams.Select(h => h.LeaveTypeParamDetails))
                .FirstOrDefault();
            var leaveType = _mapper.Map<LeaveType>(leaveTypeEntity);

            // Calculate new total leave day
            var totalLeaveDay = 0f;
            var userEntity = _unitOfWork.UserRepository
                .Find(t => t.Id == userId, includeProperties: "UserInformation")
                .FirstOrDefault();
            if (userEntity != null)
            {
                // todo
                totalLeaveDay = CalculateNumberOfLeaveDaysBySeniorityPolicy(leaveType, DateTime.Now, GenderType.Male);
            }
            var userLeaveTrack = new UserLeaveTrack
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserLeaveYearId = userLeaveYearId.GetValueOrDefault(),
                LeaveTypeId = leaveType.Id,
                TotalLeaveDay = totalLeaveDay,
            };
            userLeaveTrack.RemainLeaveDay = userLeaveTrack.TotalLeaveDay;

            var userLeaveTrackEntity = _mapper.Map<PAS.Repositories.DataModel.UserLeaveTrack>(userLeaveTrack);
            userLeaveTrackEntity.Created = DateTime.UtcNow;
            userLeaveTrackEntity.Modified = DateTime.UtcNow;
            _unitOfWork.UserLeaveTrackRepository.Add(userLeaveTrackEntity);
            _unitOfWork.Save();
            return userLeaveTrack.Id;

        }

        public List<Guid> AddUserLeaveTracks(int userId, int year)
        {

            // default authoring site of user
            var leaveTypeListEntity = _unitOfWork.LeaveTypeRepository
                .Find()
                .Include(t => t.LeaveTypeParams.Select(h => h.LeaveTypeParamDetails))
                .ToList();
            var leaveTypeList = _mapper.Map<List<LeaveType>>(leaveTypeListEntity);

            var userLeaveTrackList = new List<UserLeaveTrack>();
            var userLeaveYearId = _unitOfWork.UserLeaveYearRepository
                .Find(t => t.Year == year)
                .FirstOrDefault()?.Id;

            foreach (var leaveType in leaveTypeList)
            {
                // Calculate new total leave day
                //var totalLeaveDay = 0f;
                //var userEntity = _unitOfWork.UserRepository
                //    .Find(t => t.Id == userId, includeProperties: "UserInformation")
                //    .FirstOrDefault();
                //if (userEntity != null)
                //{
                //    totalLeaveDay = CalculateNumberOfLeaveDaysBySeniorityPolicy(leaveType,
                //        userEntity.StartDate.GetValueOrDefault(), userEntity.UserInformation.Gender);
                //}

                var userLeaveTrack = new UserLeaveTrack
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    UserLeaveYearId = userLeaveYearId.GetValueOrDefault(),
                    LeaveTypeId = leaveType.Id,
                    TotalLeaveDay = leaveType.BaseNumberOfDay,
                    RemainLeaveDay = leaveType.BaseNumberOfDay,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow,
                };
                userLeaveTrackList.Add(userLeaveTrack);
            }

            var userLeaveTrackEntities = _mapper.Map<List<PAS.Repositories.DataModel.UserLeaveTrack>>(userLeaveTrackList);
            foreach (var userLeaveTrackEntity in userLeaveTrackEntities)
            {
                userLeaveTrackEntity.Created = DateTime.UtcNow;
                userLeaveTrackEntity.Modified = DateTime.UtcNow;
                _unitOfWork.UserLeaveTrackRepository.Add(userLeaveTrackEntity);
            }
            _unitOfWork.Save();
            return userLeaveTrackList.Select(t => t.Id).ToList();

        }
        public void UpdateUserLeaveTrackBySeniorityPolicy(int userId, int year)
        {

            var userLeaveYearId = _unitOfWork.UserLeaveYearRepository.
                Find(t => t.Year == year)?.FirstOrDefault()?.Id;
            var leaveTypeEntityList = _unitOfWork.LeaveTypeRepository
                .Find().Include(t => t.LeaveTypeParams).Include("LeaveTypeParams.LeaveTypeParamDetails").ToList();
            var leaveTypeList = _mapper.Map<List<LeaveType>>(leaveTypeEntityList);

            foreach (var leaveType in leaveTypeList)
            {

                var userLeaveTrackEntity = _unitOfWork.UserLeaveTrackRepository
                    .Find(t => t.UserLeaveYearId == userLeaveYearId
                               && t.UserId == userId
                               && t.LeaveTypeId == leaveType.Id)
                    .FirstOrDefault();
                var userLeaveTrack = _mapper.Map<UserLeaveTrack>(userLeaveTrackEntity);
                var userEntity = _unitOfWork.UserRepository
                    .Find(t => t.Id == userId, includeProperties: "UserInformation")
                    .FirstOrDefault();
                var totalLeaveDay = 0f;
                if (userEntity != null)
                {
                    // TODO
                    totalLeaveDay = CalculateNumberOfLeaveDaysBySeniorityPolicy(leaveType, DateTime.Now, GenderType.Male, userLeaveTrack);
                }
                // update current remaining leave day
                if (userLeaveTrackEntity != null)
                {
                    userLeaveTrackEntity.RemainLeaveDay = totalLeaveDay;
                    userLeaveTrackEntity.TotalLeaveDay = totalLeaveDay;
                    userLeaveTrackEntity.Modified = DateTime.UtcNow;
                    _unitOfWork.Save();
                }
            }

        }
        public UserLeaveTrack GetUserLeaveTrackByUserAndYearAndLeaveType(int userId, int yearId, int leaveTypeId)
        {

            var result = GetUserLeaveTrackByUserAndYearAndLeaveTypeContent(userId, yearId, leaveTypeId);
            return result;

        }

        public UserLeaveTrack GetUserLeaveTrackById(Guid userLeaveTrackId)
        {

            var item = _unitOfWork.UserLeaveTrackRepository.Find(t => t.Id == userLeaveTrackId, includeProperties: "UserLeaveYear").FirstOrDefault();
            var result = _mapper.Map<UserLeaveTrack>(item);
            return result;

        }

        public UserLeaveTrack GetUserLeaveTrackByUserAndYearAndLeaveTypePublic(int userId, int yearId, int leaveTypeId)
        {
            var result = GetUserLeaveTrackByUserAndYearAndLeaveTypeContent(userId, yearId, leaveTypeId);
            return result;
        }

        public List<UserLeaveTrack> GetUserLeaveTrackByUserAndYear(int userId, int year)
        {
            var result = _unitOfWork.UserLeaveTrackRepository
                .Find(t => t.UserId == userId && t.UserLeaveYear.Year == year)
                .ToList();

            var item = _mapper.Map<List<UserLeaveTrack>>(result);
            return item;
        }

        public List<UserLeaveTrack> GetUserLeaveTrackByYear(int year)
        {
            var result = _unitOfWork.UserLeaveTrackRepository
                .Find(t => t.UserLeaveYear.Year == year)
                .ToList();

            var item = _mapper.Map<List<UserLeaveTrack>>(result);
            return item;
        }
        public List<UserLeaveTrack> GetUserLeaveTrackByYearAndManagerId(int year, int managerId)
        {
            var userIdsWithThisManager = this._userService.GetUsersWithManagerId(managerId).Select(user => user.Id);

            if (!userIdsWithThisManager.Any())
            {
                return new List<UserLeaveTrack>();
            }

            var result = _unitOfWork.UserLeaveTrackRepository
                .Find(t => userIdsWithThisManager.Contains(t.UserId) && t.UserLeaveYear.Year == year)
                .ToList();

            var item = _mapper.Map<List<UserLeaveTrack>>(result);
            return item;
        }

        public List<UserLeaveTrack> GetFullUserLeaveTrackByUserAndYear(int userId, int yearId)
        {
            var result = _unitOfWork.UserLeaveTrackRepository
                            .Find(t => t.UserId == userId && t.UserLeaveYearId == yearId)
                            //.Include(t => t.User)
                            .Include(t => t.LeaveType)
                            .Include(t => t.UserLeaveYear)
                            .ToList();
            // Cannot include object properties

            var item = _mapper.Map<List<UserLeaveTrack>>(result);
            return item;

        }

        public UserLeaveTrack UpdateUserLeaveTrackTotalLeaveDay(UserLeaveTrackPost userLeaveTrackPost,
            Guid actionUserId, Guid modifiedId)
        {
            var userLeaveTrackEntity = _unitOfWork.UserLeaveTrackRepository
                .Find(t => t.Id == userLeaveTrackPost.Id)
                .FirstOrDefault();
            // add history for updating
            if (userLeaveTrackEntity != null)
            {
                var userLeaveTrackHistory = new UserLeaveTrackHistory
                {
                    Id = Guid.NewGuid(),
                    UserLeaveTrackId = userLeaveTrackEntity.Id,
                    OldValue = userLeaveTrackEntity.TotalLeaveDay,
                    NewValue = userLeaveTrackPost.TotalLeaveDay,
                    ModifiedId = modifiedId,
                    Created = DateTime.UtcNow,
                    CreatedBy = actionUserId,
                    Modified = DateTime.UtcNow,
                    ModifiedBy = actionUserId,
                };
            }

            // update user leave track
            if (userLeaveTrackEntity != null)
            {
                var numberOfDayLeave = _unitOfWork.LeaveRequestRepository.Find(x => x.RequestForUserId == userLeaveTrackEntity.UserId && x.LeaveTypeId == userLeaveTrackEntity.LeaveTypeId && (x.Status != ApprovalStep.Rejected && !x.IsRemoved)).ToList()?.Sum(x => x.NumberOfDay) ?? 0;

                userLeaveTrackEntity.RemainLeaveDay = userLeaveTrackPost.TotalLeaveDay - numberOfDayLeave;
                userLeaveTrackEntity.TotalLeaveDay = userLeaveTrackPost.TotalLeaveDay;
                userLeaveTrackEntity.Modified = DateTime.UtcNow;
                _unitOfWork.Save();
            }

            return _mapper.Map<UserLeaveTrack>(userLeaveTrackEntity);

        }

        public float GetRemainLeaveDayOfYear(int userId, int leaveTypeId, int year)
        {

            var result = 0f;
            //var userLeaveYearId = _unitOfWork.UserLeaveYearRepository.
            //    Find(t => t.Year == year).FirstOrDefault()?.Id;
            //var userLeaveTrack = _unitOfWork.UserLeaveTrackRepository
            //    .Find(t => t.UserId == userId
            //               && t.LeaveTypeId == leaveTypeId
            //               && t.UserLeaveYearId == userLeaveYearId)
            //    .FirstOrDefault();

            var query = from uLeaveYear in _unitOfWork.UserLeaveYearRepository.DbSet()
                        join uLeaveTrack in _unitOfWork.UserLeaveTrackRepository.DbSet()
                        on uLeaveYear.Id equals uLeaveTrack.UserLeaveYearId
                        where uLeaveYear.Year == year
                              && uLeaveTrack.UserId == userId
                              && uLeaveTrack.LeaveTypeId == leaveTypeId
                        select uLeaveTrack;
            var userLeaveTrack = query.FirstOrDefault();

            if (userLeaveTrack != null)
            {
                // do not get negative number
                result = userLeaveTrack.RemainLeaveDay > 0f
                    ? userLeaveTrack.RemainLeaveDay
                    : result;
            }
            return result;

        }

        public Guid AddUserLeaveTrackHistory(UserLeaveTrackHistory userLeaveTrackHistory)
        {
            var userLeaveTrackHistoryEntity = _mapper.Map<PAS.Repositories.DataModel.UserLeaveTrackHistory>(userLeaveTrackHistory);
            userLeaveTrackHistoryEntity.Created = DateTime.UtcNow;
            userLeaveTrackHistoryEntity.Modified = DateTime.UtcNow;
            _unitOfWork.UserLeaveTrackHistoryRepository.Add(userLeaveTrackHistoryEntity);
            _unitOfWork.Save();
            return userLeaveTrackHistoryEntity.Id;

        }

        public List<UserLeaveTrackHistory> GetUserLeaveTrackHistoryByLeaveRequestId(Guid leaveRequestId)
        {
            var result = _unitOfWork.UserLeaveTrackHistoryRepository.
                    Find(t => t.LeaveRequestId == leaveRequestId).OrderByDescending(t => t.UserLeaveTrack.UserLeaveYear.Year).Take(2).ToList();

            var item = _mapper.Map<List<UserLeaveTrackHistory>>(result);
            return item;
        }

        public UserLeaveTrack UpdateEditedRequest(LeaveRequestPost leaveRequest, int year)
        {

            UserLeaveTrack userLeaveTrack = null;
            var authoringSiteId = _unitOfWork.LeaveTypeRepository
                .Find(t => t.Id == leaveRequest.LeaveTypeId)
                .FirstOrDefault()?.AuthoringSiteId;

            var userLeaveYear = _unitOfWork.UserLeaveYearRepository
                .Find(t => t.Year == year, includeProperties: "Holidays")
                .FirstOrDefault();

            var holidayList = _mapper.Map<List<Holiday>>(userLeaveYear?.Holidays);
            var leaveRequestPeriodList = GetLeaveRequestPeriodsByUserId(leaveRequest.RequestForUserId)
                .Where(t => t.Id != leaveRequest.Id)
                .ToList(); // except current leave request id
                           // update 

            var foundUserLeaveTrack = _unitOfWork.UserLeaveTrackRepository
                .Find(t => t.UserId == leaveRequest.RequestForUserId
                           && t.LeaveTypeId == leaveRequest.LeaveTypeId
                           && t.UserLeaveYearId == userLeaveYear.Id)
                .FirstOrDefault();
            if (foundUserLeaveTrack != null)
            {
                foundUserLeaveTrack.RemainLeaveDay = leaveRequest.EditRemainLeaveDay;
                foundUserLeaveTrack.Modified = DateTime.UtcNow;
                _unitOfWork.Save();
                userLeaveTrack = _mapper.Map<UserLeaveTrack>(foundUserLeaveTrack);
            }
            return userLeaveTrack;

        }

        public List<UserLeaveTrack> UpdateOldLeaveBetweenResetAndApplyDate(List<UserLeaveTrackHistory> listOldUserLeaveTrackHistories, LeaveRequestPost leaveRequest)
        {

            List<PAS.Repositories.DataModel.UserLeaveTrack> listUserLeaveTrack = new List<PAS.Repositories.DataModel.UserLeaveTrack>();
            float numberOfDays = 0f;
            for (int i = 0; i < listOldUserLeaveTrackHistories.Count; i++)
            {
                LeaveRequest currentLeaveRequest = new LeaveRequest();
                currentLeaveRequest.Id = leaveRequest.Id;

                var authoringSiteId = _unitOfWork.LeaveTypeRepository
                .Find(t => t.Id == leaveRequest.LeaveTypeId)
                .FirstOrDefault()?.AuthoringSiteId;
                int? userLeaveYearId = null;
                if (i == 0)
                {
                    var userLeaveYear = _unitOfWork.UserLeaveYearRepository
                    .Find(t => t.Year == DateTime.UtcNow.Year + 1, includeProperties: "Holidays")
                    .FirstOrDefault();
                    userLeaveYearId = userLeaveYear.Id;
                }
                if (i == 1)
                {
                    var userLeaveYear = _unitOfWork.UserLeaveYearRepository
                    .Find(t => t.Year == DateTime.UtcNow.Year, includeProperties: "Holidays")
                    .FirstOrDefault();
                    userLeaveYearId = userLeaveYear.Id;
                }
                var foundUserLeaveTrack = _unitOfWork.UserLeaveTrackRepository
                 .Find(t => t.UserId == leaveRequest.RequestForUserId
                            && t.LeaveTypeId == leaveRequest.LeaveTypeId
                            && t.UserLeaveYearId == userLeaveYearId)
                 .FirstOrDefault();
                numberOfDays = foundUserLeaveTrack.RemainLeaveDay + (listOldUserLeaveTrackHistories[i].OldValue - listOldUserLeaveTrackHistories[i].NewValue);
                if (foundUserLeaveTrack != null)
                {
                    foundUserLeaveTrack.RemainLeaveDay = numberOfDays;
                    foundUserLeaveTrack.Modified = DateTime.UtcNow;
                    _unitOfWork.Save();
                    listUserLeaveTrack.Add(foundUserLeaveTrack);
                }
            }
            return _mapper.Map<List<UserLeaveTrack>>(listUserLeaveTrack);

        }
        #region Private Methods

        private UserLeaveTrack GetUserLeaveTrackByUserAndYearAndLeaveTypeContent(int userId, int yearId, int leaveTypeId)
        {
            var result = _unitOfWork.UserLeaveTrackRepository.
               Find(t => t.UserId == userId
               && t.UserLeaveYearId == yearId
               && t.LeaveTypeId == leaveTypeId)
               .FirstOrDefault();

            return _mapper.Map<UserLeaveTrack>(result);
        }

        private float CalculateLeaveDayBaseOnPreviousRemaining(LeaveType leaveType,
            float currentRemainingLeaveDay,
            float previousRemainingLeaveDay)
        {
            // get actual previous remaining leave day
            if (previousRemainingLeaveDay > leaveType.MaximumNumberOfReservedDay)
            {
                previousRemainingLeaveDay = leaveType.MaximumNumberOfReservedDay;
            }
            // increase the number of leave days basing on remaining days of previous year
            if ((currentRemainingLeaveDay + previousRemainingLeaveDay) < leaveType.MaximumNumberOfDay)
            {
                currentRemainingLeaveDay += previousRemainingLeaveDay;
            }
            else
            {
                currentRemainingLeaveDay = leaveType.MaximumNumberOfDay;
            }

            return currentRemainingLeaveDay;
        }

        #endregion
    }
}