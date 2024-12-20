using Telegram.Bot;
using Telegram.Bot.Types;

namespace SalaryPredictor.Bot.Handlers;

public static class CallbackQueryHandler
{
    public static async Task HandleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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
            "RemoteRatioInfo" => "Remote Ratio is the percentage of time you work remotely (e.g. US).",
            "CompanyLocationInfo" => "Company Location is the country where your company is based (e.g. US).",
            "CompanySizeInfo" => "Company Size refers to the size of your company (e.g., S, M, L).",
            _ => "No information available."
        };

        await botClient.AnswerCallbackQuery(
            callbackQuery.Id,
            infoText,
            showAlert: true,
            cancellationToken: cancellationToken
        );
    }
}