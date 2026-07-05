using CleanIdentity.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CleanIdentity.Infrastructure.Middleware;

public sealed class IpAllowListMiddleware
{
    private readonly RequestDelegate _next;

    public IpAllowListMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<ApplicationSecurityOptions> options)
    {
        var allowed = options.Value.AllowedIpAddresses;
        if (allowed.Length == 0)
        {
            await _next(context);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        var isAllowed = remoteIp is not null && allowed.Contains(remoteIp, StringComparer.OrdinalIgnoreCase);

        if (!isAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Dostęp z tego adresu IP jest zablokowany.");
            return;
        }

        await _next(context);
    }
}
