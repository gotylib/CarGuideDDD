using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

Bot.bot = new TelegramBotClient("8041332399:AAGPCvDrFXD9ZV74a9nyztJptNEuXe90pdI");

Console.WriteLine("Запущен бот " + Bot.bot.GetMeAsync().Result.FirstName);

var cts = new CancellationTokenSource();

var cancellationToken = cts.Token;

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message } // Пример: получение только обновлений с типом Message
};


Bot.bot.StartReceiving(
    Bot.HandlerUpdateAsync,
    Bot.HandleErrorAsync,
    receiverOptions,
    cancellationToken
);
Console.ReadLine();