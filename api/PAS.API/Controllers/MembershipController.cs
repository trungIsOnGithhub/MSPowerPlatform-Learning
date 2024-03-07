using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PAS.Model.Dto;
using PAS.Model.Mapping;
using PAS.Model.Domain;
using PAS.Services;
namespace PAS.API.Controllers
{
    public struct tempDates
    {
        public DateTime startDate;
        public DateTime endDate;
    }

    [RoutePrefix("api/membership")]
    public class MembershipController : ApiController
    {
        private readonly IDepartmentService _departmentService;
        private readonly IDepartmentDtoMapper _departmentDtoMapper;
        private readonly IUserCoachingService _userCoachingService;
        private readonly IUserCoachingDtoMapper _userCoachingDtoMapper;
        private readonly ITeamDtoMapper _teamDtoMapper;
        private readonly ITeamService _teamService;
        private readonly ITeamMemberService _teamMemberService;
        private readonly ITeamMembersDtoMapper _teamMembersDtoMapper;
        private readonly IUserDtoMapper _userDtoMapper;

        public MembershipController(IUserCoachingService userCoachingService, IUserCoachingDtoMapper userCoachingDtoMapper, ITeamService teamService, ITeamDtoMapper teamDtoMapper, ITeamMembersDtoMapper teamMembersDtoMapper, ITeamMemberService teamMemberService, IDepartmentService departmentService, IDepartmentDtoMapper departmentDtoMapper, IUserDtoMapper userDtoMapper)
        {
            _userCoachingService = userCoachingService;
            _userCoachingDtoMapper = userCoachingDtoMapper;
            _teamService = teamService;
            _teamDtoMapper = teamDtoMapper;
            _teamMembersDtoMapper = teamMembersDtoMapper;
            _teamMemberService = teamMemberService;
            _departmentService = departmentService;
            _departmentDtoMapper = departmentDtoMapper;
            _userDtoMapper = userDtoMapper;
        }

        // UserCoaching

        [HttpGet]
        [Route("user-coaching")]
        public IEnumerable<UserCoachingDto> GetUserCoachings()
        {
            var userCoachings = _userCoachingService.GetUserCoachings()
                .Select(uc => _userCoachingDtoMapper.ToDto(uc));
            return userCoachings;
        }

        [HttpPost]
        [Route("user-coaching/add")]
        public void AssignUserCoach(UserCoachingUploadDto uploadDto)
        {
            _userCoachingService.AssignCoach(_userCoachingDtoMapper.ToDomain(uploadDto));
        }



        // Team
        [HttpPost]
        [Route("team/add")]
        public int AddTeam([FromBody] TeamDto teamDto)
        {
            return _teamService.AddTeam(_teamDtoMapper.ToDomain(teamDto));
        }



        [HttpGet]
        [Route("teams/all-team")]
        public IEnumerable<TeamDto> GetTeams()
        {
            List<Model.Dto.TeamDto> teams = _teamService.GetTeams().Select(x => _teamDtoMapper.ToDto(x)).OrderBy(x => x.Name).ToList();
            return teams;
        }

        [HttpPost]
        [Route("team/update")]
        public int UpdateTeam([FromBody] TeamDto teamDto)
        {
            return _teamService.UpdateTeam(_teamDtoMapper.ToDomain(teamDto));
        }

        // TeamMembers

        [HttpGet]
        [Route("team-members/{id}")]
        public IEnumerable<TeamMembersDto> GetTeamMembers([FromUri(Name = "id")] int userId)
        {
            List<Model.Dto.TeamMembersDto> teamMembers = _teamMemberService.GetTeamMembersById(userId)
                .Select(t => _teamMembersDtoMapper.ToDto(t))
                .ToList();
            return teamMembers;
        }

        [HttpPost]
        [Route("team-members/{id}")]
        public void AddTeamMembers([FromBody] IEnumerable<TeamMembersDto> teamDtos, [FromUri(Name = "id")] int userId)
        {
            
            if (teamDtos is null || !teamDtos.Any() || teamDtos.FirstOrDefault() is null) return;

            List<TeamMembers> teamDomains = new List<TeamMembers>();
            foreach (var teamDto in teamDtos)
            {
                teamDomains.Add(_teamMembersDtoMapper.ToDomain(teamDto, userId));
            }
            _teamMemberService.AddTeams(teamDomains, userId);
        }

        [HttpPost]
        [Route("team-members/primary/{id}")]
        public void AddMainTeamMember([FromBody] TeamMembersDto teamDto, [FromUri(Name = "id")] int userId)
        {
            if (teamDto is null) return;
            _teamMemberService.AddPrimaryTeam(_teamMembersDtoMapper.ToDomain(teamDto, userId), userId);
        }

        [HttpPost]
        [Route("team-members/update/{id}")]
        public bool UpdateTeamMember([FromBody] tempDates tempDates, [FromUri(Name = "id")] int id)
        {
            DateTime startDate = tempDates.startDate;
            DateTime endDate = tempDates.endDate;
            return _teamMemberService.UpdateTeamMember(id, startDate, endDate);
        }

        [HttpGet]
        [Route("team-members/all-ids/{teamId}")]
        public List<int> GetTeamMemberUserIdByTeamId([FromUri] int teamId)
        {
            return _teamMemberService.GetTeamMemberUserIdByTeamId(teamId);
        }

        [HttpPost]
        [Route("team-members/update/current/{id:int}")]
        public void UpdateIsCurrentTeamMember([FromUri] int id, [FromBody] TeamMembersDto teamMembersDto)
        {
            _teamMemberService.SetIsCurrentById(id, _teamMembersDtoMapper.ToDomain(teamMembersDto, teamMembersDto.UserId));
        }

        // Department
        [HttpGet]
        [Route("department/all")]
        public List<DepartmentDto> GetAllDepartments()
        {
            var domainDepartments = _departmentService.GetAllDepartments();
            var result = new List<DepartmentDto>();
            foreach(var department in domainDepartments)
            {
                var dtoDepartment = _departmentDtoMapper.ToDto(department);
                dtoDepartment.Manager = _userDtoMapper.ToDto(department.Manager);
                dtoDepartment.Parent = _departmentDtoMapper.ToDto(department.Parent);
                dtoDepartment.Children = new List<DepartmentDto>();
                foreach (var child in department.Children)
                {
                    dtoDepartment.Children.Add(_departmentDtoMapper.ToDto(child));
                }
                result.Add(dtoDepartment);
            }
            return result;
        }

        [HttpGet]
        [Route("department/active")]
        public List<DepartmentDto> GetActiveDepartments()
        {
            var domainDepartments = _departmentService.GetAllActiveDepartments();
            var result = new List<DepartmentDto>();
            foreach (var department in domainDepartments)
            {
                var dtoDepartment = _departmentDtoMapper.ToDto(department);
                dtoDepartment.Manager = _userDtoMapper.ToDto(department.Manager);
                dtoDepartment.Parent = _departmentDtoMapper.ToDto(department.Parent);
                dtoDepartment.Children = new List<DepartmentDto>();
                foreach (var child in department.Children)
                {
                    dtoDepartment.Children.Add(_departmentDtoMapper.ToDto(child));
                }
                result.Add(dtoDepartment);
            }
            return result;
        }

        [HttpPost]
        [Route("department/add")]
        public int AddDepartment(DepartmentDto departmentDto)
        {
            var domain = _departmentDtoMapper.ToDomain(departmentDto);
            if (departmentDto != null)
            {
                domain.Parent = _departmentDtoMapper.ToDomain(departmentDto.Parent);
                domain.Manager = _userDtoMapper.ToDomain(departmentDto.Manager);
            }
            
            return _departmentService.AddDepartment(domain);
        }

        [HttpPost]
        [Route("department/update")]
        public int UpdateDepartment(DepartmentDto departmentDto)
        {
            var domain = _departmentDtoMapper.ToDomain(departmentDto);
            if (departmentDto != null)
            {
                domain.Parent = _departmentDtoMapper.ToDomain(departmentDto.Parent);
                domain.Manager = _userDtoMapper.ToDomain(departmentDto.Manager);
            }

            return _departmentService.UpdateDepartment(domain);
        }

        [HttpPost]
        [Route("department/toggle")]
        public bool ToggleDepartment(Department department)
        {
            return _departmentService.ToggleDepartment(department);
        }

    }
}
