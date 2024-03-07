using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IUserCoachingService
    {
        public IEnumerable<Model.Domain.UserCoaching> GetUserCoachings();
        public void AssignCoach(Model.Domain.UserCoaching userCoaching);
    }
}
