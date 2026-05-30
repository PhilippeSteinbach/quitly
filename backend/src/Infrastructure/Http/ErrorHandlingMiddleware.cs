using System.Net;

namespace Quitly.Api.Infrastructure.Http;

public static class ErrorHandlingMiddleware
{
    public static IApplicationBuilder UseQuitlyErrorHandling(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (ArgumentException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "validation_error",
                        message = exception.Message
                    }
                });
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "unexpected_error",
                        message = "An unexpected error occurred.",
                        detail = exception.Message
                    }
                });
            }
        });
    }
}
