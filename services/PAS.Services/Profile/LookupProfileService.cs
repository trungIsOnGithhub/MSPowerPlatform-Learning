using PAS.Model.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Repositories;

namespace PAS.Services
{
    public class LookupProfileService : ILookupProfileService
    {
        private readonly ILookupProfileRepository lookupProfileRepository;

        public LookupProfileService(ILookupProfileRepository lookupProfileRepository)
        {
            this.lookupProfileRepository = lookupProfileRepository;
        }

        public List<GenericItem> GetActiveLanguages()
        {
            return this.lookupProfileRepository.GetActiveLanguages();
        }
    }
}
