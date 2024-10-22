using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http.Headers;
using System.Text;
using CarGuideDDD.TelegramBot.ProcessingMethods;

public class Bot 
{
    public static ITelegramBotClient bot;
    private static readonly string clientId = "52506965";   
    private static readonly string clientSecret = "Qsbj6ZrB7cRpU8UHy0SS"; 
    private static readonly string redirectUri = "https://t.me/BycarQPDbot"; 
    private static readonly Dictionary<long,List<string>> tokens = new Dictionary<long,List<string>>();

    public static async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(JsonConvert.SerializeObject(update));
        if (update.Type == UpdateType.Message)
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
                var information = await Methods.HandleRedirect(code);
                if (information[0] == "Error")
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat, "У вас не получилось войти, попробуйте ещё раз.");
                }
                else
                {
                    information.Add("Az100Az.");
                    information.Add("Null");
                    var url = "https://localhost:7162/api/Users/RegisterOrLogin";
                    var jsonContent = new
                    {
                        username = $"{Methods.Translit(information[0])}",
                        email = $"{information[1]}",
                        password = $"{information[2]}",
                        secretCode = $"{information[3]}"
                    };

                    using (var httpClient = new HttpClient())
                    {
                        // Установка заголовков
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                        // Сериализация объекта в JSON
                        var json = JsonConvert.SerializeObject(jsonContent);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        // Отправка POST-запроса
                        var response = await httpClient.PostAsync(url, content);

                        // Проверка ответа
                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            var token = JsonConvert.DeserializeObject<Token>(responseData);
                            Console.WriteLine("Access Token: " + token.accessToken);
                            Console.WriteLine("Refresh Token: " + token.refreshToken);
                            tokens.Add(message.Chat.Id, new List<string>() { token.accessToken, token.refreshToken });

                            var replyKeyboard = new ReplyKeyboardMarkup(
                                new List<KeyboardButton[]>()
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Посмотреть каталог машин"),
                                        new KeyboardButton("Купить машину"),
                                        new KeyboardButton("Информация о машине"),
                                    },
                                })
                            {
                                ResizeKeyboard = true,
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать! Выберите действие:", replyMarkup: replyKeyboard);
                        }
                        else
                        {
                            Console.WriteLine("Error: " + response.StatusCode);
                        }
                    }
                }
            }
            else if (message.Text == "Посмотреть каталог машин")
            {
                var url = "https://localhost:7162/api/Cars/GetFofAll";
                using (var httpClient = new HttpClient())
                {
                    // Установка заголовков
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                    var response = await httpClient.GetAsync(url);
                    var responsData = await response.Content.ReadAsStringAsync();
                    await botClient.SendTextMessageAsync(message.Chat.Id, responsData);
                }

            }
            else if (message.Text == "Купить машину")
            {

                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите Купить:<id машины>");
            }
            else if (message.Text == "Информация о машине")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите Информация:<id машины>");
            }
            else if (message.Text.Contains("Купить:"))
            {
                string carIdstr = "";
                if (message.Text.Length > 7)
                {
                    carIdstr = message.Text.Remove(0, 7);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вы неправильно ввели id машины попробуйте ещё раз");
                }
                if (tokens.ContainsKey(message.Chat.Id))
                {
                    int carId;
                    bool resultParse = int.TryParse(carIdstr, out carId);
                    if (resultParse)
                    {
                        var result = await Methods.BuyOrInformate(tokens[message.Chat.Id][0], carId, true);
                        await botClient.SendTextMessageAsync(message.Chat.Id, result);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Вы неправильно ввели id машины попробуйте ещё раз");
                    }
                }
                else
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
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вы не зарегестрированны!!!", replyMarkup: replyKeyboard);
                }
            }
            else if (message.Text.Contains("Информация:"))
            {
                var carId = message.Text.Remove(0, 11);
                if (tokens.ContainsKey(message.Chat.Id))
                {
                    var result = await Methods.BuyOrInformate(tokens[message.Chat.Id][0], int.Parse(carId), true);
                    await botClient.SendTextMessageAsync(message.Chat.Id, result);
                }
                else
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
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вы не зарегестрированны!!!", replyMarkup: replyKeyboard);
                }
            }
        }
    }
    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(JsonConvert.SerializeObject(exception));
    }

}
