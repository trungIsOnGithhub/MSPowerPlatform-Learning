using PAS.Common;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace PAS.API.ErrorHandler
{
    public class PASExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {         
            var statusCode = System.Net.HttpStatusCode.InternalServerError;
            var content = context != null ? context.Exception.Message + context.Exception.StackTrace : String.Empty;

            if (context.Exception is UnauthorizedAccessException || context.Exception is Microsoft.SharePoint.Client.ServerUnauthorizedAccessException)
            {
                statusCode = System.Net.HttpStatusCode.Unauthorized;
            }

            if (context.Exception is UnauthorizedException)
            {
                statusCode = System.Net.HttpStatusCode.Unauthorized;
                if (context?.Exception?.Message != null)
                {
                    content = context.Exception.Message;
                }
            }

            if (context.Exception is NotFoundException)
            {
                statusCode = System.Net.HttpStatusCode.NotFound;
            }

            if (statusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                content = $"System error. Error detail: {content}";
            }

            context.Result = new PASExceptionResult(
                statusCode,
                content,
                context.Request,
                context.Exception);

            return Task.FromResult(0);
        }
    }
}