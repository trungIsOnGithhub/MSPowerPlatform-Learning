using AutoMapper;
using PAS.Model.HRM;
using PAS.Repositories;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PAS.Services.HRM
{

    public interface ICurrencyService : IBaseService
    {
        CurrencyPaging GetExchangeRateListByFilter(Guid authoringSiteId, int page, int itemsPerPage, SettingFilter filter);
        string GetCurrentCurrency(Guid userId);
        string ChangeCurrentCurrency(Guid userId, string code);
        Currency GetCurrencyById(int id);
        List<Currency> GetAllCurrentCurrency(Guid authoringSiteId);
        List<Currency> GetAllCurrency();
        List<CurrencyDetail> GetExchangeRates(int currencyId);
        List<CurrencyDetail> GetExchangeRateListByAuthoringSite(Guid authoringSiteId);
        void RemoveExchangeRate(int firstCurrencyId, int secondCurrencyId, Guid authoringSite);
        float? GetCurrentExchangeRate(int fromCurrencyId, int toCurrencyId, Guid authoringSiteId);
        CurrencyDetail GetCurrentDetail(int fromCurrencyId, int toCurrencyId, Guid authoringSiteId);
        void UpdateExchangeRate(int firstCurrencyId, int secondCurrencyId, float firstToSecond, float secondToFirst
            , Guid authoringSite);

        bool IsCurrencyDetailExisted(CurrencyDetailPost claimCurrencyDetail);
        void AddCurrencyDetail(CurrencyDetailPost claimCurrencyDetail);
        void UpdateCurrencyDetail(CurrencyDetailPost claimCurrencyDetail);
        void RemoveExchangeRate(int currencyDetailId);
    }
    public class CurrencyService : BaseService, ICurrencyService
    {
        private IMapper _mapper;
        public CurrencyService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        private IEnumerable<PAS.Repositories.DataModel.CurrencyDetail> GetExchangeRateListContent(Guid authoringSiteId)
        {
            var currencyQuery = _unitOfWork.CurrencyDetailRepository.Find(x => x.AuthoringSiteId == authoringSiteId)
                                          .Include(t => t.FromCurrency)
                                          .Include(t => t.ToCurrency)
                                          .OrderByDescending(x => x.FromCurrency.Code)
                                          .AsEnumerable();
            return currencyQuery;
        }

        public CurrencyPaging GetExchangeRateListByFilter(Guid authoringSiteId, int page, int itemsPerPage, SettingFilter filter)
        {
            var currencyPaging = new CurrencyPaging();
            var currencyQuery = GetExchangeRateListContent(authoringSiteId);
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                currencyQuery = currencyQuery.Where(x => x.FromCurrency.Code.ToLower().Contains(filter.SearchString.ToLower())
                || x.ToCurrency.Code.ToLower().Contains(filter.SearchString.ToLower())
                || x.FromCurrency.Name.ToLower().Contains(filter.SearchString.ToLower())
                || x.ToCurrency.Name.ToLower().Contains(filter.SearchString.ToLower()));
            }
            var result = Pagination(currencyQuery, page, itemsPerPage);
            return result;
        }

        public List<CurrencyDetail> GetExchangeRateListByAuthoringSite(Guid authoringSiteId)
        {
            var currencyQuery = GetExchangeRateListContent(authoringSiteId);
            var result = _mapper.Map<List<CurrencyDetail>>(currencyQuery.ToList());
            return result;
        }

        public Currency GetCurrencyById(int id)
        {
            return _unitOfWork.CurrencyRepository.Find(x => x.Id == id, null, "CurrencyLocalizations")
                .Select(x => _mapper.Map<Currency>(x)).FirstOrDefault();
        }

        public List<Currency> GetAllCurrentCurrency(Guid authoringSiteId)
        {
            var fromCurrencyList = _unitOfWork.CurrencyDetailRepository
                .Find(t => t.AuthoringSiteId == authoringSiteId).Include(x => x.FromCurrency).AsEnumerable()
                .Select(x => _mapper.Map<Currency>(x.FromCurrency))
                .GroupBy(t => t.Id)
                .Select(t => t.FirstOrDefault());

            var toCurrencyList = _unitOfWork.CurrencyDetailRepository
                .Find(t => t.AuthoringSiteId == authoringSiteId).Include(x => x.ToCurrency).AsEnumerable()
                .Select(x => _mapper.Map<Currency>(x.ToCurrency))
                .GroupBy(t => t.Id)
                .Select(t => t.FirstOrDefault());

            var currencyList = fromCurrencyList.Union(toCurrencyList).ToList();

            return currencyList;
        }

        public List<Currency> GetAllCurrency()
        {
            var currencyList = _unitOfWork.CurrencyRepository.GetAll();
            var result = _mapper.Map<List<Currency>>(currencyList);
            return result;
        }

        public void UpdateExchangeRate(int firstCurrencyId, int secondCurrencyId, float firstToSecond,
            float secondToFirst, Guid authoringSite)
        {
            var currencyDetails = _unitOfWork.CurrencyDetailRepository
                .Find(x => (
                               (x.FromCurrencyId == firstCurrencyId
                                && x.ToCurrencyId == secondCurrencyId)
                               ||
                               (x.ToCurrencyId == firstCurrencyId
                                && x.FromCurrencyId == secondCurrencyId)
                           )
                           && x.AuthoringSiteId == authoringSite
                ).ToList();
            currencyDetails.ForEach(detail =>
            {
                if (detail.FromCurrencyId == firstCurrencyId)
                {
                    detail.ExchangeRateFrom = firstToSecond;
                }
                else
                {
                    detail.ExchangeRateTo = secondToFirst;
                }
            });
            _unitOfWork.Save();
        }

        public List<CurrencyDetail> GetExchangeRates(int currencyId)
        {
            return _unitOfWork.CurrencyDetailRepository.Find(x => x.FromCurrencyId == currencyId || x.ToCurrencyId == currencyId)
                .Select(x => _mapper.Map<CurrencyDetail>(x)).ToList();
        }

        public string GetCurrentCurrency(Guid userId)
        {
            //var currentCurrency = _context.UserSetting.Where(x => x.UserId == userId && x.Name == "currency").FirstOrDefault();
            //if (currentCurrency == null)
            //{
            //    currentCurrency = new PAS.Repositories.DataModel.UserSetting()
            //    {
            //        Id = Guid.NewGuid(),
            //        Name = "currency",
            //        UserId = userId,
            //        Value = "USD"
            //    };
            //    _context.UserSetting.Add(currentCurrency);
            //    _context.SaveChangesAsync();
            //}
            //return currentCurrency.Value;
            throw new NotImplementedException();
        }

        public string ChangeCurrentCurrency(Guid userId, string currency)
        {
            //var userSettings = _context.UserSetting
            //.Where(x => x.UserId == userId && x.Name == "currency")
            //.Select(x => x).FirstOrDefault();
            //userSettings.Value = currency;

            //_context.SaveChangesAsync();
            //return currency;
            throw new NotImplementedException();
        }

        public bool IsCurrencyDetailExisted(CurrencyDetailPost currencyDetail)
        {

            var result = _unitOfWork.CurrencyDetailRepository
             .Find(x => ((x.FromCurrencyId == currencyDetail.FromCurrencyId
                   && x.ToCurrencyId == currencyDetail.ToCurrencyId) || (x.FromCurrencyId == currencyDetail.ToCurrencyId && x.ToCurrencyId == currencyDetail.FromCurrencyId))
                   && x.AuthoringSiteId == currencyDetail.AuthoringSiteId).Any();
            return result;

        }

        public void AddCurrencyDetail(CurrencyDetailPost currencyDetail)
        {
            var currencyDetailEntity = _mapper.Map<PAS.Repositories.DataModel.CurrencyDetail>(currencyDetail);
            currencyDetailEntity.Created = DateTime.UtcNow;
            currencyDetailEntity.Modified = DateTime.UtcNow;
            currencyDetailEntity.CreatedBy = currencyDetail.ModifiedBy;
            currencyDetailEntity.ModifiedBy = currencyDetail.ModifiedBy;
            _unitOfWork.CurrencyDetailRepository.Add(currencyDetailEntity);
            _unitOfWork.Save();

        }

        public void UpdateCurrencyDetail(CurrencyDetailPost currencyDetail)
        {
            //// Update From To
            var currencyDetailEntityFrom = _unitOfWork.CurrencyDetailRepository
             .Find(x => ((x.FromCurrencyId == currencyDetail.FromCurrencyId
             && x.ToCurrencyId == currencyDetail.ToCurrencyId))
             && x.AuthoringSiteId == currencyDetail.AuthoringSiteId).FirstOrDefault();
            if (currencyDetailEntityFrom != null)
            {
                currencyDetailEntityFrom.ExchangeRateFrom = currencyDetail.ExchangeRateFrom;
                currencyDetailEntityFrom.ExchangeRateTo = currencyDetail.ExchangeRateTo;
                currencyDetailEntityFrom.Modified = DateTime.UtcNow;
                currencyDetailEntityFrom.ModifiedBy = currencyDetail.ModifiedBy;
                _unitOfWork.Save();
            }
            //// Update To From
            ///
            var currencyDetailEntityTo = _unitOfWork.CurrencyDetailRepository
              .Find(x => ((x.FromCurrencyId == currencyDetail.ToCurrencyId && x.ToCurrencyId == currencyDetail.FromCurrencyId))
            && x.AuthoringSiteId == currencyDetail.AuthoringSiteId).FirstOrDefault();
            if (currencyDetailEntityTo != null)
            {
                currencyDetailEntityTo.ExchangeRateFrom = currencyDetail.ExchangeRateTo;
                currencyDetailEntityTo.ExchangeRateTo = currencyDetail.ExchangeRateFrom;
                currencyDetailEntityTo.Modified = DateTime.UtcNow;
                currencyDetailEntityTo.ModifiedBy = currencyDetail.ModifiedBy;
                _unitOfWork.Save();
            }

        }

        public void RemoveExchangeRate(int firstCurrencyId, int secondCurrencyId, Guid authoringSite)
        {
            var currencyDetails = _unitOfWork.CurrencyDetailRepository
                .Find(x => (
                               (x.FromCurrencyId == firstCurrencyId
                                && x.ToCurrencyId == secondCurrencyId)
                               ||
                               (x.ToCurrencyId == firstCurrencyId
                                && x.FromCurrencyId == secondCurrencyId)
                           )
                           && x.AuthoringSiteId == authoringSite
                ).ToList();
            currencyDetails.ForEach(detail =>
            {
                _unitOfWork.CurrencyDetailRepository.Delete(detail);
            });
            _unitOfWork.Save();
        }

        public float? GetCurrentExchangeRate(int fromCurrencyId, int toCurrencyId, Guid authoringSiteId)
        {
            var currencyDetail = _unitOfWork.CurrencyDetailRepository.Find(x => ((x.FromCurrencyId == fromCurrencyId && x.ToCurrencyId == toCurrencyId) || (x.ToCurrencyId == fromCurrencyId && x.FromCurrencyId == toCurrencyId)) && x.AuthoringSiteId == authoringSiteId).FirstOrDefault();
            if (fromCurrencyId == currencyDetail.FromCurrencyId)
            {
                return currencyDetail?.ExchangeRateFrom;
            }
            else
            {
                return currencyDetail?.ExchangeRateTo;
            }
        }

        private CurrencyPaging Pagination(IEnumerable<PAS.Repositories.DataModel.CurrencyDetail> searchCurrencyList, int page, int itemsPerPage)
        {
            var currencyPaging = new CurrencyPaging();
            var totalCount = searchCurrencyList.Count();
            currencyPaging.TotalItems = totalCount;
            currencyPaging.TotalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage);
            if (page <= currencyPaging.TotalPages && page >= 1)
            {
                if (totalCount > itemsPerPage)
                {
                    var claims = searchCurrencyList.OrderByDescending(t => t.Modified).Skip(itemsPerPage * (page - 1))
                        .Take(itemsPerPage)
                        .OrderByDescending(t => t.FromCurrency.Code)
                        .ToList();
                    currencyPaging.CurrencyDetails = _mapper.Map<List<CurrencyDetail>>(claims);
                }
                else
                {
                    var claims = searchCurrencyList
                        .OrderByDescending(t => t.FromCurrency.Code)
                        .ToList();
                    currencyPaging.CurrencyDetails = _mapper.Map<List<CurrencyDetail>>(claims);
                }
                foreach (var item in currencyPaging.CurrencyDetails)
                {
                    var user = _unitOfWork.UserRepository.FindById(item.ModifiedBy);
                    if (user != null)
                    {
                        item.ModifiedUser = new User()
                        {
                            DisplayName = user.Name,
                            //Avatar = user.Avatar
                        };
                    }
                    else
                    {
                        item.ModifiedUser = new User()
                        {

                        };
                    }
                }
            }
            return currencyPaging;
        }

        public CurrencyDetail GetCurrentDetail(int fromCurrencyId, int toCurrencyId, Guid authoringSiteId)
        {
            var currencyDetail = _unitOfWork.CurrencyDetailRepository.Find(x => ((x.FromCurrencyId == fromCurrencyId && x.ToCurrencyId == toCurrencyId) || (x.ToCurrencyId == fromCurrencyId && x.FromCurrencyId == toCurrencyId)) && x.AuthoringSiteId == authoringSiteId).FirstOrDefault();
            return _mapper.Map<CurrencyDetail>(currencyDetail);
        }

        public void RemoveExchangeRate(int currencyDetailId)
        {
            _unitOfWork.CurrencyDetailRepository.Delete(currencyDetailId);
            _unitOfWork.Save();
        }
    }
}
