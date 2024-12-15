using Microsoft.ML.Data;

namespace SalaryPredictor.Training.Models;

public class SalaryData
{
    [LoadColumn(0)]
    public float WorkYear { get; set; }

    [LoadColumn(1)]
    public string ExperienceLevel { get; set; }

    [LoadColumn(2)]
    public string EmploymentType { get; set; }

    [LoadColumn(3)]
    public string JobTitle { get; set; }

    [LoadColumn(4)]
    public float Salary { get; set; }

    [LoadColumn(5)]
    public string SalaryCurrency { get; set; }

    [LoadColumn(6)]
    public float SalaryInUSD { get; set; }

    [LoadColumn(7)]
    public string EmployeeResidence { get; set; }

    [LoadColumn(8)]
    public float RemoteRatio { get; set; }

    [LoadColumn(9)]
    public string CompanyLocation { get; set; }

    [LoadColumn(10)]
    public string CompanySize { get; set; }
}