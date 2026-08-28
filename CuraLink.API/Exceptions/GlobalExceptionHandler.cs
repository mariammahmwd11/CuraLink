using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CuraLink.Application.Common.Exceptions;
namespace CuraLink.API.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");

            if (exception is NotFoundException)
            {
                httpContext.Response.StatusCode =
                    StatusCodes.Status404NotFound;

                await httpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        message = exception.Message
                    },
                    cancellationToken);

                return true;
            }

            if (exception is UnauthorizedAccessException)
            {
                httpContext.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await httpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        message = exception.Message
                    },
                    cancellationToken);

                return true;
            }

            return false;
        }
    }
}