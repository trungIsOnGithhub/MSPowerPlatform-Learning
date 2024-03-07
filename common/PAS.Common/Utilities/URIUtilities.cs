using System;
using System.Text.RegularExpressions;

namespace PAS.Common.Utilities
{
    public static class URIUtilities
    {
        //public static string BuildSharePointAbsoluteUrl(string sourceUrl)
        //{
        //    string siteUrl = EnsureUrl(Common.Configurations.SharePointConfigurations.SharePointRootUrl);
        //    sourceUrl = EnsureUrl(sourceUrl);
        //    if (!string.IsNullOrEmpty(sourceUrl) && sourceUrl.StartsWith(siteUrl, StringComparison.OrdinalIgnoreCase))
        //    {
        //        return sourceUrl;
        //    }
        //    if (string.IsNullOrEmpty(sourceUrl))
        //    {
        //        return siteUrl;
        //    }

        //    Uri absolueteSiteUrl = new Uri(siteUrl);

        //    sourceUrl = sourceUrl.Replace(absolueteSiteUrl.LocalPath, string.Empty);
        //    return $"{siteUrl}{sourceUrl}";
        //}

        //public static string GetRelativeUrl(string sourceUrl)
        //{
        //    if (sourceUrl.StartsWith("/"))
        //    {
        //        return sourceUrl;
        //    }
        //    return sourceUrl.Replace(EnsureUrl(Common.Configurations.SharePointConfigurations.TenantUrl), string.Empty);
        //}

        public static string EnsureUrl(string url)
        {
            if(string.IsNullOrEmpty(url))
            {
                return url;
            }

            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                if (url.EndsWith("/"))
                {
                    return url.TrimEnd(new char[] { '/' });
                }
            }
            else
            {
                if (!url.StartsWith("/"))
                {
                    return $"/{url}";
                }
            }
            return url;
        }
    }
}
