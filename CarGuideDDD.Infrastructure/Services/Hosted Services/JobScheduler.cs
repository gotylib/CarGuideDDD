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

        public async Task ScheduleJob(string make, string model, string color)
        {
            var jobKey = new JobKey("checkAddPhotoToCarJob");

            var jobData = new JobDataMap
        {
            { "make", make },
            { "model", model },
            { "color", color }
        };

            var jobDetail = JobBuilder.Create<CheckAddPhotoToCarJob>()
                .WithIdentity(jobKey)
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .ForJob(jobDetail)
                .WithIdentity("checkAddPhotoToCarJob-trigger")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromMinutes(1))  // Пример интервала
                    .RepeatForever())
                .Build();

            await _scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}