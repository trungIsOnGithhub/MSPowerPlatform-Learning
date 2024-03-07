using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Repositories;

namespace PAS.Services
{
    public class UserCoachingService : IUserCoachingService
    {
        private readonly IUserCoachingRepository _userCoachingRepository;

        public UserCoachingService(IUserCoachingRepository userCoachingRepository)
        {
            _userCoachingRepository = userCoachingRepository;
        }

        public IEnumerable<Model.Domain.UserCoaching> GetUserCoachings()
        {
            return _userCoachingRepository.GetUserCoachings();
        }

        public void AssignCoach(Model.Domain.UserCoaching userCoaching)
        {
            _userCoachingRepository.SetEndDateForOldCoach(userCoaching);
            _userCoachingRepository.AssignCoach(userCoaching);
            _userCoachingRepository.UnitOfWork.SaveEntities();
        }

    }
}
