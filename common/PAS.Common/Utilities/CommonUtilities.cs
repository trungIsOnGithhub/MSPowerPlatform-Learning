using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PAS.Common.Utilities
{
    public static class CommonUtilities
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);


        public static string ReplaceWithBold(this string content, string token, string replaceBy)
        {
            return content.Replace(token, $"<b>{replaceBy}</b>");
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            builder.Append('_'); //number 1 is fixed for SQL keyword: ALL, FOR, ...
            return builder.ToString();
        }

        public static bool UserEquals(string loginNameUser1, string loginNameUser2)
        {
            string claimsPrefix = "i:0#.f|membership|";
            loginNameUser1 = loginNameUser1.ToLower().Replace(claimsPrefix, "");
            loginNameUser2 = loginNameUser2.ToLower().Replace(claimsPrefix, "");
            return loginNameUser1 == loginNameUser2;
        }

        public static string SerializeJson<T>(T obj)
        {
            if (obj != null)
            {
                return JsonConvert.SerializeObject(obj);
            }
            return null;
        }

        public static List<int> Split(string arr, char separator = ';')
        {
            if(string.IsNullOrEmpty(arr))
            {
                return new List<int>();
            }
            return arr.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(v =>
            {
                int.TryParse(v, out int parsedValue);
                return parsedValue;
            }).ToList();
        }

        public static string Join(List<int> arr, string separator = ";")
        {
            if (arr == null || arr.Count == 0)
            {
                return string.Empty;
            }
            return string.Join(separator, arr);
        }

        public static int? CalcNumOfYears(DateTime? fromDate)
        {
            if (fromDate == null)
            {
                return null;
            }

            TimeSpan span = DateTime.Now - fromDate.Value;
            int years = (int)Math.Round(span.TotalDays / 365.25, MidpointRounding.AwayFromZero);
            return years;
        }
    }
}
