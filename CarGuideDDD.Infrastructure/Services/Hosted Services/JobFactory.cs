

using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace CarGuideDDD.Infrastructure.Services.Hosted_Services
{
    public class JobFactory : IJobFactory
    {
        protected readonly IServiceScopeFactory _scopeFactory;

        public JobFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            using var scope = _scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetServices(bundle.JobDetail.JobType) as IJob;
            return job;
        }

        public void ReturnJob(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}
