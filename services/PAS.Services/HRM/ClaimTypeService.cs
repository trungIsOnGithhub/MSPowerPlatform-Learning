using AutoMapper;
using PAS.Model.HRM.ClaimModels;
using PAS.Repositories;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services.HRM
{

    public interface IClaimTypeService : IBaseService
    {
        List<ClaimType> GetClaimTypes(Guid userId);

        bool IsClaimTypeExisted(ClaimTypePost claimType);
        void AddClaimType(ClaimTypePost claimType);
        void EditClaimType(ClaimTypePost claimType);
        void RemoveClaimTypeById(int claimTypeID);
    }
    public class ClaimTypeService : BaseService, IClaimTypeService
    {
        private IMapper _mapper;
        public ClaimTypeService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public List<ClaimType> GetClaimTypes(Guid userId)
        {
            var authoringSite = _unitOfWork.ClaimUserRoleRepository.Find(w => w.UserId == userId)
                        .Select(w => w.ClaimAuthoringSiteId).FirstOrDefault();
            var select = _unitOfWork.ClaimTypeRepository.Find(x => (x.ClaimAuthoringSiteId == authoringSite) && x.IsActive, null, "ClaimTypeLocalizations")
                .Select(x => _mapper.Map<ClaimType>(x));
            List<ClaimType> claimTypes = select.ToList();
            return claimTypes;
        }

        public bool IsClaimTypeExisted(ClaimTypePost claimType)
        {
            var result = false;
            var count = _unitOfWork.ClaimTypeRepository.Find(x => x.Code == claimType.Code && x.ClaimAuthoringSiteId == claimType.ClaimAuthoringSiteId).Count();

            if (count > 0)
            {
                result = true;
            }
            return result;
        }

        public PAS.Repositories.DataModel.ClaimType FindClaimTypeByCodeAndNotActive(ClaimTypePost claimType)
        {
            PAS.Repositories.DataModel.ClaimType result = null;
            result = _unitOfWork.ClaimTypeRepository
                .Find(x => x.Code == claimType.Code && x.ClaimAuthoringSiteId == claimType.ClaimAuthoringSiteId && !x.IsActive)
                .FirstOrDefault();

            return result;
        }


        public void AddClaimType(ClaimTypePost claimType)
        {
            PAS.Repositories.DataModel.ClaimType claimTypeEntity = FindClaimTypeByCodeAndNotActive(claimType);

            bool isEntityExisted = true;

            if (claimTypeEntity == null)
            {
                isEntityExisted = false;
                var claimTypeDto = _mapper.Map<ClaimType>(claimType);
                claimTypeEntity = _mapper.Map<PAS.Repositories.DataModel.ClaimType>(claimTypeDto);
            }

            if (claimTypeEntity.ClaimTypeLocalizations == null || claimTypeEntity.ClaimTypeLocalizations.Count == 0)
            {
                claimTypeEntity.ClaimTypeLocalizations.Add(new PAS.Repositories.DataModel.ClaimTypeLocalization()
                {
                    Name = claimType.Name,
                    LanguageCode = "en-US"
                });
            }

            claimTypeEntity.Created = DateTime.Now;
            claimTypeEntity.CreatedBy = Guid.Empty;

            claimTypeEntity.Modified = DateTime.Now;
            claimTypeEntity.ModifiedBy = Guid.Empty;

            claimTypeEntity.IsActive = true;

            if (!isEntityExisted)
            {
                _unitOfWork.ClaimTypeRepository.Add(claimTypeEntity);
            }
            _unitOfWork.Save();
        }

        public void EditClaimType(ClaimTypePost claimType)
        {
            var claimTypeEntity = _unitOfWork.ClaimTypeRepository.Find(x => x.Id == claimType.Id, null, "ClaimTypeLocalizations").FirstOrDefault();

            if (claimTypeEntity != null)
            {
                claimTypeEntity.Icon = claimType.Icon;
                claimTypeEntity.Code = claimType.Code;

                if (claimType.ClaimTypeLocalizations != null && claimType.ClaimTypeLocalizations.Count() > 0)
                {
                    claimType.ClaimTypeLocalizations.ForEach(x =>
                    {
                        var claimTypeLocalizationEntity = claimTypeEntity.ClaimTypeLocalizations
                            .Where(y => y.ClaimTypeId == claimType.Id && y.LanguageCode == x.LanguageCode)
                            .FirstOrDefault();
                        if (claimTypeLocalizationEntity != null)
                        {
                            claimTypeLocalizationEntity.Name = x.Name;
                            claimTypeLocalizationEntity.Description = x.Description;
                        }
                    });
                }
                else
                {
                    var claimTypeLocalizationEntity = claimTypeEntity.ClaimTypeLocalizations
                        .Where(y => y.ClaimTypeId == claimType.Id && y.LanguageCode.ToLower() == "en-US")
                        .FirstOrDefault();
                    if (claimTypeLocalizationEntity != null)
                    {
                        claimTypeLocalizationEntity.Name = claimType.Name;
                    }
                    else
                    {
                        claimTypeEntity.ClaimTypeLocalizations.Add(
                            new PAS.Repositories.DataModel.ClaimTypeLocalization()
                            {
                                Name = claimType.Name,
                                LanguageCode = "en-US"
                            });
                    }
                }

                claimTypeEntity.Modified = DateTime.Now;
                claimTypeEntity.ModifiedBy = Guid.Empty;

                _unitOfWork.Save();
            }
        }

        public void RemoveClaimTypeById(int claimTypeID)
        {
            var claimTypeEntity = _unitOfWork.ClaimTypeRepository.FindById(claimTypeID);
            claimTypeEntity.IsActive = false;

            _unitOfWork.ClaimTypeLocalizationRepository
                .Find(x => x.ClaimTypeId == claimTypeID).ToList()
                .ForEach(x => _unitOfWork.ClaimTypeLocalizationRepository.Delete(x));

            _unitOfWork.Save();

        }
    }
}
