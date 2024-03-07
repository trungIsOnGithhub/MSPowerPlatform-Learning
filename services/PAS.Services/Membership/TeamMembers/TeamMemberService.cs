using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class TeamMemberService : ITeamMemberService
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IUserRepository _userRepository;

        public TeamMemberService(IUserRepository userRepository, ITeamMemberRepository teamMemberRepository)
        {
            _userRepository = userRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        public void AddTeams(IEnumerable<Model.Domain.TeamMembers> teamMembers, int userId)
        {
            var mainTeam = teamMembers
                .Where(m => m.IsMain == true)
                .FirstOrDefault();
            //this._teamMemberRepository.setEndDateForOldPrimaryTeamMemberById(userId);
            this._teamMemberRepository.AddTeamMembers(teamMembers);
            this._teamMemberRepository.UnitOfWork.SaveEntities();
            if (mainTeam != null)
            {
                this._userRepository.UpdateTeamId(userId, mainTeam.TeamId);
                this._userRepository.UnitOfWork.SaveEntities();
            }
        }

        public void AddPrimaryTeam(Model.Domain.TeamMembers teamMembers, int userId)
        {
            this._teamMemberRepository.setEndDateForOldPrimaryTeamMember(userId, teamMembers);
            this._teamMemberRepository.AddTeamMember(teamMembers);
            this._teamMemberRepository.UnitOfWork.SaveEntities();
            if (teamMembers != null)
            {
                this._userRepository.UpdateTeamId(userId, (int)teamMembers.TeamId);
                this._userRepository.UnitOfWork.SaveEntities();
            }
        }

        public void ClearTeamsById(int userId)
        {
            bool result = this._teamMemberRepository.ClearTeamsById(userId);
            if (result) this._teamMemberRepository.UnitOfWork.SaveEntities();
        }

        public void ClearAndAddTeams(IEnumerable<Model.Domain.TeamMembers> teamMembers, int userId)
        {
            this.ClearTeamsById(userId);
            this.AddTeams(teamMembers, userId);
        }

        public IEnumerable<Model.Domain.TeamMembers> GetTeamMembersById(int userId)
        {
            return this._teamMemberRepository.GetAllTeamMembersById(userId);
        }

        public void SetIsCurrentById(int teamMemberId, Model.Domain.TeamMembers teamMember)
        {
            _teamMemberRepository.SetIsCurrentById(teamMemberId, teamMember);
        }

        public bool UpdateTeamMember(int id, DateTime startDate, DateTime endDate)
        {
            this._teamMemberRepository.UpdateTeamMember(id, startDate, endDate);
            this._teamMemberRepository.UnitOfWork.SaveEntities();
            return true;
        }
        public List<int> GetTeamMemberUserIdByTeamId(int teamId)
        {
            return _teamMemberRepository.GetAllTeamMemberUserIdByTeamId(teamId);
        }
    }
}
