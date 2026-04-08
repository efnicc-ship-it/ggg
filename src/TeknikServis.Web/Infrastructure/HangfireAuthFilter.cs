using Hangfire.Dashboard;

namespace TeknikServis.Web.Infrastructure;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.IsInRole("SuperAdmin") || httpContext.User.IsInRole("TenantOwner");
    }
}
