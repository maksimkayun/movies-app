using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MoviesApp.Controllers;

namespace MoviesApp.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            if (httpContext.Request.Path.ToString().Contains("/Artists"))
            {
                StringBuilder sb = new StringBuilder();
                if (httpContext.Request.Method == "POST")
                {
                    var values = httpContext.Request.Form;
                    foreach (var v in values)
                    {
                        sb.AppendLine(v.Key + ": " + v.Value);
                    }
                    logger.LogInformation($"TypeRequest {httpContext.Request.Path}  Method: " +
                                          $"{httpContext.Request.Method} \n      Params:\n{sb}");
                }
                else
                {
                    logger.LogInformation($"TypeRequest {httpContext.Request.Path}  Method: " +
                                          $"{httpContext.Request.Method}"); 
                }
                
            }
            
            await _next(httpContext);
        }
    }
}