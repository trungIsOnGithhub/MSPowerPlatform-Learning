using PAS.Common;
using PAS.Model;

using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services
{
    public interface IWorkHistoryService
    {
        void TerminateUser(Model.User user);
        void RestoreUser(Model.User user, DateTime ReturnedDate);
        ICollection<DataTransfer.WorkRecordDTO> GetWorkRecord(int userId);
        public void UpdateRecord(DataTransfer.WorkRecordDTO update);
        int CalculateYearOfExperience(int userId);
        int CalculateYearOfService(int userId);
    }

    public class WorkHistoryService : IWorkHistoryService
    {
        private readonly IUserRepository userRepository;
        private readonly IRecordRepository<WorkRecord> recordRepository;


        public WorkHistoryService(IUserRepository userRepository, IRecordRepository<WorkRecord> recordRepository)
        {
            this.userRepository = userRepository;
            this.recordRepository = recordRepository;
        }

        public void TerminateUser(Model.User user)
        {
            userRepository.TerminateUser(user);

            #region Manage Record
            var latestRecord = recordRepository
                .Read(user.Id)
                .OrderByDescending(record => record.EndDate ?? DateTime.MaxValue)
                .FirstOrDefault();

            if (latestRecord == null)
            {
                throw new Exception("Not Enough Data");
            }

            if (latestRecord.EndDate != null)
            {
                latestRecord.EndDate = latestRecord.EndDate < user.FirstTerminatedDate ? user.FirstTerminatedDate : latestRecord.EndDate;
            } else
            {
                latestRecord.EndDate = user.FirstTerminatedDate ?? DateTime.UtcNow;
            }
            
            recordRepository.Update(latestRecord);
            #endregion
        }

        public void RestoreUser(Model.User user, DateTime returnedDate)
        {
            userRepository.RestoreUser(user);
            var userDb = userRepository.GetUserById(user.Id);

            #region Add Record

            recordRepository.Create(new WorkRecord
            {
                User = userDb,
                StartDate = returnedDate,
                EndDate = null
            });
            #endregion
        }


        public ICollection<DataTransfer.WorkRecordDTO> GetWorkRecord(int userId)
        {
            return recordRepository
                .Read(userId)
                .OrderByDescending(record => record.EndDate ?? DateTime.MaxValue)
                .Select(record => new DataTransfer.WorkRecordDTO
                    {
                        Id = record.Id,
                        StartDate = record.StartDate,
                        EndDate = record.EndDate
                    })
                .ToList();
        }

        public void UpdateRecord(DataTransfer.WorkRecordDTO update)
        {
            var record = recordRepository.GetById(update.Id);

            if (record != null)
            {
                record.StartDate = update.StartDate;
                record.EndDate = update.EndDate;
                recordRepository.Update(record);
                return;
            }

            var user = userRepository.GetUserById(update.UserId);

            if (user == null)
            {
                throw new NotFoundException("Undefined Record");
            }

            recordRepository.Create(new WorkRecord
            {
                User = user,
                StartDate = update.StartDate,
                EndDate = update.EndDate
            });
        }


        private double CalculateYearSpan(TimeSpan time)
        {
            return time.TotalDays / 365.25D;
        }

        public int CalculateYearOfExperience(int userId)
        {
            var user = userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new NotFoundException($"A User with ID {userId} Not Found.");
            }
            DateTime currentDate = DateTime.UtcNow;
            // The field "JoinedDate" is for storing FirstDayAtWork. After removing Sync User in the future, please rename it :>
            var firstDayAtWork = user.JoinedDate;
            return Convert.ToInt32(
                Math.Round(CalculateYearSpan(currentDate - firstDayAtWork), MidpointRounding.AwayFromZero)
            );
        }

        public int CalculateYearOfService(int userId)
        {
            DateTime currentDate = DateTime.UtcNow;
            var user = userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new NotFoundException($"A User with ID {userId} Not Found.");
            }

            var records = recordRepository.Read(userId)
                .Where(r => r.StartDate <= currentDate)
                .OrderBy(r => r.StartDate)
                .ToList();

            if (records.Count == 0)
            {
                throw new ApplicationException("Not Enough Data");
            }

            if (records.Last().EndDate > currentDate)
            {
                records.Last().EndDate = currentDate;
            }

            return Convert.ToInt32(Math.Round(records.Aggregate(
                0.0, 
                (prev, next) => 
                    prev + CalculateYearSpan((next.EndDate ?? currentDate) - next.StartDate)
            ), MidpointRounding.AwayFromZero));
        }
    }
}
