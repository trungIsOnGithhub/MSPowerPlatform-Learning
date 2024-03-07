using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface ILookupProfileService
    {
        List<Model.Domain.GenericItem> GetActiveLanguages();
    }
}
