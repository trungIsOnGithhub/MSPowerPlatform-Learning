using AutoMapper;
using PAS.Model.Enum.HRM;
using PAS.Model.HRM;
using PAS.Model.HRM.ClaimModels;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PAS.Services.HRM
{
    public interface IClaimUserRoleService : IBaseService
    {
        List<ClaimRole> GetClaimUserRoles(Guid userId, Guid authouringSiteId);
        ClaimUserRole GetClaimUserRole(Guid userId);
        ClaimUserRole AddClaimUserRole(ClaimUserRolePost claimUserRolePost);
        List<User> GetClaimUserByClaimAuthoringSiteAndRole(Guid claimAuthoringSiteId, ClaimRoles claimRoles);
        void RemoveUserRole(Guid userId);
        void RemoveUserRoles(List<Guid> listUserId);
        void UpdateClaimUserRole(ClaimUserRolePost claimUserRole);
    }

    public class ClaimUserRoleService : BaseService, IClaimUserRoleService
    {
        private IMapper _mapper;
        public ClaimUserRoleService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }
        public ClaimUserRole GetClaimUserRole(Guid userId)
        {
            var claimUserRole = _unitOfWork.ClaimUserRoleRepository.Find(t => t.UserId == userId).FirstOrDefault();
            var result = _mapper.Map<ClaimUserRole>(claimUserRole);
            return result;
        }
        public ClaimUserRole AddClaimUserRole(ClaimUserRolePost claimUserRolePost)
        {
            var newClaimUserRoleEntity = _mapper.Map<ClaimUserRole>(claimUserRolePost);
            var claimUserRoleEntity = _mapper.Map<PAS.Repositories.DataModel.ClaimUserRole>(newClaimUserRoleEntity);
            claimUserRoleEntity.Created = DateTime.UtcNow;
            claimUserRoleEntity.Modified = DateTime.UtcNow;
            claimUserRoleEntity.User = null;
            claimUserRoleEntity.ClaimRole = null;
            claimUserRoleEntity.ClaimAuthoringSite = null;
            _unitOfWork.ClaimUserRoleRepository.Add(claimUserRoleEntity);
            _unitOfWork.Save();

            return newClaimUserRoleEntity;

        }

        public void RemoveUserRole(Guid userId)
        {
            var claimUserRole = _unitOfWork.ClaimUserRoleRepository.Find(t => t.UserId == userId).FirstOrDefault();
            _unitOfWork.ClaimUserRoleRepository.Delete(claimUserRole);
            _unitOfWork.Save();

        }

        public void RemoveUserRoles(List<Guid> listUserId)
        {
            var claimUserRoles = _unitOfWork.ClaimUserRoleRepository.Find(t => listUserId.Contains(t.UserId));
            foreach (var user in claimUserRoles)
            {
                _unitOfWork.ClaimUserRoleRepository.Delete(user);
            }
            _unitOfWork.Save();

        }

        public void UpdateClaimUserRole(ClaimUserRolePost claimUserRolePost)
        {
            var claimUserRole = _unitOfWork.ClaimUserRoleRepository.Find(t => t.UserId == claimUserRolePost.UserId && t.ClaimAuthoringSiteId == claimUserRolePost.ClaimAuthoringSiteId).FirstOrDefault();
            claimUserRole.ClaimRoleId = claimUserRolePost.ClaimRoleId;
            _unitOfWork.Save();
        }

        public List<ClaimRole> GetClaimUserRoles(Guid userId, Guid authouringSiteId)
        {
            var claimUserRole = _unitOfWork.ClaimUserRoleRepository.Find(t => t.UserId == userId && t.ClaimAuthoringSiteId == authouringSiteId);
            var result = claimUserRole.Select(x => new ClaimRole()
            {
                Id = x.ClaimRoleId,
                Name = x.ClaimRole.Name
            }).ToList();
            return result;
        }
     
        public List<User> GetClaimUserByClaimAuthoringSiteAndRole(Guid claimAuthoringSiteId, ClaimRoles claimRoles)
        {
            var role = Enum.GetName(typeof(ClaimRoles), claimRoles);
            return _unitOfWork.ClaimUserRoleRepository
                              .Find(t => t.ClaimAuthoringSiteId == claimAuthoringSiteId
                                    && t.ClaimRole.Name == role,
                                    includeProperties: "User.UserInformation")
                              .AsEnumerable()
                              .Select(t => _mapper.Map<User>(t.User)).ToList();

        }
        #region Private Methods


        #endregion
    }
}