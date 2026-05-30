 using System.Threading.RateLimiting;

 namespace Quitly.Api.Infrastructure.Security;

 public static class RateLimitingExtensions
 {
     public static IServiceCollection AddQuitlyRateLimiting(this IServiceCollection services)
     {
         services.AddRateLimiter(options =>
         {
             options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
             options.AddFixedWindowLimiter("auth", limiter =>
             {
                 limiter.PermitLimit = 10;
                 limiter.Window = TimeSpan.FromMinutes(1);
                 limiter.QueueLimit = 0;
             });

             options.AddFixedWindowLimiter("writes", limiter =>
             {
                 limiter.PermitLimit = 30;
                 limiter.Window = TimeSpan.FromMinutes(1);
                 limiter.QueueLimit = 0;
             });
         });

         return services;
     }

     public static IApplicationBuilder UseQuitlySecurityHeaders(this IApplicationBuilder app)
     {
         return app.Use(async (context, next) =>
         {
             context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
             context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
             context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
             context.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

             if (!context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
             {
                 context.Response.Headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
             }

             await next();
         });
     }
 }