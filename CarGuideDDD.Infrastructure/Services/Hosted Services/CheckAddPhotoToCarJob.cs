
using CarGuideDDD.Core.MailSendObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;


namespace CarGuideDDD.Infrastructure.Services.Hosted_Services
{
    public class CheckAddPhotoToCarJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private ILogger<CheckAddPhotoToCarJob> _logger;
        public CheckAddPhotoToCarJob(IServiceScopeFactory serviceScopeFactory ,ILogger<CheckAddPhotoToCarJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        public Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var _applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var _producerHostedService = scope.ServiceProvider.GetRequiredService<ProducerHostedService>();
            
            var triggerTime = DateTime.Now.ToUniversalTime().AddHours(-1);

            var uniqueEmails = _applicationDbContext.CarWithoutPhotos
            .Where(car => car.AddTime <= triggerTime && car.AddUserName != null)
            .Select(car => car.AddUserName)
            .Distinct()
            .Join(_applicationDbContext.Users,
                addUserName => addUserName,
                user => user.UserName,
                (addUserName, user) => user.Email)
            .Distinct()
            .ToList();

            if (uniqueEmails.Any())
            {
                var jsonUniqueEmails = JsonConvert.SerializeObject(uniqueEmails);

                _producerHostedService.SendMessage(jsonUniqueEmails, "MailSendJob");
            }

            return Task.CompletedTask;
        }
    }
}
