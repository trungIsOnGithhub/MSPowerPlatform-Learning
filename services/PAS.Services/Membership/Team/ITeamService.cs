using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface ITeamService
    {
        int AddTeam(Model.Domain.Team team);
        public IEnumerable<Model.Domain.Team> GetTeams();
        int UpdateTeam(Model.Domain.Team domainObject);
    }
}
