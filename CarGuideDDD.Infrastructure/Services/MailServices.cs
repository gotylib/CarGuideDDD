

using CarGuideDDD.Infrastructure.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;

namespace CarGuideDDD.Infrastructure.Services
{
    public class MailServices : IMailServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public MailServices(IHttpClientFactory httpClientFactory) 
        { 
            _httpClientFactory = httpClientFactory;
        }

        public Task<bool> SendBuyCarMessageAsync(string clientEmail, string managerEmail)
        {
            throw new NotImplementedException();
        }


        public Task<bool> SendInformCarMessageAsync(string clientEmail, string managerEmail)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendUserNotFountManagerMessageAsync(string email)
        {
            string subject = "Сообщение об ошибки";
            string body = "Не получается создать заявку, мы уже работаем над решением этой проблемы";
            var userAnswer = _httpClientFactory.CreateClient();
            {
                userAnswer.BaseAddress = new Uri("https://localhost:7288");
                userAnswer.DefaultRequestHeaders.Accept.Clear();
                userAnswer.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = $"{{ \"mailRecipient\": \"{email}\", \"subject\": \"{subject}\", \"body\": \"{body}\" }}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    await userAnswer.PostAsync("/api/Mail/SendMessageToMain", content);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }

            }

        }
    }
}
