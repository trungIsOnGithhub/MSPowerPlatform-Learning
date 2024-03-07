using PAS.Model.Domain;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services.Criterion
{
    public class CriterionServices : ICriterionServices
    {
        private readonly ICriteriaGroupRepository _criteriaGroupRepository;
        private readonly IApplicationContext _applicationContext;
        private readonly ICriteriaRepository _criteriaRepository;

        public CriterionServices(ICriteriaGroupRepository criteriaGroupRepository, IApplicationContext applicationContext, ICriteriaRepository criteriaRepository)
        {
            _criteriaGroupRepository = criteriaGroupRepository;
            _applicationContext = applicationContext;
            _criteriaRepository = criteriaRepository;
        }

        public bool AddCriteriaGroup(CriteriaGroup group)
        {
            if (_applicationContext.CurrentUser.Role != Model.Enum.Role.Admin && _applicationContext.CurrentUser.Role != Model.Enum.Role.BOD)
            {
                throw new Exception("Invalid user");
            }
            group.Criterion = group.Criterion.Select(c =>
            {
                c.CreatedBy = _applicationContext.CurrentUser.Id;
                c.CreatedDate = DateTime.Now;
                return c;
            });
            _criteriaGroupRepository.Add(group);
            return _criteriaGroupRepository.UnitOfWork.SaveEntities();
        }

        public bool AddCriteria(Model.Domain.Criteria criteria)
        {
            if (_applicationContext.CurrentUser.Role != Model.Enum.Role.Admin && _applicationContext.CurrentUser.Role != Model.Enum.Role.BOD)
            {
                throw new Exception("Invalid user");
            }
            if (criteria.Name == null)
            {
                throw new Exception("Invalid name criteria");
            }
            if (criteria.GroupId == 0)
            {
                throw new Exception("Invalid group criteria");
            }
            if (criteria.OptionListId == default(int))
                throw new Exception("Invalid criterion option list");
            if (criteria.TypeId == default(int))
                throw new Exception("Invalid type id");

            criteria.CreatedBy = _applicationContext.CurrentUser.Id;
            criteria.CreatedDate = DateTime.Now;

            _criteriaGroupRepository.AddCriteria(criteria);
            return _criteriaGroupRepository.UnitOfWork.SaveEntities();
        }

        public IEnumerable<CriteriaGroup> GetGroups()
        {
            return _criteriaGroupRepository.GetGroupsAndCriteria();
        }

        public IEnumerable<ProjectCriterion> GetProjectCriterion(IEnumerable<int> projectId)
        {
            return _criteriaRepository.GetProjectCriteria(projectId);
        }
        public bool UpdateCriteria(Criteria criteria)
        {
            _criteriaGroupRepository.UpdateCriteria(criteria);
            return _criteriaGroupRepository.UnitOfWork.SaveEntities();
        }

        public bool DeleteCriteria(int criteriaId)
        {
            _criteriaGroupRepository.DeleteCriteria(criteriaId);
            return _criteriaGroupRepository.UnitOfWork.SaveEntities();
        }
    }
}
