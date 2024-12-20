using SalaryPredictor.Shared.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace SalaryPredictor.Bot;

public static class IoHelper
{
    public static InlineKeyboardMarkup GetInlineKeyboardForStep(string step)
    {
        var callbackData = step + "Info";
        return new InlineKeyboardMarkup(
            InlineKeyboardButton.WithCallbackData("More info", callbackData)
        );
    }

    public static bool ProcessInput(string input, ref SalaryData data, ref string step)
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
                    var validCompanySizes = new[] {"S", "M", "L"};
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