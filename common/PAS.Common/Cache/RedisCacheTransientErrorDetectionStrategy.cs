using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using StackExchange.Redis;

namespace PAS.Common
{
    class RedisCacheTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            if (ex == null) return false;

            if (ex is TimeoutException) return true;

            if (ex is RedisServerException) return true;

            if (ex is RedisException) return true;

            if (ex.InnerException != null)
            {
                return IsTransient(ex.InnerException);
            }

            return false;
        }
    }
}
