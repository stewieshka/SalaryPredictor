using Microsoft.ML.Data;

namespace SalaryPredictor.Training.Models;

public class SalaryPrediction
{
    [ColumnName("Score")]
    public float PredictedSalaryInUSD { get; set; }
}