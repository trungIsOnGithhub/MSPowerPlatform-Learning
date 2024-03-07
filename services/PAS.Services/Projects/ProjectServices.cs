using PAS.Model.Domain;
using PAS.Repositories.Projects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Services.Projects
{
    public class ProjectServices : IProjectServices
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectServices(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public bool AddProject(Model.Domain.Project project)
        {
            if (project.Name == null) return false;
            _projectRepository.Add(project);
            return _projectRepository.UnitOfWork.SaveEntities();
        }
        public Project GetProjectById(int projectId)
        {
            return _projectRepository.GetProjectByIdWithFullCriteria(projectId);
        }
        public Project GetProjectByIdWithUsers(int projectId)
        {
            return _projectRepository.GetProjectByIdWithUsers(projectId);
        }
        public IEnumerable<Project> GetProjectCriterionAtPeriod(int periodId)
        {
            return this._projectRepository.GetProjectCriterionAtPeriod(periodId);
        }
        public IEnumerable<ProjectAndUsers> GetProjectAndUsers()
        {
            return this._projectRepository.GetProjectAndUsers();
        }
        public IEnumerable<Project> GetProjectOfUserInPeriod(int userId, DateTime startDate, DateTime endDate)
        {
            return _projectRepository.GetProjectWithUsers().Aggregate(new List<Project>(), (newList, project) =>
            {
                return project.Users.Aggregate(newList, (newList, projAndUser) =>
                {
                    if (projAndUser.UserId == userId && !newList.Any(p => p.Id == project.Id))
                    {
                        if (projAndUser.StartDate >= startDate && projAndUser.StartDate <= endDate)
                        {
                            project.IsIgnored = projAndUser.IsBeingIgnoredByOthers;
                            newList.Add(project);
                        }
                        else if (projAndUser.StartDate < startDate)
                        {
                            if (projAndUser.EndDate == null)
                            {
                                project.IsIgnored = projAndUser.IsBeingIgnoredByOthers;
                                newList.Add(project);
                            }
                            else if (projAndUser.EndDate >= startDate)
                            {
                                project.IsIgnored = projAndUser.IsBeingIgnoredByOthers;
                                newList.Add(project);
                            }
                        }
                    }
                    return newList;
                });
            });
        }

        public IEnumerable<Project> GetProjectOfUserInPeriodNotIgnore(int userId, DateTime startDate, DateTime endDate)
        {
            return _projectRepository.GetProjectWithUsers().Aggregate(new List<Project>(), (newList, project) =>
            {
                return project.Users.Aggregate(newList, (newList, projAndUser) =>
                {
                    if (projAndUser.UserId == userId && !newList.Any(p => p.Id == project.Id) && !projAndUser.IsBeingIgnoredByOthers)
                    {
                        if (projAndUser.StartDate >= startDate && projAndUser.StartDate <= endDate)
                        {
                            project.IsIgnored = projAndUser.IsBeingIgnoredByOthers;
                            newList.Add(project);
                        }
                        else if (projAndUser.StartDate < startDate)
                        {
                            if (projAndUser.EndDate == null)
                            {
                                project.IsIgnored = projAndUser.IsBeingIgnoredByOthers;
                                newList.Add(project);
                            }
                            else if (projAndUser.EndDate >= startDate)
                            {
                                project.IsIgnored = projAndUser.IsBeingIgnoredByOthers;
                                newList.Add(project);
                            }
                        }
                    }
                    return newList;
                });
            });
        }
















        public IEnumerable<Project> GetProjects(params int[] filteredIds)
        {
            return _projectRepository.GetProjects(filteredIds);
        }

        public Model.Domain.Project GetProjectDetail(int projectId)
        {
            return _projectRepository.GetProjectDetail(projectId);
        }

        public IEnumerable<Project> GetProjectAndCriterion()
        {
            return _projectRepository.GetProjectsWithCriterion();
        }

        public bool AssignUsersToProject(ProjectAndUsers projectAndUsers)
        {

            //if (!_projectRepository.AssignedUser(projectAndUsers.UserId, projectAndUsers.ProjectId))
            //{
                _projectRepository.AssignUsersToProject(projectAndUsers);
                return _projectRepository.UnitOfWork.SaveEntities();
            //}
            //else
            //{
            //    return false;
            //}
        }

        public bool UpdateProject(Model.Domain.Project project)
        {
            _projectRepository.UpdateDetailProject(project);
            return _projectRepository.UnitOfWork.SaveEntities();
        }

        public bool RemoveUserFromProject(int idProjectAndUser)
        {
            //if (!_projectRepository.AssignedUser(userId, projectId))
            //{
            //    return false;
            //}
            _projectRepository.DeleteUsersFromProject(idProjectAndUser);
            return _projectRepository.UnitOfWork.SaveEntities();

        }
        public bool UpdateProjectAndUser(Model.Domain.ProjectAndUsers projectAndUsers)
        {
            _projectRepository.UpdateProjectAndUser(projectAndUsers);
            return _projectRepository.UnitOfWork.SaveEntities();
        }
        public bool DeleteProject(int projectId)
        {
            _projectRepository.DeleteProject(projectId);
            return _projectRepository.UnitOfWork.SaveEntities();
        }

        public bool AddCriteriaToProject(IEnumerable<Model.Domain.Criteria> criterias, int projectId)
        {
            _projectRepository.AddCriteriaToProject(criterias, projectId);
            return _projectRepository.UnitOfWork.SaveEntities();
        }
        public bool DeleteCriteriaFromProject(int projectId, int criteriaId)
        {
            _projectRepository.DeleteCriteriaFromProject(projectId, criteriaId);
            return _projectRepository.UnitOfWork.SaveEntities();
        }

        public bool UpdateCriteriaFromProject(int projectId, Model.Domain.Criteria criteria)
        {
            _projectRepository.UpdateCriteriaFromProject(projectId, criteria);
            return _projectRepository.UnitOfWork.SaveEntities();
        }

        public IEnumerable<Model.Domain.Project> GetRelevantProjects(Model.User currentUser)
        {
            return _projectRepository.GetRelevantProjects(currentUser);
        }
    }
}
