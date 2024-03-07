using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Common
{
    public interface IAuthProvider
    {
        Task<string> GetUserAccessTokenAsync();
    }
}
