using CarGuideDDD.Core.MapObjects;
using Domain.Entities;
using DTOs;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CarGuideDDD.Domain.Methods
{
    public class CreateRequestCar
    {
        public IHttpClientFactory _httpClientFactory { get; set; }
        public CreateRequestCar(IHttpClientFactory httpClientFactory) 
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<bool> CreatePurchaseRequestOrGetInformationAboutCar(PriorityCarDto priorityCarDto, UserDto clientDto, UserDto? managerDto, bool Buy)
        {
            Car car = Maps.MapPriorityCarDtoToCar(priorityCarDto);
            User client = Maps.MapUserDtoToUser(clientDto);
            User manager = Maps.MapUserDtoToUser(managerDto);
            System.Text.StringBuilder toUserBody = new System.Text.StringBuilder();
            System.Text.StringBuilder toManagerBody = new System.Text.StringBuilder();  

            if (managerDto == null)
            {
                toUserBody.AppendLine("Не получается создать заявку, мы уже работаем над решением этой проблемы");
                return false;
            }
            else
            {
                toUserBody.AppendLine("Вы заинтересовались машиной, вот её данные:");
                toUserBody.AppendLine($"Марка: {car.Make}");
                toUserBody.AppendLine($"Модель: {car.Model}");
                toUserBody.AppendLine($"Цвет: {car.Color}");
                toUserBody.AppendLine("Мы уже перендали менеджеру вашу заявку");

                toManagerBody.AppendLine("Пользователь заинтерисовался машиной, свяжитесь с ним как можно скорее");
                toManagerBody.AppendLine("Покупатель:");
                toManagerBody.AppendLine($"{client.Name}");
                toManagerBody.AppendLine($"{client.Email}");
                toManagerBody.AppendLine("Машина:");
                toUserBody.AppendLine($"Марка: {car.Make}");
                toUserBody.AppendLine($"Модель: {car.Model}");
                toUserBody.AppendLine($"Цвет: {car.Color}");

                string subject;
                if( Buy)
                {
                    subject = "Заявка на покупку машины";
                }
                else
                {
                    subject = "Заявка на получение информации о машине";
                }

                var userAnswer = _httpClientFactory.CreateClient();
                {
                    userAnswer.BaseAddress = new Uri("https://localhost:7288");
                    userAnswer.DefaultRequestHeaders.Accept.Clear();
                    userAnswer.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = $"{{ \"mailRecipient\": \"{client.Email}\", \"subject\": \"{subject}\", \"body\": \"{toUserBody.ToString()}\" }}";

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var result = await userAnswer.PostAsync("/api/Mail/SendMessageToMain", content);
                    await result.Content.ReadAsStringAsync();

                    json = $"{{ \"mailRecipient\": \"{manager.Email}\", \"subject\": \"{subject}\", \"body\": \"{toManagerBody.ToString()}\" }}";
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    result = await userAnswer.PostAsync("/api/Mail/SendMessageToMain", content);
                    await result.Content.ReadAsStringAsync();

                    return true;
                }
            }

            
        }
    }
}
