using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace PAS.Common.Extensions
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        public static string ToPlainText(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                s = Regex.Replace(s, @"<[^>]*>", string.Empty);
                s = HttpUtility.HtmlDecode(s);
            }
            return s;
        }

        public static T Deserialize<T>(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default;
            }
            JsonSerializerSettings setting = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            if (source.IndexOf("&quot;") > -1)
            {
                source = HttpUtility.HtmlDecode(source);
            }
            return JsonConvert.DeserializeObject<T>(source, setting);
        }

        public static string RemoveSquareBrackets(this string str)
        {
            return str.Replace("[", "").Replace("]", "");
        }
    }
}
