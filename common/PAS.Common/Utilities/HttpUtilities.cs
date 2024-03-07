using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace PAS.Common.Utilities
{
    public class HttpUtilities
    {
        public static void AddExcelContentToHttpResponseMessage(string fileName, HttpResponseMessage result, byte[] tmpArr)
        {
            //todo
            // IE needs url encoding, FF doesn't support it, Google Chrome doesn't care
            //if (HttpContext.Current.Request.Browser.IsBrowser("IE"))
            //{
            //    fileName = HttpUtility.UrlPathEncode(fileName);
            //}

            result.Content = new ByteArrayContent(tmpArr);
            result.Content.Headers.ContentLength = tmpArr.Length;
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = HttpUtility.HtmlEncode(fileName)
            };


            result.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
        }
    }
}
