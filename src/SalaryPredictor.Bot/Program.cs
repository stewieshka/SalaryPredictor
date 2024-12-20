using System.Collections.Concurrent;
using SalaryPredictor.Shared.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SalaryPredictor.Bot;

public static class Program
{
    private static readonly ITelegramBotClient BotClient = new TelegramBotClient("");

    private static readonly ConcurrentDictionary<long, (SalaryData Data, string Step)> UserSessions = new();

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
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken
        );

        Console.WriteLine("Bot is running. Press Enter to stop.");
        Console.ReadLine();

        await cancellationTokenSource.CancelAsync();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message != null)
            {
                await HandleMessageAsync(botClient, update.Message, cancellationToken);
            }
            else if (update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling update: {ex}");
        }
    }

    private static async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Text == null) return;

        var chatId = message.Chat.Id;
        var messageText = message.Text.Trim();

        if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            UserSessions[chatId] = (new SalaryData(), "WorkYear");

            await botClient.SendMessage(
                chatId,
                "Welcome! Let's predict a salary. Please enter the WorkYear (e.g., 2024):",
                replyMarkup: GetInlineKeyboardForStep("WorkYear"),
                cancellationToken: cancellationToken
            );
            return;
        }

        if (!UserSessions.TryGetValue(chatId, out var session))
        {
            await botClient.SendMessage(chatId, "Please start with /start command.", cancellationToken: cancellationToken);
            return;
        }

        var (currentData, currentStep) = session;

        if (!ProcessInput(messageText, ref currentData, ref currentStep))
        {
            await botClient.SendMessage(chatId, $"Invalid input for {currentStep}. Please try again.", cancellationToken: cancellationToken);
            return;
        }

        if (currentStep == "Done")
        {
            var prediction = await Predictor.PredictAsync(currentData);
            await botClient.SendMessage(
                chatId,
                $"Predicted salary (USD): {prediction.PredictedSalaryInUSD:0.##}",
                cancellationToken: cancellationToken
            );
            UserSessions.TryRemove(chatId, out _);
            return;
        }

        UserSessions[chatId] = (currentData, currentStep);

        await botClient.SendMessage(
            chatId,
            $"Please enter {currentStep}:",
            replyMarkup: GetInlineKeyboardForStep(currentStep),
            cancellationToken: cancellationToken
        );
    }

    private static async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message.Chat.Id;
        var callbackData = callbackQuery.Data;

        var infoText = callbackData switch
        {
            "WorkYearInfo" => "WorkYear is the year you started working. For example: 2024.",
            "ExperienceLevelInfo" => "Experience level could be SE, MI, EN, EX.",
            "EmploymentTypeInfo" => "Employment type could be FT, CT, FL, PT.",
            "JobTitleInfo" => "Job Title is your current role, such as Software Engineer.",
            "EmployeeResidenceInfo" => "Employee Residence is the country you currently live in.",
            "RemoteRatioInfo" => "Remote Ratio is the percentage of time you work remotely.",
            "CompanyLocationInfo" => "Company Location is the country where your company is based.",
            "CompanySizeInfo" => "Company Size refers to the size of your company (e.g., Small, Medium, Large).",
            _ => "No information available."
        };

        await botClient.AnswerCallbackQuery(
            callbackQuery.Id,
            infoText,
            showAlert: true,
            cancellationToken: cancellationToken
        );
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    private static InlineKeyboardMarkup GetInlineKeyboardForStep(string step)
    {
        var callbackData = step + "Info";
        return new InlineKeyboardMarkup(
            InlineKeyboardButton.WithCallbackData("More info", callbackData)
        );
    }

    private static bool ProcessInput(string input, ref SalaryData data, ref string step)
    {
        try
        {
            switch (step)
            {
                case "WorkYear":
                    if (!int.TryParse(input, out var workYear) || workYear < 1900 || workYear > DateTime.Now.Year)
                    {
                        return false; // Некорректный год работы
                    }

                    data.WorkYear = workYear;
                    step = "ExperienceLevel";
                    break;

                case "ExperienceLevel":
                    var validExperienceLevels = new[] {"SE", "MI", "EN", "EX"};
                    if (!validExperienceLevels.Contains(input.ToUpperInvariant()))
                    {
                        return false; // Некорректный уровень опыта
                    }

                    data.ExperienceLevel = input.ToUpperInvariant();
                    step = "EmploymentType";
                    break;

                case "EmploymentType":
                    var validEmploymentTypes = new[] {"FT", "CT", "FL", "PT"};
                    if (!validEmploymentTypes.Contains(input.ToUpperInvariant()))
                    {
                        return false; // Некорректный тип занятости
                    }

                    data.EmploymentType = input.ToUpperInvariant();
                    step = "JobTitle";
                    break;

                case "JobTitle":
                    if (string.IsNullOrWhiteSpace(input) || input.Length > 100)
                    {
                        return false; // Некорректное название должности
                    }

                    data.JobTitle = input;
                    step = "EmployeeResidence";
                    break;

                case "EmployeeResidence":
                    if (string.IsNullOrWhiteSpace(input) || input.Length > 2)
                    {
                        return false; // Некорректное местоположение сотрудника
                    }

                    data.EmployeeResidence = input;
                    step = "RemoteRatio";
                    break;

                case "RemoteRatio":
                    if (!int.TryParse(input, out var remoteRatio) || remoteRatio < 0 || remoteRatio > 100)
                    {
                        return false; // Некорректное значение процента удалённой работы
                    }

                    data.RemoteRatio = remoteRatio;
                    step = "CompanyLocation";
                    break;

                case "CompanyLocation":
                    if (string.IsNullOrWhiteSpace(input) || input.Length > 50)
                    {
                        return false; // Некорректное местоположение компании
                    }

                    data.CompanyLocation = input;
                    step = "CompanySize";
                    break;

                case "CompanySize":
                    var validCompanySizes = new[] {"Small", "Medium", "Large"};
                    if (!validCompanySizes.Contains(input, StringComparer.OrdinalIgnoreCase))
                    {
                        return false; // Некорректный размер компании
                    }

                    data.CompanySize = input;
                    step = "Done";
                    break;

                default:
                    return false; // Неизвестный шаг
            }

            return true;
        }
        catch
        {
            return false; // Обработка исключений на случай неожиданных ошибок
        }
    }
}