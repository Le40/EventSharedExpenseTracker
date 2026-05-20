using Microsoft.AspNetCore.Diagnostics;

namespace EventSharedExpenseTracker.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAppRequestLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("RequestLogging");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            logger.LogInformation(
                "Request started {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await next();

            stopwatch.Stop();

            logger.LogInformation(
                "Request finished {Method} {Path} with {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        });

        return app;
    }

    public static IApplicationBuilder UseAppExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");

                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

                if (exceptionFeature?.Error != null)
                {
                    logger.LogError(
                        exceptionFeature.Error,
                        "Unhandled exception occurred while processing request {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);
                }

                context.Response.Redirect("/Home/Error");
                await Task.CompletedTask;
            });
        });

        return app;
    }
}
