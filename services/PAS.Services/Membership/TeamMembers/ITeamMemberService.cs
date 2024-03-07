using PAS.Model.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface ITeamMemberService
    {
        public IEnumerable<TeamMembers> GetTeamMembersById(int userId);
        public void AddTeams(IEnumerable<TeamMembers> teamMembers, int userId);
        public void AddPrimaryTeam(TeamMembers teamMembers, int userId);
        public void ClearTeamsById(int userId);
        public void SetIsCurrentById(int teamMemberId, TeamMembers teamMember);
        public void ClearAndAddTeams(IEnumerable<TeamMembers> teamMembers, int userId);
        public bool UpdateTeamMember(int id, DateTime startDate, DateTime endDay);
        List<int> GetTeamMemberUserIdByTeamId(int teamId);
    }
}
