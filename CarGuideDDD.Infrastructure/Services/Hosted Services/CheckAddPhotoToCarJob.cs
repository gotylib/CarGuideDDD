
using CarGuideDDD.Core.MailSendObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using static System.Formats.Asn1.AsnWriter;


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
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var _applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var _producerHostedService = scope.ServiceProvider.GetRequiredService<ProducerHostedService>();

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var make = dataMap.GetString("make");
            var model = dataMap.GetString("model");
            var color = dataMap.GetString("color");
            if (string.IsNullOrEmpty(make) || string.IsNullOrEmpty(model) || string.IsNullOrEmpty(color))
            {
                return;      
            }

            var car = await _applicationDbContext.Cars
                        .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Color == color);


            if (car != null && car.NameOfPhoto.IsNullOrEmpty())
            {
                var user = _applicationDbContext.Users.FirstOrDefault(u => u.UserName == car.AddUserName);
                if (user == null)
                { 
                    _logger.LogError("Don't have user for job");
                    return;
                }
                var sendMessaga = new MailSendObj
                {
                    EventId = 4,
                    User = Maps.MapEntityUserToMailUser(user),
                    Score = 1
                };

                _producerHostedService.SendMessage(sendMessaga);
            }
            else
            {
                _logger.LogError("Don't have care for job");
                return;
            }
        }
    }
}
