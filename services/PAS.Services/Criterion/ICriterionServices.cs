using System.Collections;
using System.Collections.Generic;

namespace PAS.Services.Criterion
{
    public interface ICriterionServices
    {
        bool AddCriteriaGroup(Model.Domain.CriteriaGroup group);
        bool AddCriteria(Model.Domain.Criteria criteria);
        bool UpdateCriteria(Model.Domain.Criteria criteria);
        bool DeleteCriteria(int criteriaId);
        IEnumerable<Model.Domain.CriteriaGroup> GetGroups();
        IEnumerable<Model.Domain.ProjectCriterion> GetProjectCriterion(IEnumerable<int> projectId);
    }
}