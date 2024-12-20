using System.Collections.Concurrent;
using SalaryPredictor.Shared.Models;

namespace SalaryPredictor.Bot;

public static class UserSessions
{
    public static readonly ConcurrentDictionary<long, (SalaryData Data, string Step)> Values = new();
}