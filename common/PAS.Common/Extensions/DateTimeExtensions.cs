using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDateWithTimeFieldValue(this DateTime instance, DateTimeKind defaultKindIfUnspecified = DateTimeKind.Local)
        {
            if (instance.Kind == DateTimeKind.Unspecified)
            {
                return instance != DateTime.MinValue
                ? DateTime.SpecifyKind(instance, defaultKindIfUnspecified)
                : DateTime.MinValue;
            }
            else
            {
                return instance;
            }
        }
    }
}
