using PAS.Model.Mapping;
using PAS.Repositories;
using PAS.Services;
using PAS.Services.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/project")]
    public class ProjectController : BaseController
    {
        private readonly IProjectDtoMapper _projectDtoMapper;
        private readonly IProjectServices _projectServices;
        private readonly ICriteriaDtoMapper _criteriaDtoMapper;

        public ProjectController(IPermissionService permissonService, IProjectDtoMapper projectDtoMapper, IProjectServices projectServices, ICriteriaDtoMapper criteriaDtoMapper): base(permissonService)
        {
            _projectDtoMapper = projectDtoMapper;
            _projectServices = projectServices;
            _criteriaDtoMapper = criteriaDtoMapper;
        }

        [HttpGet]
        public IEnumerable<Model.Dto.ProjectDto> GetProjects()
        {
            CheckAdminPagePermission();
            return _projectDtoMapper.ToDtos(_projectServices.GetProjects());
        }
        
        [HttpGet]
        [Route("{projectId}")]
        public Model.Dto.ProjectDto GetProjectDetail(int projectId)
        {
            CheckAdminPagePermission();
            var projectDto = _projectDtoMapper.ToDto(_projectServices.GetProjectDetail(projectId));
            projectDto.Criterion = projectDto.Criterion.OrderBy(x => x.SortOrder);
            return projectDto;
        }

        [HttpPost]
        [Route("creating")]
        public bool CreateProjects([FromBody] Model.Dto.ProjectDto project)
        {
            CheckAdminPagePermission();
            return _projectServices.AddProject(_projectDtoMapper.ToDomain(project));
        }

        [HttpPut]
        public bool UpdateProject([FromBody] Model.Dto.ProjectDto project)
        {
            CheckAdminPagePermission();
            return _projectServices.UpdateProject(_projectDtoMapper.ToDomain(project));
        }

        [HttpPost]
        [Route("assigning")]
        public bool AssignUsers([FromBody] Model.Dto.ProjectAndUserDto projectAndUserDtos)
        {
            CheckAdminPagePermission();
            return _projectServices.AssignUsersToProject( _projectDtoMapper.ToDomain(projectAndUserDtos));
        }

        [Route("delete/{idProjectAndUser}")]
        [HttpDelete]
        public bool DeleteUserFromProject(int idProjectAndUser)
        {
            CheckAdminPagePermission();
            return _projectServices.RemoveUserFromProject(idProjectAndUser);
        }

        [Route("project-and-user")]
        [HttpPut]
        public bool UpdateProjectAndUser([FromBody] Model.Dto.ProjectAndUserDto projectAndUserDtos)
        {
            CheckAdminPagePermission();
            return _projectServices.UpdateProjectAndUser(_projectDtoMapper.ToDomain(projectAndUserDtos));
        }

        [Route("delete-project/{projectId}")]
        [HttpDelete]
        public bool DeleteProject(int projectId)
        {
            CheckAdminPagePermission();
            return _projectServices.DeleteProject(projectId);
        }

        [Route("assign-criteria/{projectId}")]
        [HttpPost]
        public bool AssignCriteriaToProject(int projectId, [FromBody] IEnumerable<Model.Dto.CriteriaDto> criterias)
        {
            CheckAdminPagePermission();
            return _projectServices.AddCriteriaToProject(_criteriaDtoMapper.ToDomains(criterias), projectId);
        }

        [Route("delete-criteria/{projectId}/{criteriaId}")]
        [HttpDelete]
        public bool DeleteCriteriaFromProject(int projectId, int criteriaId)
        {
            CheckAdminPagePermission();
            return _projectServices.DeleteCriteriaFromProject(projectId, criteriaId);
        }

        [Route("project-and-criteria/{projectId}")]
        [HttpPut]
        public bool UpdateProjectAndCriteria(int projectId, [FromBody] Model.Dto.CriteriaDto criteriaDto)
        {
            CheckAdminPagePermission();
            return _projectServices.UpdateCriteriaFromProject(projectId,_criteriaDtoMapper.ToDomain(criteriaDto));
        }
    }
}
