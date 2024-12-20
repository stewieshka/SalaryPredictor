using SalaryPredictor.Bot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace SalaryPredictor.Bot;

public static class Program
{
    private static readonly ITelegramBotClient BotClient = new TelegramBotClient("");

    public static async Task Main()
    {
        Console.WriteLine("Bot is starting...");

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = []
        };

        BotClient.StartReceiving(
            UpdateHandler.HandleAsync,
            ErrorHandler.HandleAsync,
            receiverOptions,
            cancellationToken
        );

        Console.WriteLine("Bot is running. Press Enter to stop.");
        Console.ReadLine();

        await cancellationTokenSource.CancelAsync();
    }
}