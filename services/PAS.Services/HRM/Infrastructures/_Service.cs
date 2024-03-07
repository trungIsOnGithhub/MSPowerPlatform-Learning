using AutoMapper;
using Microsoft.SharePoint.Client;
using PAS.Model.Enum.HRM;
using PAS.Model.HRM;
using PAS.Repositories;
using PAS.Repositories.HRM;
using PAS.Repositories.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PAS.Services.HRM.Infrastructures
{
    public interface IBaseService
    {
        List<LeaveRequestPeriod> GetLeaveRequestPeriodsByUserId(int userId);

        float CalculateNumberOfLeaveDays(DateTime startDate,
            DateTime endDate, List<Holiday> holidayList,
            List<LeaveRequestPeriod> leaveRequestPeriodList,
            DayLeaveType dayLeaveType = DayLeaveType.FullDay,
            Boolean isEditRequest = false);

        float CalculateNumberOfLeaveDaysBySeniorityPolicy(LeaveType leaveType,
            DateTime startWorkingDate, GenderType gender, UserLeaveTrack userLeaveTrack = null);
        bool IsHalfDay(DateTime startDate,
            DateTime endDate, List<Holiday> holidayList, List<LeaveRequestPeriod> leaveRequestPeriodList,
            DayLeaveType dayLeaveType = DayLeaveType.FullDay, Boolean isEditRequest = false);
    }

    public abstract class BaseService
    {
        private const int FILE_PASSWORD_LENGTH = 10;
        protected readonly IHRMUnitOfWork _unitOfWork;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="unitOfWork"></param>
        protected BaseService(IHRMUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Private Methods

        private bool IsConditionSatisfied(LeaveTypeParamDetail leaveTypeParamDetail, DateTime startWorkingDate,
            GenderType gender)
        {
            var paramsString = leaveTypeParamDetail.Params;
            // Seniority Month
            if (leaveTypeParamDetail.Type == LeaveTypeParamDetailType.SeniorityMonth.ToString())
            {
                var parameters = paramsString.Split(';');
                var minMonth = int.Parse(parameters[0]);
                var maxMonth = int.Parse(parameters[1]);

                var numberOfDayPerMonth = 30; // any month has 30 days
                var numberOfWorkingDays = (DateTime.UtcNow - startWorkingDate).Days;

                if (minMonth * numberOfDayPerMonth <= numberOfWorkingDays
                    && numberOfWorkingDays < maxMonth * numberOfDayPerMonth)
                    return true;
            }

            // Start Working Month
            if (leaveTypeParamDetail.Type == LeaveTypeParamDetailType.StartWorkingMonth.ToString())
            {
                var parameters = paramsString.Split(';');
                var minMonth = int.Parse(parameters[0]);
                var maxMonth = int.Parse(parameters[1]);

                var monthInYearOfStartWorkingDate = startWorkingDate.Month;

                if (minMonth <= monthInYearOfStartWorkingDate && monthInYearOfStartWorkingDate <= maxMonth)
                    return true;
            }

            // Employee Gender
            if (leaveTypeParamDetail.Type == LeaveTypeParamDetailType.EmployeeGender.ToString())
            {
                var genderString = paramsString;

                if (genderString == gender.ToString())
                    return true;
            }

            return false;
        }

        #endregion

        #region Commons

        /// <summary>
        ///     Get leave request period list of specific user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<LeaveRequestPeriod> GetLeaveRequestPeriodsByUserId(int userId)
        {
            //var query = from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
            //            where leaveRequest.RequestForUserId == userId
            //            && !leaveRequest.IsRemoved
            //            select leaveRequest;

            var query = from leaveRequest in _unitOfWork.LeaveRequestRepository.DbSet()
                        where leaveRequest.RequestForUserId == userId
                        && !leaveRequest.IsRemoved
                        select leaveRequest;

            var result = new List<LeaveRequestPeriod>();
            foreach (var leaveRequest in query.ToList())
            {
                if (leaveRequest.Status != ApprovalStep.Pending &&
                    leaveRequest.Status != ApprovalStep.Approved) continue;
                var item = new LeaveRequestPeriod
                {
                    Id = leaveRequest.Id,
                    DayLeaveType = leaveRequest.DayLeaveType,
                    StartDate = leaveRequest.StartDate,
                    EndDate = leaveRequest.EndDate,
                    Status = leaveRequest.Status
                };
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        ///     Calculate the number of leave day in a leave request
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="holidayList"></param>
        /// <param name="leaveRequestPeriodList"></param>
        /// <param name="dayLeaveType"></param>
        /// <returns></returns>
        public float CalculateNumberOfLeaveDays(DateTime startDate,
            DateTime endDate, List<Holiday> holidayList, List<LeaveRequestPeriod> leaveRequestPeriodList,
            DayLeaveType dayLeaveType = DayLeaveType.FullDay, Boolean isEditRequest = false)
        {
            // eliminate time from date instance 
            var start = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var end = new DateTime(endDate.Year, endDate.Month, endDate.Day);
            var days = 0.0f;
            if (dayLeaveType == DayLeaveType.FullDay)
            {
                days = (float)end.Subtract(start).Days + 1;
            }
            else
            {
                days = 0.5f; // assume this case always run well
            }
            //
            var checkedDay = startDate;
            while (checkedDay <= endDate)
            {
                // holiday
                var isInHolidayInterval = holidayList.Any(t =>
                {
                    var holidayStart = new DateTime(t.StartDate.Year, t.StartDate.Month, t.StartDate.Day);
                    var holidayEnd = new DateTime(t.EndDate.Year, t.EndDate.Month, t.EndDate.Day);
                    return holidayStart <= checkedDay && checkedDay <= holidayEnd;
                });
                // previous leave request period
                var isInLeaveRequestPeriod = false;
                var isInLeaveRequestPeriodCount = 0;
                var isFullDay = false;
                foreach (var leaveRequestPeriod in leaveRequestPeriodList)
                {
                    var startD = new DateTime(leaveRequestPeriod.StartDate.Year, leaveRequestPeriod.StartDate.Month,
                        leaveRequestPeriod.StartDate.Day);
                    var endD = new DateTime(leaveRequestPeriod.EndDate.Year, leaveRequestPeriod.EndDate.Month,
                        leaveRequestPeriod.EndDate.Day);
                    if (false == isEditRequest)
                    {
                        if (startD <= checkedDay && checkedDay <= endD)
                        {
                            isInLeaveRequestPeriod = true;
                            isInLeaveRequestPeriodCount++;
                            isFullDay = (leaveRequestPeriod.DayLeaveType == DayLeaveType.FullDay);
                        }
                    }
                }
                if (isInLeaveRequestPeriodCount == 2) // two half of day -> full day
                {
                    isFullDay = true;
                }
                // weekends
                var isInWeekend = (checkedDay.DayOfWeek == DayOfWeek.Saturday || checkedDay.DayOfWeek == DayOfWeek.Sunday);

                //
                if (isInWeekend || isInHolidayInterval)
                {
                    days = days - 1f;

                }
                else if (isInLeaveRequestPeriod)
                {
                    if (isFullDay)
                    {
                        days = days - 1f;
                    }
                    else
                    {
                        days = days - 0.5f;
                    }
                }
                checkedDay = checkedDay.AddDays(1);
            }

            return days;
        }
        public float CalculateNumberOfLeaveDaysExcel(DateTime startDate,
           DateTime endDate, List<Holiday> holidayList, List<LeaveRequestPeriod> leaveRequestPeriodList,
           DayLeaveType dayLeaveType = DayLeaveType.FullDay)
        {
            // eliminate time from date instance 
            var start = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var end = new DateTime(endDate.Year, endDate.Month, endDate.Day);
            var days = 0.0f;
            if (dayLeaveType == DayLeaveType.FullDay)
            {
                days = (float)end.Subtract(start).Days + 1;
            }
            else
            {
                days = 0.5f; // assume this case always run well
            }
            //
            var checkedDay = startDate;
            while (checkedDay <= endDate)
            {
                // holiday
                var isInHolidayInterval = holidayList.Any(t =>
                {
                    var holidayStart = new DateTime(t.StartDate.Year, t.StartDate.Month, t.StartDate.Day);
                    var holidayEnd = new DateTime(t.EndDate.Year, t.EndDate.Month, t.EndDate.Day);
                    return holidayStart <= checkedDay && checkedDay <= holidayEnd;
                });
                // previous leave request period
                var isInLeaveRequestPeriod = false;
                var isInLeaveRequestPeriodCount = 0;
                var isFullDay = false;
                foreach (var leaveRequestPeriod in leaveRequestPeriodList)
                {
                    var startD = new DateTime(leaveRequestPeriod.StartDate.Year, leaveRequestPeriod.StartDate.Month,
                        leaveRequestPeriod.StartDate.Day);
                    var endD = new DateTime(leaveRequestPeriod.EndDate.Year, leaveRequestPeriod.EndDate.Month,
                        leaveRequestPeriod.EndDate.Day);
                    if (startD <= checkedDay && checkedDay <= endD)
                    {
                        isInLeaveRequestPeriod = true;
                        isInLeaveRequestPeriodCount++;
                        isFullDay = (leaveRequestPeriod.DayLeaveType == DayLeaveType.FullDay);
                    }
                }
                if (isInLeaveRequestPeriodCount == 2) // two half of day -> full day
                {
                    isFullDay = true;
                }
                // weekends
                var isInWeekend = (checkedDay.DayOfWeek == DayOfWeek.Saturday || checkedDay.DayOfWeek == DayOfWeek.Sunday);

                // 
                if (isInWeekend || isInHolidayInterval || (isInLeaveRequestPeriod && isFullDay))
                {
                    days = days - 1f;
                }
                checkedDay = checkedDay.AddDays(1);
            }

            return days;
        }
        /// <summary>
        ///     Calculate the number of leave day
        /// </summary>
        /// <param name="leaveType">Including: LeaveTypeParams, LeaveTypeParamDetails</param>
        /// <param name="startWorkingDate"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        public float CalculateNumberOfLeaveDaysBySeniorityPolicy(LeaveType leaveType,
            DateTime startWorkingDate, GenderType gender, UserLeaveTrack userLeaveTrack = null)
        {
            float result = 0f;
            if (userLeaveTrack != null)
            {
                result = userLeaveTrack.RemainLeaveDay;
            }
            else
            {
                result = leaveType.BaseNumberOfDay;
            }


            var leaveTypeParamList = leaveType.LeaveTypeParams;
            if (!(leaveTypeParamList?.Count > 0)) return result;
            // policies
            foreach (var leaveTypeParam in leaveTypeParamList)
            {
                var extraDay = leaveTypeParam.NumberOfDay;
                var leaveTypeParamDetailList = leaveTypeParam.LeaveTypeParamDetails;

                var isAllSatisfied = true;
                // check every condition in policy
                foreach (var leaveTypeParamDetail in leaveTypeParamDetailList)
                {
                    if (IsConditionSatisfied(leaveTypeParamDetail, startWorkingDate, gender))
                    {
                        continue;
                    }
                    isAllSatisfied = false;
                    break;
                }

                // all conditions are satisfied
                if (!isAllSatisfied) continue;
                result = leaveType.BaseNumberOfDay + extraDay < leaveType.MaximumNumberOfDay
                    ? leaveType.BaseNumberOfDay + extraDay
                    : leaveType.MaximumNumberOfDay;
                break;
            }

            return result;
        }
        public string RandomPassword(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
        public byte[] GetPasswordHash(ClientContext ctx)
        {
            //string secretKey = ConfigurationManager.AppSettings["ClientSecret"];
            //temporary fix
            string secretKey = "qHX05z2FGo1q1XGMsZ6AxQCYVEuiKxsNX8Fhru31IIw=";
            string salt = string.Empty;

            List list = ctx.Web.Lists.GetByTitle(Constants.HRFolder.GeneralSettingListName);

            var query = new CamlQuery() { ViewXml = $"<View><Query><Where><Eq><FieldRef Name='SettingKey' /><Value Type='Text'>{Constants.SPGeneralSetting.SaltKey}</Value></Eq></Where></Query></View>" };
            var settingItems = list.GetItems(query);
            ctx.Load(settingItems);
            ctx.ExecuteQuery();
            if (settingItems != null)
            {
                var settingItem = settingItems.FirstOrDefault();
                salt = settingItem["SettingValue"].ToString();
            }
            return Crypto.GetHashKey(secretKey, salt);
        }
        public string GenerateFilePassword(byte[] hashPassword)
        {

            var randomPassword = RandomPassword(FILE_PASSWORD_LENGTH, true);
            var encryptPassword = Crypto.Encrypt(hashPassword, randomPassword);
            return encryptPassword;
        }
        public string GetDecryptedFilePassword(byte[] hashKey, string encryptedPassword)
        {
            //string secretKey = ConfigurationManager.AppSettings["ClientSecret"];

            var decryptPassword = Crypto.Decrypt(hashKey, encryptedPassword);
            return decryptPassword;
        }
        public bool IsHalfDay(DateTime startDate,
            DateTime endDate, List<Holiday> holidayList, List<LeaveRequestPeriod> leaveRequestPeriodList,
            DayLeaveType dayLeaveType = DayLeaveType.FullDay, Boolean isEditRequest = false)
        {
            // eliminate time from date instance 
            var start = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var end = new DateTime(endDate.Year, endDate.Month, endDate.Day);
            var checkedDay = startDate;
            while (checkedDay <= endDate)
            {
                // previous leave request period
                var isInLeaveRequestPeriod = false;
                var isInLeaveRequestPeriodCount = 0;
                var isFullDay = false;
                foreach (var leaveRequestPeriod in leaveRequestPeriodList)
                {
                    var startD = new DateTime(leaveRequestPeriod.StartDate.Year, leaveRequestPeriod.StartDate.Month,
                        leaveRequestPeriod.StartDate.Day);
                    var endD = new DateTime(leaveRequestPeriod.EndDate.Year, leaveRequestPeriod.EndDate.Month,
                        leaveRequestPeriod.EndDate.Day);


                    if (startD <= checkedDay && checkedDay <= endD)
                    {
                        isInLeaveRequestPeriod = true;
                        isInLeaveRequestPeriodCount++;
                        isFullDay = (leaveRequestPeriod.DayLeaveType == DayLeaveType.FullDay);
                    }
                }
                if (isInLeaveRequestPeriodCount == 2) // two half of day -> full day
                {
                    isFullDay = true;
                }
                // weekends
                var isInWeekend = (checkedDay.DayOfWeek == DayOfWeek.Saturday || checkedDay.DayOfWeek == DayOfWeek.Sunday);

                if (isInLeaveRequestPeriod && !isFullDay && dayLeaveType == DayLeaveType.FullDay)
                {
                    return true;
                }
                checkedDay = checkedDay.AddDays(1);
            }

            return false;
        }
        #endregion
    }
}