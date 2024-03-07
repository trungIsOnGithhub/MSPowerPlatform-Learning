using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Model.Domain;
using PAS.Repositories;
namespace PAS.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;

        public TeamService(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public int AddTeam(Model.Domain.Team team)
        {
            int count = _teamRepository.AddTeam(team);
            if (count > 0) _teamRepository.UnitOfWork.SaveEntities();
            return count;
        }

        public IEnumerable<Model.Domain.Team> GetTeams()
        {
            return this._teamRepository.GetAllTeams();
        }

        public int UpdateTeam(Team domainObject)
        {
            int count = _teamRepository.UpdateTeam(domainObject);
            if (count > 0) _teamRepository.UnitOfWork.SaveEntities();
            return count;
        }
    }
}
