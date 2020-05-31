using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(IWebHostEnvironment environment, ILogger<GlobalExceptionFilter> logger)
        {
            this._environment = environment;
            this._logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var json = new JsonErrorResponse();
            if (context.Exception.GetType() == typeof(UserOperationException))
            {
                json.Message = context.Exception.Message;
                context.Result = new BadRequestObjectResult(json);
            }
            else
            {
                json.Message = "发生了未知错误";
                if (_environment.IsDevelopment())
                {
                    json.DevMessage = context.Exception.StackTrace;
                }
                context.Result = new InternalServerErrorObjectResult(json);
            }

            _logger.LogError(context.Exception, context.Exception.Message);
            context.ExceptionHandled = true;
        }
    }

    public class InternalServerErrorObjectResult : ObjectResult
    {

        public InternalServerErrorObjectResult(object error):base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    public class UserOperationException :Exception
    {
        public UserOperationException() { }
        public UserOperationException(string message) : base(message) { }
        public UserOperationException(string message,Exception innerException) : base(message, innerException) { }

    }

    public class JsonErrorResponse
    {
        public string Message { get; set; }
        public string DevMessage { get; set; }

    }
}
