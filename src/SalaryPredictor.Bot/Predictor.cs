using Microsoft.ML;
using SalaryPredictor.Shared.Models;
using SalaryPredictor.Training.Models;

namespace SalaryPredictor.Bot;

public static class Predictor
{
    private const string ModelPath = "SalaryPredictionModel.zip";
    private static readonly MLContext MlContext = new();
    private static readonly PredictionEngine<SalaryData,SalaryPrediction> PredictionEngine;

    static Predictor()
    {
        var model = MlContext.Model.Load(ModelPath, out var modelInputSchema);
        PredictionEngine = MlContext.Model.CreatePredictionEngine<SalaryData, SalaryPrediction>(model);
    }

    public static async Task<SalaryPrediction> PredictAsync(SalaryData salaryData)
    {
        return await Task.Run(() => PredictionEngine.Predict(salaryData));
    }
}