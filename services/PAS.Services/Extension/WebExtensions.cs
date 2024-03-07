using DocumentFormat.OpenXml.InkML;
using Microsoft.SharePoint.Client;
using System;

namespace PAS.Services.Extension
{
    public static class WebExtensions
    {
        public static bool TryGetFileByServerRelativeUrl(this Web web, string serverRelativeUrl, out File file)
        {
            var ctx = web.Context;
            try
            {
                file = web.GetFileByServerRelativeUrl(serverRelativeUrl);
                ctx.Load(file);
                ctx.ExecuteQuery();
                return true;
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    file = null;
                    return false;
                }
                throw;
            }
        }
    }
}
