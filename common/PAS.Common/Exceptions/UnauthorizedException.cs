using System;

namespace PAS.Common
{
    public class UnauthorizedException: Exception
    {
        public UnauthorizedException(string message): base(message) { }
    }
}
