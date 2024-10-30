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
    public static ITelegramBotClient? bot;
    private const string ClientId = "52506965";
    public static readonly string ClientSecret = "Qsbj6ZrB7cRpU8UHy0SS";
    private const string RedirectUri = "https://t.me/BycarQPDbot";
    private static readonly Dictionary<long,List<string>> Tokens = new Dictionary<long,List<string>>();

    public static async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(JsonConvert.SerializeObject(update));
        if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            switch (message?.Text)
            {
                case "/start":
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
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать! Выберите действие:", replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                    break;
                }
                case "Зарегистрироваться":
                {
                    var chatId = update.Message.Chat.Id;

                    // Формируем ссылку для авторизации   
                    const string authorizationUrl = $"https://oauth.vk.com/authorize?client_id={ClientId}&display=page&redirect_uri={RedirectUri}&scope=email&response_type=code&v=5.131";

                    await botClient.SendTextMessageAsync(update.Message.Chat, "Введите \"Код:< ваш код для продолжения регистрации > \"");

                    // Создаем встроенную клавиатуру с кнопками-ссылками     
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("VK", authorizationUrl),
                        }
                    });

                    await botClient.SendTextMessageAsync(chatId, "Перейдите по ссылке для регистрации", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                    break;
                }
                default:
                {
                    if (message.Text.Contains("Код:"))
                    {
                        var code = message.Text;
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
                            const string url = "https://localhost:7162/api/Users/RegisterOrLogin";
                            var jsonContent = new
                            {
                                username = $"{Methods.Translit(information[0])}",
                                email = $"{information[1]}",
                                password = $"{information[2]}",
                                secretCode = $"{information[3]}"
                            };

                            using var httpClient = new HttpClient();
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
                                Tokens.Add(message.Chat.Id, new List<string>() { token.accessToken, token.refreshToken });

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
                    else switch (message.Text)
                    {
                        case "Посмотреть каталог машин":
                        {
                            var url = "https://localhost:7162/api/Cars/GetFofAll";
                            using var httpClient = new HttpClient();
                            // Установка заголовков
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                            var response = await httpClient.GetAsync(url);
                            var responsData = await response.Content.ReadAsStringAsync();
                            await botClient.SendTextMessageAsync(message.Chat.Id, responsData);
                            break;
                        }
                        case "Купить машину":
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите Купить:<id машины>");
                            break;
                        case "Информация о машине":
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите Информация:<id машины>");
                            break;
                        default:
                        {
                            if (message.Text.Contains("Купить:"))
                            {
                                var carIdstr = "";
                                if (message.Text.Length > 7)
                                {
                                    carIdstr = message.Text.Remove(0, 7);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вы неправильно ввели id машины попробуйте ещё раз");
                                }
                                if (Tokens.TryGetValue(message.Chat.Id, out var token))
                                {
                                    int carId;
                                    var resultParse = int.TryParse(carIdstr, out carId);
                                    if (resultParse)
                                    {
                                        var result = await Methods.BuyOrInformate(token[0], carId, true);
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
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вы не зарегестрированны!!!", replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                                }
                            }
                            else if (message.Text.Contains("Информация:"))
                            {
                                var carId = message.Text.Remove(0, 11);
                                if (Tokens.TryGetValue(message.Chat.Id, out var token))
                                {
                                    var result = await Methods.BuyOrInformate(token[0], int.Parse(carId), true);
                                    await botClient.SendTextMessageAsync(message.Chat.Id, result, cancellationToken: cancellationToken);
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
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вы не зарегестрированны!!!", replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                                }
                            }

                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(JsonConvert.SerializeObject(exception));
        return Task.CompletedTask;
    }

}
