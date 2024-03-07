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

    public interface IClaimInvoiceCategoryService : IBaseService
    {
        List<ClaimInvoiceCategory> GetInvoiceCategories(Guid userId);

        bool IsClaimInvoiceCategoryExisted(ClaimInvoiceCategoryPost claimInvoiceCategory);
        void AddClaimInvoiceCategory(ClaimInvoiceCategoryPost claimInvoiceCategory);
        void EditClaimInvoiceCategory(ClaimInvoiceCategoryPost claimInvoiceCategory);
        void RemoveClaimInvoiceCategoryById(int claimInvoiceCategoryID);
    }

    public class ClaimInvoiceCategoryService : BaseService, IClaimInvoiceCategoryService
    {
        private IMapper _mapper;

        public ClaimInvoiceCategoryService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }


        public List<ClaimInvoiceCategory> GetInvoiceCategories(Guid userId)
        {
            var authoringSite = _unitOfWork.ClaimUserRoleRepository.Find(w => w.UserId == userId)
                                           .Select(w => w.ClaimAuthoringSiteId).FirstOrDefault();
            return _unitOfWork.ClaimInvoiceCategoryRepository.Find(x => (x.ClaimAuthoringSiteId == authoringSite) && x.IsActive, includeProperties: "ClaimInvoiceCategoryLocalizations").AsEnumerable()
                              .Select(x => _mapper.Map<ClaimInvoiceCategory>(x)).ToList();
        }

        public bool IsClaimInvoiceCategoryExisted(ClaimInvoiceCategoryPost claimInvoiceCategory)
        {
            return false;
        }

        public void AddClaimInvoiceCategory(ClaimInvoiceCategoryPost claimInvoiceCategory)
        {
            var claimInvoiceCategoryDto = _mapper.Map<ClaimInvoiceCategory>(claimInvoiceCategory);

            var claimInvoiceCategoryEntity = _mapper.Map<PAS.Repositories.DataModel.ClaimInvoiceCategory>(claimInvoiceCategoryDto);

            claimInvoiceCategoryEntity.IsActive = true;

            if (claimInvoiceCategoryEntity.ClaimInvoiceCategoryLocalizations == null ||
                claimInvoiceCategoryEntity.ClaimInvoiceCategoryLocalizations.Count == 0)
            {
                claimInvoiceCategoryEntity.ClaimInvoiceCategoryLocalizations.Add(new PAS.Repositories.DataModel.ClaimInvoiceCategoryLocalization()
                {
                    Name = claimInvoiceCategory.Name,
                    LanguageCode = "en-US"
                });
            }

            _unitOfWork.ClaimInvoiceCategoryRepository.Add(claimInvoiceCategoryEntity);
            _unitOfWork.Save();
        }

        public void EditClaimInvoiceCategory(ClaimInvoiceCategoryPost claimInvoiceCategory)
        {
            var claimInvoiceCategoryEntity = _unitOfWork.ClaimInvoiceCategoryRepository.Find(x => x.Id == claimInvoiceCategory.Id, null, "ClaimInvoiceCategoryLocalizations").FirstOrDefault();

            if (claimInvoiceCategoryEntity != null)
            {
                if (claimInvoiceCategory.ClaimInvoiceCategoryLocalizations != null &&
                    claimInvoiceCategory.ClaimInvoiceCategoryLocalizations.Count > 0)
                {
                    claimInvoiceCategory.ClaimInvoiceCategoryLocalizations.ForEach(x =>
                    {
                        var claimInvoiceCategoryLocalizationEntity = claimInvoiceCategoryEntity
                            .ClaimInvoiceCategoryLocalizations.Where(y =>
                                y.ClaimInvoiceCategoryId == claimInvoiceCategory.Id && y.LanguageCode == x.LanguageCode)
                            .FirstOrDefault();
                        if (claimInvoiceCategoryLocalizationEntity != null)
                        {
                            claimInvoiceCategoryLocalizationEntity.Name = x.Name;
                            claimInvoiceCategoryLocalizationEntity.Desciption = x.Desciption;
                        }
                    });
                }
                else
                {
                    var claimInvoiceCategoryLocalizationEntity = claimInvoiceCategoryEntity.ClaimInvoiceCategoryLocalizations
                        .Where(y => y.ClaimInvoiceCategoryId == claimInvoiceCategory.Id && y.LanguageCode.ToLower() == "en-US")
                        .FirstOrDefault();
                    if (claimInvoiceCategoryLocalizationEntity != null)
                    {
                        claimInvoiceCategoryLocalizationEntity.Name = claimInvoiceCategory.Name;
                    }
                    else
                    {
                        claimInvoiceCategoryEntity.ClaimInvoiceCategoryLocalizations.Add(
                            new PAS.Repositories.DataModel.ClaimInvoiceCategoryLocalization()
                            {
                                Name = claimInvoiceCategory.Name,
                                LanguageCode = "en-US"
                            });
                    }
                }

                _unitOfWork.Save();
            }
        }

        public void RemoveClaimInvoiceCategoryById(int claimInvoiceCategoryID)
        {
            var claimInvoiceCategoryEntity = _unitOfWork.ClaimInvoiceCategoryRepository.FindById(claimInvoiceCategoryID);
            claimInvoiceCategoryEntity.IsActive = false;

            _unitOfWork.ClaimInvoiceCategoryLocalizationRepository
                .Find(x => x.ClaimInvoiceCategoryId == claimInvoiceCategoryID).ToList()
                .ForEach(x => _unitOfWork.ClaimInvoiceCategoryLocalizationRepository.Delete(x));

            _unitOfWork.Save();
        }

    }
}
