using AutoMapper;
using PAS.Model.Enum.HRM;
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
    public interface ILeaveTypeService : IBaseService
    {
        List<LeaveType> GetAllLeaveType();

        List<LeaveType> GetAllLeaveTypeByAuthoringSiteId(Guid authoringSiteId);
        List<LeaveType> GetAllLeaveTypeIncludeParamsByAuthoringSiteId(Guid authoringSiteId);
        Task AddLeaveType(LeaveTypePost leaveType);
        Task AddLeaveType(LeaveType leaveType);
        void UpdateLeaveType(LeaveTypePost leaveType);
        LeaveType GetLeaveTypeById(int leaveTypeId);
        List<LeaveTypeParam> GetLeaveTypeParamByLeaveType(int leaveTypeId);
        void UpdateLeaveTypeParam(LeaveTypeParam leaveTypeParam);
        LeaveTypeParam GetLeaveTypeParamById(int leaveTypeParamId);
        Task AddLeaveTypeParam(LeaveTypeParam leaveTypeParam);
        List<LeaveTypeSummary> GetDefaultLeaveTypeByAuthoringSiteId(Guid authoringSiteId);
        bool RemoveLeavePolicies(LeaveTypeParam leavePolicies);
        bool RemoveLeaveType(int leaveTypeId);
    }

    public class LeaveTypeService : BaseService, ILeaveTypeService
    {
        private IMapper _mapper;
        public LeaveTypeService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public List<LeaveType> GetAllLeaveType()
        {
            var leaveTypeList = _unitOfWork.LeaveTypeRepository
                .Find(t => true, includeProperties: "LeaveTypeLocalizations")
                .ToList();

            var items = _mapper.Map<List<LeaveType>>(leaveTypeList);
            // order ascending by id
            // items = items.ToList();
            items = LocalizeValueForList(items);

            return items;
        }

        public List<LeaveType> GetAllLeaveTypeByAuthoringSiteId(Guid authoringSiteId)
        {
            var leaveTypeList = _unitOfWork.LeaveTypeRepository
                .Find(t => t.AuthoringSiteId == authoringSiteId && t.IsActive == true,
                    includeProperties: "LeaveTypeLocalizations")
                .ToList();
            var items = _mapper.Map<List<LeaveType>>(leaveTypeList);
            // order ascending by id
            items = items.OrderBy(t => t.Id).ToList();
            items = LocalizeValueForList(items);

            return items;
        }

        public List<LeaveType> GetAllLeaveTypeIncludeParamsByAuthoringSiteId(Guid authoringSiteId)
        {
            var result = _unitOfWork.LeaveTypeRepository
                .Find(t => t.AuthoringSiteId == authoringSiteId && t.IsActive == true, includeProperties: "LeaveTypeParams")?.ToList();
            var items = _mapper.Map<List<LeaveType>>(result);

            return items;
        }

        public async Task AddLeaveType(LeaveTypePost leaveType)
        {

            var newLeaveType = _mapper.Map<LeaveType>(leaveType);
            newLeaveType.LeaveTypeLocalizations.Add(new LeaveTypeLocalization
            {
                LanguageCode = Constants.LanguageCode.English,
                Name = leaveType.Name,
                Description = leaveType.Description
            });
            var leaveTypeEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveType>(newLeaveType);
            leaveTypeEntity.IsActive = true;
            leaveTypeEntity.Created = DateTime.UtcNow;
            leaveTypeEntity.Modified = DateTime.UtcNow;
            _unitOfWork.LeaveTypeRepository.Add(leaveTypeEntity);
            await _unitOfWork.SaveAsync();

        }
        public async Task AddLeaveType(LeaveType leaveType)
        {

            var addedLeaveType = new LeaveType();
            var existLeaveType = _unitOfWork.LeaveTypeRepository.Find(t => t.Code == leaveType.Code && t.AuthoringSiteId == leaveType.AuthoringSiteId).Any();
            if (!existLeaveType)
            {
                var leaveTypeEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveType>(leaveType);
                leaveTypeEntity.IsActive = true;
                leaveTypeEntity.Created = DateTime.UtcNow;
                leaveTypeEntity.Modified = DateTime.UtcNow;
                _unitOfWork.LeaveTypeRepository.Add(leaveTypeEntity);
                await _unitOfWork.SaveAsync();
            }
        }
        public void UpdateLeaveType(LeaveTypePost leaveType)
        {
            var foundLeaveType = _unitOfWork.LeaveTypeRepository.Find(t => t.Id == leaveType.Id).FirstOrDefault();
            var foundLeaveTypeLocalization = _unitOfWork.LeaveTypeLocalizationRepository.Find(t => t.LeaveTypeId == foundLeaveType.Id && t.LanguageCode == Constants.LanguageCode.English).FirstOrDefault();
            if (foundLeaveTypeLocalization != null)
            {
                foundLeaveTypeLocalization.Name = leaveType.Name;
                foundLeaveTypeLocalization.Description = leaveType.Description;
            }
            _unitOfWork.Save();

            if (foundLeaveType != null)
            {
                foundLeaveType.BaseNumberOfDay = leaveType.BaseNumberOfDay;
                foundLeaveType.Code = leaveType.Code;
                foundLeaveType.MaximumNumberOfDay = leaveType.MaximumNumberOfDay;
                foundLeaveType.MaximumNumberOfReservedDay = leaveType.MaximumNumberOfReservedDay;
                foundLeaveType.IsApprovalRequired = leaveType.IsApprovalRequired;
                _unitOfWork.Save();
            }
        }

        public LeaveType GetLeaveTypeById(int leaveTypeId)
        {
            var foundLeaveType = _unitOfWork.LeaveTypeRepository.Find(t => t.Id == leaveTypeId && t.IsActive == true, includeProperties: "LeaveTypeLocalizations").FirstOrDefault();
            var leaveType = _mapper.Map<LeaveType>(foundLeaveType);
            leaveType.Name = leaveType.LeaveTypeLocalizations[0].Name;
            leaveType.Description = leaveType.LeaveTypeLocalizations[0].Description;
            return leaveType;
        }
        public List<LeaveTypeParam> GetLeaveTypeParamByLeaveType(int leaveTypeId)
        {
            var foundLeaveTypeParam = _unitOfWork.LeaveTypeParamRepository
                .Find(t => t.LeaveTypeId == leaveTypeId, includeProperties: "LeaveTypeParamDetails")
                .ToList();
            var leaveTypeParam = _mapper.Map<List<LeaveTypeParam>>(foundLeaveTypeParam);
            return leaveTypeParam;


        }
        public LeaveTypeParam GetLeaveTypeParamById(int leaveTypeParamId)
        {
            var foundLeaveTypeParam = _unitOfWork.LeaveTypeParamRepository.Find(t => t.Id == leaveTypeParamId).Include(h => h.LeaveTypeParamDetails).FirstOrDefault();
            var leaveTypeParam = _mapper.Map<LeaveTypeParam>(foundLeaveTypeParam);
            return leaveTypeParam;
        }

        public void UpdateLeaveTypeParam(LeaveTypeParam leaveTypeParam)
        {
            var foundLeaveTypeParam = _unitOfWork.LeaveTypeParamRepository
                .Find(t => t.Id == leaveTypeParam.Id, includeProperties: "LeaveTypeParamDetails")
                .FirstOrDefault();
            // 
            if (foundLeaveTypeParam != null)
            {
                foundLeaveTypeParam.Name = leaveTypeParam.Name;
                foundLeaveTypeParam.NumberOfDay = leaveTypeParam.NumberOfDay;
                foundLeaveTypeParam.Modified = DateTime.UtcNow;
                // LeaveTypeParamDetails
                var oldLeaveTypeParamDetails = foundLeaveTypeParam.LeaveTypeParamDetails.ToList();
                foreach (var oldItem in oldLeaveTypeParamDetails)
                {
                    var count = leaveTypeParam.LeaveTypeParamDetails.Count(t => t.Id == oldItem.Id);
                    if (count == 0)
                    {
                        _unitOfWork.LeaveTypeParamDetailRepository.Delete(oldItem);
                    }
                    else
                    {
                        var updateItem = leaveTypeParam.LeaveTypeParamDetails.FirstOrDefault(t => oldItem.Id == t.Id);
                        if (updateItem == null) continue;
                        oldItem.Params = updateItem.Params;
                        oldItem.Type = updateItem.Type;
                    }
                }
                foreach (var item in leaveTypeParam.LeaveTypeParamDetails)
                {
                    var count = oldLeaveTypeParamDetails.Count(t => t.Id == item.Id);
                    if (count == 0)
                    {
                        var newItem = _mapper.Map<PAS.Repositories.DataModel.LeaveTypeParamDetail>(item);
                        foundLeaveTypeParam.LeaveTypeParamDetails.Add(newItem);
                    }
                }
            }
            _unitOfWork.Save();

        }

        public async Task AddLeaveTypeParam(LeaveTypeParam leaveTypeParam)
        {
            var leaveTypeParamEntity = _mapper.Map<PAS.Repositories.DataModel.LeaveTypeParam>(leaveTypeParam);
            leaveTypeParamEntity.Created = DateTime.UtcNow;
            leaveTypeParamEntity.Modified = DateTime.UtcNow;

            _unitOfWork.LeaveTypeParamRepository.Add(leaveTypeParamEntity);
            await _unitOfWork.SaveAsync();
        }

        public List<LeaveTypeSummary> GetDefaultLeaveTypeByAuthoringSiteId(Guid authoringSiteId)
        {
            var leaveTypeList = _unitOfWork.LeaveTypeRepository
                .Find(t => t.AuthoringSiteId == authoringSiteId
                           && t.IsActive == true
                           && t.IsApprovalRequired,
                    includeProperties: "LeaveTypeLocalizations")
                ?.ToList();
            // mapping
            var items = _mapper.Map<List<LeaveTypeSummary>>(leaveTypeList)
              .Select(t =>
              {
                  t.Name = t.LeaveTypeLocalizations[0].Name; // TODO: default language code: en-US
                  t.Description = t.LeaveTypeLocalizations[0].Description;
                  return t;
              })?.ToList();
            return items;
        }
        public bool RemoveLeavePolicies(LeaveTypeParam leavePolicies)
        {
            var foundLeaveParam = _unitOfWork.LeaveTypeParamRepository.Find(t => t.Id == leavePolicies.Id).FirstOrDefault();
            _unitOfWork.LeaveTypeParamRepository.Delete(foundLeaveParam);
            _unitOfWork.Save();
            return true;

        }
        public bool RemoveLeaveType(int leaveTypeId)
        {
            var foundLeaveType = _unitOfWork.LeaveTypeRepository.Find(t => t.Id == leaveTypeId).FirstOrDefault();
            foundLeaveType.IsActive = false;
            _unitOfWork.Save();
            return true;

        }
        #region Private Methods

        private List<LeaveType> LocalizeValueForList(List<LeaveType> items)
        {
            var result = items.Select(t =>
            {
                t.Name = t.LeaveTypeLocalizations[0].Name; // TODO: default language code: en-US
                t.Description = t.LeaveTypeLocalizations[0].Description;
                return t;
            }).ToList();

            return result;
        }

        #endregion
    }
}