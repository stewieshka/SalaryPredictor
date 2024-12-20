using Telegram.Bot;
using Telegram.Bot.Types;

namespace SalaryPredictor.Bot.Handlers;

public static class UpdateHandler
{
    public static async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message != null)
            {
                await MessageHandler.HandleAsync(botClient, update.Message, cancellationToken);
            }
            else if (update.CallbackQuery != null)
            {
                await CallbackQueryHandler.HandleAsync(botClient, update.CallbackQuery, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling update: {ex}");
        }
    }
}