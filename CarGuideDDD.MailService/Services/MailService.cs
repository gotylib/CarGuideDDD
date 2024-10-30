using System.Text;
using CarGuideDDD.MailService.Objects;
using CarGuideDDD.MailService.Services.Interfaces;
using Newtonsoft.Json;

namespace CarGuideDDD.MailService.Services;

public class MailServices : IMailServices
    {
 
        public bool SendBuyCarMessage(User user, User? manager, Car car)
        {

            var toUserBodyBuild = new StringBuilder()
                            .AppendLine("Вы отправили нам заявку на покупку машины:")
                            .AppendLine($"Марка: {car.Make}")
                            .AppendLine($"Модель: {car.Model}")
                            .AppendLine($"Цвет: {car.Color}")
                            .AppendLine($"Мы уже назначили вам менеджера {manager.Username}")
                            .AppendLine("С вами свяжутся в ближайшее время");
            var toUserBody = toUserBodyBuild.ToString();

            var toManagerBodyBuilder = new StringBuilder()
                                .AppendLine("Заявка на покупку машины:")
                                .AppendLine($"Марка: {car.Make}")
                                .AppendLine($"Модель: {car.Model}")
                                .AppendLine($"Цвет: {car.Color}")
                                .AppendLine($"Свяжитесь с покупателем: Имя: {user.Username}, Email: {user.Email}");
            var toManagerBody = toManagerBodyBuilder.ToString();

            const string subject = "Заявка на покупку машины";

            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = toUserBody
            };
            // Отправка сообщения пользователю

            var jsonUser = JsonConvert.SerializeObject(dataUser);

            var dataManager = new
            {
                mailRecipient = manager.Email,
                subject = subject,
                body = toManagerBody
            };

            var jsonManager = JsonConvert.SerializeObject(dataManager);

            _producerHostedService.SendMessage(jsonUser);
            _producerHostedService.SendMessage(jsonManager);

            return true;

        }

        public bool SendInformCarMessage(User user, User manager, Car car)
        {
            var toUserBodyBuild = new StringBuilder()
                            .AppendLine("Вы отправили нам заявку на предоставление более полной информации о машине:")
                            .AppendLine($"Марка:{car.Make}")
                            .AppendLine($"Модель:{car.Model}")
                            .AppendLine($"Цвет:{car.Color}")
                            .AppendLine($"Мы уже назначили вам менеджера {manager.Username}")
                            .AppendLine($"С вами свяжутся в ближайшее время");

            var toUserBody = toUserBodyBuild.ToString();

            var toManagerBodyBuild = new StringBuilder()
                              .AppendLine("Заявку на предоставление более полной информации о машине:")
                              .AppendLine($"Марка:{car.Make}")
                              .AppendLine($"Модель:{car.Model}")
                              .AppendLine($"Цвет:{car.Color}")
                              .AppendLine($"Свяжитесь с покупателем: Имя:{user.Username}, Email:{user.Email}");

            var toManagerBody = toManagerBodyBuild.ToString();

            const string subject = "Заявка на предоставление более полной информации о машине";



            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = toUserBody
            };
            // Отправка сообщения пользователю

            var jsonUser = JsonConvert.SerializeObject(dataUser);

            var dataManager = new
            {
                mailRecipient = manager.Email,
                subject = subject,
                body = toManagerBody
            };

            var jsonManager = JsonConvert.SerializeObject(dataManager);

            _producerHostedService.SendMessage(jsonUser);
            _producerHostedService.SendMessage(jsonManager);

            return true;
        }

        public bool SendUserNoHaveCarMessage(User user, Car car)
        {
            const string subject = "Сообщение об ошибке: нет машин на складе";
            const string toUserBody = "Извините, сейчас на складе нет машины, которую вы хотите купить. Как только она появиться мы вам сообщим";

            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = toUserBody
            };
            // Отправка сообщения пользователю

            var jsonUser = JsonConvert.SerializeObject(dataUser);
            var contentUser = new StringContent(jsonUser, Encoding.UTF8, "application/json");

            _producerHostedService.SendMessage(jsonUser);

            return true;
        }

        public bool SendUserNotFountManagerMessage(User user)
        {
            const string subject = "Сообщение об ошибки";
            const string body = "Извините сейчас нет свободных менеджеров, приносим свои извиения.";

            var dataUser = new
            {
                mailRecipient = user.Email,
                subject = subject,
                body = body
            };
            // Отправка сообщения пользователю

            var jsonUser = JsonConvert.SerializeObject(dataUser);
            var contentUser = new StringContent(jsonUser, Encoding.UTF8, "application/json");

            _producerHostedService.SendMessage(jsonUser);
            return true;

        }
    }