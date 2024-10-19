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
        public bool CreatePurchaseRequestOrGetInformationAboutCar(PriorityCarDto priorityCarDto, UserDto clientDto, UserDto managerDto, bool BuyOrInformate)
        {
            Car car = Maps.MapPriorityCarDtoToCar(priorityCarDto);
            User client = Maps.MapUserDtoToUser(clientDto);
            User manager = Maps.MapUserDtoToUser(managerDto);
            System.Text.StringBuilder toUserBody = new System.Text.StringBuilder();

            toUserBody.AppendLine("Вы заинтересовались машиной, вот её данные:");
            toUserBody.AppendLine($"Марка: {car.Make}");
            toUserBody.AppendLine($"Модель: {car.Model}");
            toUserBody.AppendLine($"Цвет: {car.Color}");

            if (BuyOrInformate)
            {
                using (var userAnswer = new HttpClient())
                {
                    userAnswer.BaseAddress = new Uri("https://localhost:7288");
                    userAnswer.DefaultRequestHeaders.Accept.Clear();
                    userAnswer.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = $"{{ \"mailRecipient\": \"{client.Email}\", \"subject\": \"Заявка на покупку машины\", \"body\": \"{toUserBody.ToString()}\" }}";

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var result = await userAnswer.PostAsync("/api/Mail/SendMessageToMain", content);
                    return await result.Content.ReadAsStringAsync();
                }
            }
            else
            {

            }
        }
    }
}
