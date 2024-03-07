using AutoMapper;
using PAS.Model.HRM;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services.HRM
{
    public interface IUserLeaveYearService : IBaseService
    {
        UserLeaveYear GetUserLeaveYearByYear(Guid authoringSiteId, int year);
        UserLeaveYear AddUserLeaveYear(UserLeaveYear userLeaveYear);
        UserLeaveYear GetLatestUserLeaveYear(Guid authoringSiteId);
        List<UserLeaveYear> GetUserLeaveYearByAuthoringSite(Guid authoringSiteId);
    }

    public class UserLeaveYearService : BaseService, IUserLeaveYearService
    {
        private readonly IMapper _mapper;
        public UserLeaveYearService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public UserLeaveYear GetUserLeaveYearByYear(Guid authoringSiteId, int year)
        {
            var result = _unitOfWork.UserLeaveYearRepository.
                Find(t => t.Year == year)
                .FirstOrDefault();

            var item = _mapper.Map<UserLeaveYear>(result);
            return item;
        }

        public List<UserLeaveYear> GetUserLeaveYearByAuthoringSite(Guid authoringSiteId)
        {
            var result = _unitOfWork.UserLeaveYearRepository.
                Find()
                .ToList();

            var item = _mapper.Map<List<UserLeaveYear>>(result);

            return item;
        }
        public UserLeaveYear AddUserLeaveYear(UserLeaveYear userLeaveYear)
        {
           
                var existUserLeaveYear = _unitOfWork.UserLeaveYearRepository.Find(t => t.Year == userLeaveYear.Year).Any();
                if (!existUserLeaveYear)
                {
                    var userLeaveYearEntity = _mapper.Map<PAS.Repositories.DataModel.UserLeaveYear>(userLeaveYear);
                    userLeaveYearEntity.Created = DateTime.UtcNow;
                    userLeaveYearEntity.Modified = DateTime.UtcNow;
                    _unitOfWork.UserLeaveYearRepository.Add(userLeaveYearEntity);
                    _unitOfWork.Save();
                    userLeaveYear.Id = userLeaveYearEntity.Id;
                }
                return userLeaveYear;
        }

        public UserLeaveYear GetLatestUserLeaveYear(Guid authoringSiteId)
        {
            var result = _unitOfWork.UserLeaveYearRepository
                .Find()
                .OrderByDescending(t => t.Id).Take(1)
                .FirstOrDefault();

            var item = _mapper.Map<UserLeaveYear>(result);

            return item;
        }

        #region Private Methods

        #endregion
    }
}