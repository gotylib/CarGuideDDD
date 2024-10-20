using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;

public class Bot 
{
    public static ITelegramBotClient bot;
    private static readonly string clientId = "52506965";   
    private static readonly string clientSecret = "Qsbj6ZrB7cRpU8UHy0SS"; 
    private static readonly string redirectUri = "https://t.me/BycarQPDbot"; 

    public static async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;
            if (message.Text == "/start")
            {
                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Зарегистрироваться"),
                        },
                    })
                {
                    ResizeKeyboard = true,
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать! Выберите действие:", replyMarkup: replyKeyboard);
            }
            else if (message.Text == "Зарегистрироваться")
            {
                var chatId = update.Message.Chat.Id;

                // Формируем ссылку для авторизации   
                string authorizationUrl = $"https://oauth.vk.com/authorize?client_id={clientId}&display=page&redirect_uri={redirectUri}&scope=email&response_type=code&v=5.131";

                await botClient.SendTextMessageAsync(update.Message.Chat, "Введите \"Код:< ваш код для продолжения регистрации > \"");

                // Создаем встроенную клавиатуру с кнопками-ссылками     
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithUrl("VK", authorizationUrl),
                }
            });

                await botClient.SendTextMessageAsync(chatId, "Перейдите по ссылке для регистрации", replyMarkup: inlineKeyboard);
            }
            else if (message.Text.Contains("Код:"))
            {
                string code = message.Text;
                code = code.Remove(0, 4);
                await HandleRedirect(code);
            }

        }
    }
    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }

    public static async Task<string> GetAccessTokenAsync(string code)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                var response = await httpClient.GetStringAsync($"https://oauth.vk.com/access_token?client_id={clientId}&client_secret={clientSecret}&redirect_uri={redirectUri}&scope=email&code={code}");
                dynamic jsonResponse = JsonConvert.DeserializeObject(response);
                return jsonResponse.access_token;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return "Error";
            }

        }
    }

    public static async Task<dynamic> GetUserInfoAsync(string accessToken)
    {
        if(accessToken == "Error")
        {
            return null;
        }
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetStringAsync($"https://api.vk.com/method/users.get?access_token={accessToken}&fields=email&v=5.131");
            dynamic userInfo = JsonConvert.DeserializeObject(response);

            // Проверка на наличие ошибок в ответе 
            if (userInfo.error != null)
            {
                Console.WriteLine($"Error: {userInfo.error.message} (code: {userInfo.error.error_code})");
                return null;
            }

            return userInfo.response[0];
        }
    }

    public static async Task HandleRedirect(string code)
    {
        string accessToken = await GetAccessTokenAsync(code);
        var userInfo = await GetUserInfoAsync(accessToken);

        if (userInfo != null)
        {
            // Теперь можно получить email, но он может быть null
            string email = userInfo.email ?? "Email не предоставлен"; // обрабатываем случай, если email отсутствует
            Console.WriteLine($"User ID: {userInfo.id}, Name: {userInfo.first_name}, Email: {email}");
        }
        else
        {
            Console.WriteLine("Не удалось получить информацию о пользователе.");
        }
    }

}

