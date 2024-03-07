using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PAS.API.ErrorHandler
{
    public class PASExceptionResult : IHttpActionResult
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _message;
        private readonly HttpRequestMessage _request;
        private readonly Exception _exception;

        public PASExceptionResult(HttpStatusCode statusCode, string message, HttpRequestMessage request, Exception exception)
        {
            _statusCode = statusCode;
            _message = message;
            _request = request;
            _exception = exception;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_request.CreateErrorResponse(_statusCode, _message));
        }
    }
}