using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using LoggerService;
using Logging.Models;

namespace Logging
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;

        public ExceptionMiddleware(RequestDelegate next, ILoggerManager logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError("something went wrong: {ex}");
                await HandleExcepionAsync(httpContext, ex);
            }
        }

        public async Task HandleExcepionAsync(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await httpContext.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = "Internal Server Error from the custome middleware."
            }.ToString());
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
        public static void ConfigureCustomExceptionMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }

}
