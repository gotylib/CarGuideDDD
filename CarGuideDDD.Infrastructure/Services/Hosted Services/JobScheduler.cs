using Quartz;


namespace CarGuideDDD.Infrastructure.Services.Hosted_Services
{
    public class JobScheduler
    {
        private readonly IScheduler _scheduler; 

        public JobScheduler(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task ScheduleJop(string make, string model, string color , TimeSpan delay)
        {
            IJobDetail job = JobBuilder.Create<CheckAddPhotoToCarJob>()
                .UsingJobData("make", make)
                .UsingJobData("model", model)
                .UsingJobData("color", color)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.UtcNow.Add(delay))
                .Build();

            await _scheduler.ScheduleJob(job, trigger); 
        }
    }
}