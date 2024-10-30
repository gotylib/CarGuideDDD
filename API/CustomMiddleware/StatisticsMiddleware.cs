using CarGuideDDD.Infrastructure.Services.Interfaces;

namespace API.CustomMiddleware
{
    public class StatisticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public StatisticsMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var statisticsService = scope.ServiceProvider.GetRequiredService<IStatisticsService>();
                var endpoint = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";
                var queryString = context.Request.QueryString.ToString();

                await statisticsService.RecordVisit(endpoint, queryString);
            }

            await _next(context);
        }
    }


}
