using System;

namespace PAS.Common
{
    public class BadRequestException: Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}
