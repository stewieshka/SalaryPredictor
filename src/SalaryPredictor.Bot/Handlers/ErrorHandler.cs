using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace SalaryPredictor.Bot.Handlers;

public static class ErrorHandler
{
    public static Task HandleAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}