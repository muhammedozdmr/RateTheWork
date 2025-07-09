namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket risk skoru
/// </summary>
public class CompanyRiskScore
{
    public decimal OverallRisk { get; set; }
    public decimal FinancialRisk { get; set; }
    public decimal ReputationalRisk { get; set; }
    public decimal ComplianceRisk { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
}
