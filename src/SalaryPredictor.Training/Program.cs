using Microsoft.ML;
using SalaryPredictor.Shared.Models;

var mlContext = new MLContext();

const string dataPath = "ds_salaries.csv";
var data = mlContext.Data.LoadFromTextFile<SalaryData>(dataPath, hasHeader: true, separatorChar: ',');

var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
var trainData = split.TrainSet;
var testData = split.TestSet;

var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("ExperienceLevelEncoded", "ExperienceLevel")
    .Append(mlContext.Transforms.Categorical.OneHotEncoding("EmploymentTypeEncoded", "EmploymentType"))
    .Append(mlContext.Transforms.Categorical.OneHotEncoding("JobTitleEncoded", "JobTitle"))
    .Append(mlContext.Transforms.Categorical.OneHotEncoding("EmployeeResidenceEncoded", "EmployeeResidence"))
    .Append(mlContext.Transforms.Categorical.OneHotEncoding("CompanyLocationEncoded", "CompanyLocation"))
    .Append(mlContext.Transforms.Categorical.OneHotEncoding("CompanySizeEncoded", "CompanySize"))
    .Append(mlContext.Transforms.NormalizeMinMax("WorkYearNormalized", "WorkYear"))
    .Append(mlContext.Transforms.NormalizeMinMax("RemoteRatioNormalized", "RemoteRatio"))
    .Append(mlContext.Transforms.Concatenate("Features",
        "ExperienceLevelEncoded",
        "EmploymentTypeEncoded",
        "JobTitleEncoded",
        "EmployeeResidenceEncoded",
        "CompanyLocationEncoded",
        "CompanySizeEncoded",
        "WorkYearNormalized",
        "RemoteRatioNormalized"))
    .Append(mlContext.Regression.Trainers.FastTree(
        labelColumnName: "SalaryInUSD",
        featureColumnName: "Features",
        numberOfLeaves: 50,
        minimumExampleCountPerLeaf: 5,
        learningRate: 0.2));

var model = pipeline.Fit(trainData);

var predictions = model.Transform(testData);
var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "SalaryInUSD");

Console.WriteLine($"R^2: {metrics.RSquared:0.##}");
Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError:0.##}");

const string modelPath = "SalaryPredictionModel.zip";
mlContext.Model.Save(model, trainData.Schema, modelPath);
Console.WriteLine($"Model saved to: {modelPath}");