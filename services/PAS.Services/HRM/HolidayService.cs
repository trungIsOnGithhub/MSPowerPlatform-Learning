using AutoMapper;
using PAS.Model.HRM;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace PAS.Services.HRM
{
    public interface IHolidayService : IBaseService
    {
        Task<List<Holiday>> GetAllHolidays();
        List<Holiday> GetAllHolidayByAuthoringSiteId(Guid authoringSiteId);
        List<Holiday> GetHolidayByAuthoringSiteIdAndYear(Guid authoringSiteId, int year);
        Holiday AddHoliday(Holiday holiday);
        Holiday AddHoliday(HolidayPost holiday);
        Holiday UpdateHoliday(HolidayPost holiday);
        Holiday GetHolidayById(int holidayId);
        List<Holiday> GetAllLastYearHolidayByAuthoringSiteId(Guid authoringSiteId);
        List<Holiday> GetUpcomingHolidayByAuthoringSitesIdPublic(UpcomingHolidaysSetting upcomingHolidaysSetting);
        Holiday RemoveHoliday(int holidayId);
    }

    public class HolidayService : BaseService, IHolidayService
    {
        private IMapper _mapper;
        public HolidayService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public List<Holiday> GetAllLastYearHolidayByAuthoringSiteId(Guid authoringSiteId)
        {

            var latestUserLeaveYear = _unitOfWork.UserLeaveYearRepository
               .Find()
               .Include(t => t.Holidays.Select(h => h.HolidayLocalizations))
               .OrderByDescending(t => t.Id).Take(2)
               .ToList();
            var items = _mapper.Map<List<Holiday>>(latestUserLeaveYear[1]?.Holidays);
            return items;

        }

        public List<Holiday> GetHolidayByAuthoringSiteIdAndYear(Guid authoringSiteId, int year)
        {
            var userLeaveYear = _unitOfWork.UserLeaveYearRepository
                .Find(t => t.Year == year, includeProperties: "Holidays")
                .FirstOrDefault();
            var holidayList = _mapper.Map<List<Holiday>>(userLeaveYear?.Holidays);
            return holidayList;

        }

        public List<Holiday> GetAllHolidayByAuthoringSiteId(Guid authoringSiteId)
        {
                // after 'Leave Reset Day', get the latest items
                var latestUserLeaveYear = _unitOfWork.UserLeaveYearRepository
                    .Find(t => true, includeProperties: "Holidays")
                    .Include(t => t.Holidays.Select(z => z.HolidayLocalizations))
                    .OrderByDescending(t => t.Id).Take(1)
                    .FirstOrDefault();
                var items = _mapper.Map<List<Holiday>>(latestUserLeaveYear?.Holidays);
                items = LocalizeValueForList(items);
                return items;
         
        }

        public async Task<List<Holiday>> GetAllHolidays()
        {
            // after 'Leave Reset Day', get the latest items
            var items = await _unitOfWork.HolidayRepository.GetAllAsync();
            var result = _mapper.Map<List<Holiday>>(items);
            result = LocalizeValueForList(result);
            return result;
        }

        public Holiday GetHolidayById(int holidayId)
        {
                var result = _unitOfWork.HolidayRepository.Find(t => t.Id == holidayId, includeProperties: "HolidayLocalizations")?.FirstOrDefault();
                var items = _mapper.Map<Holiday>(result);
                items.Name = items.HolidayLocalizations[0].Name;
                items.Description = items.HolidayLocalizations[0].Description;
                return items;
          
        }

        public Holiday AddHoliday(Holiday holiday)
        {
                var holidayEntity = _mapper.Map<PAS.Repositories.DataModel.Holiday>(holiday);
                holidayEntity.Created = DateTime.UtcNow;
                holidayEntity.Modified = DateTime.UtcNow;
                _unitOfWork.HolidayRepository.Add(holidayEntity);
                _unitOfWork.Save(); // TODO: LanguageCode: en-US
                return holiday;
  
        }

        public Holiday AddHoliday(HolidayPost holiday)
        {
                var newHoliday = _mapper.Map<Holiday>(holiday);
                newHoliday.HolidayLocalizations.Add(new HolidayLocalization
                {
                    Description = holiday.Description,
                    Name = holiday.Name,
                    LanguageCode = Constants.LanguageCode.English
                });
                var holidayEntity = _mapper.Map<PAS.Repositories.DataModel.Holiday>(newHoliday);
                holidayEntity.Created = DateTime.UtcNow;
                holidayEntity.Modified = DateTime.UtcNow;
                _unitOfWork.HolidayRepository.Add(holidayEntity);
                _unitOfWork.Save(); // TODO: LanguageCode: en-US
                return newHoliday;
   
        }

        public Holiday UpdateHoliday(HolidayPost holiday)
        {
                var foundHolidayLocalization = _unitOfWork.HolidayLocalizationRepository.Find(t => t.HolidayId == holiday.Id && t.LanguageCode == Constants.LanguageCode.English).FirstOrDefault();
                foundHolidayLocalization.Name = holiday.Name;
                foundHolidayLocalization.Description = holiday.Description;
                _unitOfWork.Save();
                var foundHoliday = _unitOfWork.HolidayRepository.Find(t => t.Id == holiday.Id).FirstOrDefault();
                foundHoliday.StartDate = holiday.StartDate;
                foundHoliday.EndDate = holiday.EndDate;
                foundHoliday.DayBeforeReminding = holiday.DayBeforeReminding;
                _unitOfWork.Save();
                var newHoliday = _mapper.Map<Holiday>(foundHoliday);
                return newHoliday;
        }

        public List<Holiday> GetUpcomingHolidayByAuthoringSitesIdPublic(UpcomingHolidaysSetting upcomingHolidaysSetting)
        {
            var result = GetUpcomingHolidayByAuthoringSitesIdContent(upcomingHolidaysSetting);
            return result;
        }

        public Holiday RemoveHoliday(int holidayId)
        {
                var foundHoliday = _unitOfWork.HolidayRepository.Find(t => t.Id == holidayId).FirstOrDefault();
                _unitOfWork.HolidayRepository.Delete(foundHoliday);
                _unitOfWork.Save();
                return null;

        }
        #region Private Methods

        private List<Holiday> LocalizeValueForList(List<Holiday> items)
        {
            var result = items.Select(t =>
            {
                if (t.HolidayLocalizations.Any())
                {
                    t.Name = t.HolidayLocalizations[0].Name; // TODO: default language code: en-US
                    t.Description = t.HolidayLocalizations[0].Description;
                }
                return t;
            })?.ToList();

            return result;
        }

        private List<Holiday> GetUpcomingHolidayByAuthoringSitesIdContent(UpcomingHolidaysSetting upcomingHolidaysSetting)
        {
                int numberOfDays = upcomingHolidaysSetting.numberOfItems ?? 1;
                var items = new List<PAS.Repositories.DataModel.Holiday>();
                // after 'Leave Reset Day', get the latest items
                foreach (var authoringSiteId in upcomingHolidaysSetting.authoringSitesId)
                {
                    var latestUserLeaveYear = _unitOfWork.UserLeaveYearRepository
                     .Find(t => true, includeProperties: "Holidays")
                     .Include(t => t.Holidays.Select(z => z.HolidayLocalizations))
                     .OrderBy(t => t.Year).Take(10).ToList();

                    foreach (var leaveYear in latestUserLeaveYear)
                    {
                        if (leaveYear.Year >= DateTime.Now.Year)
                        {
                            var holidayYear = leaveYear.Holidays.Where(h => h.StartDate >= DateTime.Now.Date).OrderBy(h => h.StartDate).Take(numberOfDays);
                            foreach (var holiday in holidayYear)
                            {
                                if (items.Count < numberOfDays)
                                {
                                    items.Add(holiday);
                                }
                            }
                        }
                    }
                }
                var result = items.Select(holiday =>
                {
                    var holidayModel = _mapper.Map<Holiday>(holiday);
                    return holidayModel;
                }).ToList();
                result = LocalizeValueForList(result);
                return result;

        }

        #endregion
    }
}