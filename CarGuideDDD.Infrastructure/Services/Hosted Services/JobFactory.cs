

using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace CarGuideDDD.Infrastructure.Services.Hosted_Services
{
    public class JobFactory : IJobFactory
    {
        private readonly IScheduler _scheduler;

        public JobFactory(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            // Получаем тип задания из bundle
            return (IJob)ActivatorUtilities.GetServiceOrCreateInstance(
                _scheduler.Context.GetService<IServiceProvider>(),
                bundle.JobDetail.JobType);
        }

        public void ReturnJob(IJob job)
        {
            // Логика, если вам нужно вернуть job (например, для переиспользования)
        }
    }
}
