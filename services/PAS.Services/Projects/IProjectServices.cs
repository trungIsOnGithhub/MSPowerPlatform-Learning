using PAS.Model.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services.Projects
{
    public interface IProjectServices
    {
        public bool AddProject(Model.Domain.Project project);
        bool AssignUsersToProject(Model.Domain.ProjectAndUsers projectAndUsers);
        public bool UpdateProject(Model.Domain.Project project);
        public bool UpdateProjectAndUser(Model.Domain.ProjectAndUsers projectAndUsers);
        public Model.Domain.Project GetProjectDetail(int projectId);
        public bool RemoveUserFromProject(int idProjectAndUser);
        public bool DeleteProject(int projectId);
        public bool AddCriteriaToProject(IEnumerable<Model.Domain.Criteria> criterias, int projectId);
        public bool DeleteCriteriaFromProject(int projectId, int criteriaId);
        public bool UpdateCriteriaFromProject(int projectId, Model.Domain.Criteria criteria);
        public IEnumerable<Model.Domain.Project> GetProjects(params int[] filteredIds);
        public IEnumerable<Model.Domain.Project> GetProjectOfUserInPeriod(int userId,DateTime startDate, DateTime endDate);
        public IEnumerable<Model.Domain.Project> GetProjectOfUserInPeriodNotIgnore(int userId, DateTime startDate, DateTime endDate);
        public Model.Domain.Project GetProjectById(int projectId);
        public Model.Domain.Project GetProjectByIdWithUsers(int projectId);
        public IEnumerable<Model.Domain.Project> GetProjectCriterionAtPeriod(int periodId);
        public IEnumerable<Model.Domain.ProjectAndUsers> GetProjectAndUsers();
        public IEnumerable<Model.Domain.Project> GetProjectAndCriterion();
        public IEnumerable<Model.Domain.Project> GetRelevantProjects(Model.User currentUser);
    }
}
