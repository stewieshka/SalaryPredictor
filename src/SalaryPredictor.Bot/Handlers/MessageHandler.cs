using SalaryPredictor.Shared.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SalaryPredictor.Bot.Handlers;

public static class MessageHandler
{
    public static async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Text == null) return;

        var chatId = message.Chat.Id;
        var messageText = message.Text.Trim();

        if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            UserSessions.Values[chatId] = (new SalaryData(), "WorkYear");

            await botClient.SendMessage(
                chatId,
                "Welcome! Let's predict a salary. Please enter the WorkYear (e.g., 2024):",
                replyMarkup: IoHelper.GetInlineKeyboardForStep("WorkYear"),
                cancellationToken: cancellationToken
            );
            return;
        }

        if (!UserSessions.Values.TryGetValue(chatId, out var session))
        {
            await botClient.SendMessage(chatId, "Please start with /start command.", cancellationToken: cancellationToken);
            return;
        }

        var (currentData, currentStep) = session;

        if (!IoHelper.ProcessInput(messageText, ref currentData, ref currentStep))
        {
            await botClient.SendMessage(chatId, $"Invalid input for {currentStep}. Please try again.", cancellationToken: cancellationToken);
            return;
        }

        if (currentStep == "Done")
        {
            var prediction = await Predictor.PredictAsync(currentData);
            await botClient.SendMessage(
                chatId,
                $"Predicted salary: ${prediction.PredictedSalaryInUSD:0} / year",
                cancellationToken: cancellationToken
            );
            UserSessions.Values.TryRemove(chatId, out _);
            return;
        }

        UserSessions.Values[chatId] = (currentData, currentStep);

        await botClient.SendMessage(
            chatId,
            $"Please enter {currentStep}:",
            replyMarkup: IoHelper.GetInlineKeyboardForStep(currentStep),
            cancellationToken: cancellationToken
        );
    }
}