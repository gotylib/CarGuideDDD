

using CarGuideDDD.Infrastructure.Repositories;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using DTOs;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CarGuideDDD.Infrastructure.Services
{
    public class MailServices : IMailServices
    {
        IHttpClientFactory _httpClientFactory;

        public MailServices(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<bool> SendBuyCarMessageAsync(UserDto user, UserDto manager, PriorityCarDto car)
        {

            StringBuilder toUserBodyBuild = new StringBuilder();
            toUserBodyBuild.AppendLine("Вы отправили нам заявку на покупку машины:");
            toUserBodyBuild.AppendLine($"Марка: {car.Make}");
            toUserBodyBuild.AppendLine($"Модель: {car.Model}");
            toUserBodyBuild.AppendLine($"Цвет: {car.Color}");
            toUserBodyBuild.AppendLine($"Мы уже назначили вам менеджера {manager.Username}");
            toUserBodyBuild.AppendLine("С вами свяжутся в ближайшее время");
            var toUserBody = toUserBodyBuild.ToString();

            StringBuilder toManagerBodyBuilder = new StringBuilder();
            toManagerBodyBuilder.AppendLine("Заявка на покупку машины:");
            toManagerBodyBuilder.AppendLine($"Марка: {car.Make}");
            toManagerBodyBuilder.AppendLine($"Модель: {car.Model}");
            toManagerBodyBuilder.AppendLine($"Цвет: {car.Color}");
            toManagerBodyBuilder.AppendLine($"Свяжитесь с покупателем: Имя: {user.Username}, Email: {user.Email}");
            var toManagerBody = toManagerBodyBuilder.ToString();

            string subject = "Заявка на покупку машины";
            bool clientResult = false;
            bool managerResult = false;

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7288/api/Mail/SendMessageToMain");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = toUserBody
            };
            // Отправка сообщения пользователю
           
            var jsonUser = JsonConvert.SerializeObject(dataUser);
            var contentUser = new StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                var userResponse = await client.PostAsync(client.BaseAddress, contentUser);
                userResponse.EnsureSuccessStatusCode(); // Проверка успешного ответа
                clientResult = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                clientResult = false;
            }

            var dataManager = new
            {
                mailRecipient = manager.Email,
                subject = subject,
                body = toManagerBody
            };

            var jsonManager = JsonConvert.SerializeObject(dataManager);
            var contentManager = new StringContent(jsonManager, Encoding.UTF8, "application/json");
            try
            {
                var managerResponse = await client.PostAsync(client.BaseAddress, contentManager);
                managerResponse.EnsureSuccessStatusCode(); // Проверка успешного ответа
                managerResult = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                managerResult = false;
            }

            return clientResult && managerResult; 

        }

        public async Task<bool> SendInformCarMessageAsync(UserDto user, UserDto manager, PriorityCarDto car)
        {
            StringBuilder toUserBodyBuild = new StringBuilder();

            toUserBodyBuild.AppendLine("Вы отправили нам заявку на предоставление более полной информации о машине:");
            toUserBodyBuild.AppendLine($"Марка:{car.Make}");
            toUserBodyBuild.AppendLine($"Модель:{car.Model}");
            toUserBodyBuild.AppendLine($"Цвет:{car.Color}");
            toUserBodyBuild.AppendLine($"Мы уже назначили вам менеджера {manager.Username}");
            toUserBodyBuild.AppendLine($"С вами свяжутся в ближайшее время");

            var toUserBody = toUserBodyBuild.ToString();

            StringBuilder toManagerBodyBuilder = new StringBuilder();
            toUserBodyBuild.AppendLine("Заявку на предоставление более полной информации о машине:");
            toUserBodyBuild.AppendLine($"Марка:{car.Make}");
            toUserBodyBuild.AppendLine($"Модель:{car.Model}");
            toUserBodyBuild.AppendLine($"Цвет:{car.Color}");
            toUserBodyBuild.AppendLine($"Свяжитесь с покупателем: Имя:{user.Username}, Email:{user.Email}");

            var toManagerBody = toUserBodyBuild.ToString();

            string subject = "Заявка на предоставление более полной информации о машине";

            bool clientResult = false;
            bool managerResult = false;

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7288/api/Mail/SendMessageToMain");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = toUserBody
            };
            // Отправка сообщения пользователю

            var jsonUser = JsonConvert.SerializeObject(dataUser);
            var contentUser = new StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                var userResponse = await client.PostAsync(client.BaseAddress, contentUser);
                userResponse.EnsureSuccessStatusCode(); // Проверка успешного ответа
                clientResult = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                clientResult = false;
            }

            var dataManager = new
            {
                mailRecipient = manager.Email,
                subject = subject,
                body = toManagerBody
            };

            var jsonManager = JsonConvert.SerializeObject(dataManager);
            var contentManager = new StringContent(jsonManager, Encoding.UTF8, "application/json");
            try
            {
                var managerResponse = await client.PostAsync(client.BaseAddress, contentManager);
                managerResponse.EnsureSuccessStatusCode(); // Проверка успешного ответа
                managerResult = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                managerResult = false;
            }

            return clientResult && managerResult;
        }

        public async Task<bool> SendUserNoHaweCarMessageAsync(UserDto user, PriorityCarDto car)
        {
            string subject = "Сообщение об ошибке: нет машин на складе";
            string toUserBody = "Извините, сейчас на складе нет машины, которую вы хотите купить. Как только она появиться мы вам сообщим";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7288/api/Mail/SendMessageToMain");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = toUserBody
            };
            // Отправка сообщения пользователю

            var jsonUser = JsonConvert.SerializeObject(dataUser);
            var contentUser = new StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                var userResponse = await client.PostAsync(client.BaseAddress, contentUser);
                userResponse.EnsureSuccessStatusCode(); // Проверка успешного ответа
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                return false;
            }

        }

        public async Task<bool> SendUserNotFountManagerMessageAsync(UserDto user)
        {
            string subject = "Сообщение об ошибки";
            string body = "Извините сейчас нет свободных менеджеров, приносим свои извиения.";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7288/api/Mail/SendMessageToMain");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = body
            };
            // Отправка сообщения пользователю

            var jsonUser = JsonConvert.SerializeObject(dataUser);
            var contentUser = new StringContent(jsonUser, Encoding.UTF8, "application/json");

            try
            {
                var userResponse = await client.PostAsync(client.BaseAddress, contentUser);
                userResponse.EnsureSuccessStatusCode(); // Проверка успешного ответа
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                return false;
            }


        }
    }
}


