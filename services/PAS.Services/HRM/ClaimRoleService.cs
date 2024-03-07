using AutoMapper;
using PAS.Model.Enum.HRM;
using PAS.Model.HRM.ClaimModels;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PAS.Services.HRM
{
    public interface IClaimRoleService : IBaseService
    {
        List<ClaimRole> GetAllRole();
        ClaimRole GetRoleByName(ClaimRoles role);
        IList<ClaimRole> GetRolesByName(string roles);
        IList<ClaimRole> GetUserRoles(Guid userId, Guid authouringSite);
    }

    public class ClaimRoleService : BaseService, IClaimRoleService
    {

        private IMapper _mapper;
        public ClaimRoleService(IHRMUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public List<ClaimRole> GetAllRole()
        {
            var result = _unitOfWork.ClaimRoleRepository.GetAll();
            var items = _mapper.Map<List<ClaimRole>>(result);
            return items;
        }

        public ClaimRole GetRoleByName(ClaimRoles role)
        {
            var result = _unitOfWork.ClaimRoleRepository.Find(t => t.Name == role.ToString()).FirstOrDefault();
            var item = _mapper.Map<ClaimRole>(result);
            return item;
        }

        public IList<ClaimRole> GetRolesByName(string roles)
        {
            var roleList = roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var results = _unitOfWork.ClaimRoleRepository.Find(t => roleList.Contains(t.Name)).ToList();
            var item = results.Select(x => _mapper.Map<ClaimRole>(x)).ToList();
            return item;
        }

        public IList<ClaimRole> GetUserRoles(Guid userId, Guid authouringSite)
        {
            var userRoles = _unitOfWork.ClaimUserRoleRepository.Find(t => t.UserId.Equals(userId) && t.ClaimAuthoringSiteId.Equals(authouringSite)).Include(x => x.User).Include(x => x.ClaimRole).ToList();
            /// generate hash to valid roles

            var result = userRoles.Select(x => new ClaimRole()
            {
                Id = x.ClaimRole.Id,
                Name = x.ClaimRole.Name,


            }).ToList();

            return result;
        }
    }
}