using Relay.Api.Middleware;

namespace Relay.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseMiddlewarePipeline(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseMiddleware<CorrelationMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        //if (app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("RelayPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
