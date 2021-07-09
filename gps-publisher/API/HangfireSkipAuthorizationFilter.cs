using Hangfire.Dashboard;

namespace API
{
    internal class HangfireSkipAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}