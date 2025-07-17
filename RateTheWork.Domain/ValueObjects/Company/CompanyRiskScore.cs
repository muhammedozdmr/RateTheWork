namespace RateTheWork.Domain.ValueObjects.Company;

/// <summary>
/// Şirket risk skorunu temsil eden value object
/// </summary>
public sealed class CompanyRiskScore
{
    private CompanyRiskScore
    (
        double totalScore
        , string riskLevel
        , Dictionary<string, double> riskFactors
        , int validityDays = 30
    )
    {
        TotalScore = Math.Round(totalScore, 2);
        RiskLevel = riskLevel;
        RiskFactors = riskFactors ?? new Dictionary<string, double>();
        AssessmentDate = DateTime.UtcNow;
        ValidityDays = validityDays;

        // En yüksek risk faktörünü bul
        PrimaryRiskFactor = RiskFactors.Any()
            ? RiskFactors.OrderByDescending(x => x.Value).First().Key
            : "None";

        // Risk açıklaması oluştur
        RiskDescription = GenerateRiskDescription();
    }

    /// <summary>
    /// Toplam risk skoru (0-100 arası)
    /// </summary>
    public double TotalScore { get; }

    /// <summary>
    /// Risk seviyesi (Minimal, Low, Medium, High)
    /// </summary>
    public string RiskLevel { get; }

    /// <summary>
    /// Risk faktörleri ve skorları
    /// </summary>
    public Dictionary<string, double> RiskFactors { get; }

    /// <summary>
    /// Risk değerlendirme tarihi
    /// </summary>
    public DateTime AssessmentDate { get; }

    /// <summary>
    /// Risk skoru geçerlilik süresi (gün)
    /// </summary>
    public int ValidityDays { get; }

    /// <summary>
    /// En yüksek risk faktörü
    /// </summary>
    public string PrimaryRiskFactor { get; }

    /// <summary>
    /// Risk açıklaması
    /// </summary>
    public string RiskDescription { get; }

    /// <summary>
    /// Yeni risk skoru oluşturur
    /// </summary>
    public static CompanyRiskScore Create
    (
        double totalScore
        , string riskLevel
        , Dictionary<string, double> riskFactors
    )
    {
        // Validasyonlar
        if (totalScore < 0 || totalScore > 100)
            throw new ArgumentOutOfRangeException(nameof(totalScore), "Risk skoru 0-100 arasında olmalıdır.");

        if (string.IsNullOrWhiteSpace(riskLevel))
            throw new ArgumentNullException(nameof(riskLevel));

        var validRiskLevels = new[] { "Minimal", "Low", "Medium", "High", "Critical" };
        if (!validRiskLevels.Contains(riskLevel))
            throw new ArgumentException(
                $"Geçersiz risk seviyesi. Geçerli değerler: {string.Join(", ", validRiskLevels)}");

        if (riskFactors != null)
        {
            foreach (var factor in riskFactors)
            {
                if (factor.Value < 0)
                    throw new ArgumentException($"Risk faktör değeri negatif olamaz: {factor.Key}");
            }
        }

        return new CompanyRiskScore(totalScore, riskLevel, riskFactors);
    }

    /// <summary>
    /// Detaylı risk skoru oluşturur (özel geçerlilik süresiyle)
    /// </summary>
    public static CompanyRiskScore CreateDetailed
    (
        double totalScore
        , string riskLevel
        , Dictionary<string, double> riskFactors
        , int validityDays
    )
    {
        if (validityDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(validityDays), "Geçerlilik süresi pozitif olmalıdır.");

        var riskScore = Create(totalScore, riskLevel, riskFactors);

        return new CompanyRiskScore(
            riskScore.TotalScore,
            riskScore.RiskLevel,
            riskScore.RiskFactors,
            validityDays
        );
    }

    /// <summary>
    /// Minimal risk skoru oluşturur
    /// </summary>
    public static CompanyRiskScore CreateMinimal()
    {
        return new CompanyRiskScore(
            totalScore: 0,
            riskLevel: "Minimal",
            riskFactors: new Dictionary<string, double>()
        );
    }

    /// <summary>
    /// Yüksek riskli şirket için skor oluşturur
    /// </summary>
    public static CompanyRiskScore CreateHighRisk(Dictionary<string, double> riskFactors)
    {
        var totalScore = riskFactors.Values.Sum();

        return new CompanyRiskScore(
            totalScore: Math.Min(totalScore, 100), // 100'ü geçemez
            riskLevel: totalScore >= 70 ? "Critical" : "High",
            riskFactors: riskFactors
        );
    }

    /// <summary>
    /// Risk skorunun hala geçerli olup olmadığını kontrol eder
    /// </summary>
    public bool IsValid()
    {
        return (DateTime.UtcNow - AssessmentDate).TotalDays <= ValidityDays;
    }

    /// <summary>
    /// Belirli bir risk faktörünün değerini döndürür
    /// </summary>
    public double GetFactorScore(string factorName)
    {
        return RiskFactors.TryGetValue(factorName, out var score) ? score : 0;
    }

    /// <summary>
    /// Risk faktörlerini önem sırasına göre döndürür
    /// </summary>
    public IEnumerable<KeyValuePair<string, double>> GetFactorsByImportance()
    {
        return RiskFactors.OrderByDescending(x => x.Value);
    }

    private string GenerateRiskDescription()
    {
        return RiskLevel switch
        {
            "Minimal" => "Şirket minimal risk taşımaktadır."
            , "Low" => "Şirket düşük risk seviyesindedir. Normal operasyon."
            , "Medium" => "Şirket orta düzeyde risk taşımaktadır. Takip önerilir."
            , "High" => "Şirket yüksek risk seviyesindedir. Yakın takip gerekir."
            , "Critical" => "Şirket kritik risk seviyesindedir. Acil müdahale gerekebilir."
            , _ => "Risk seviyesi belirlenemedi."
        };
    }

    /// <summary>
    /// Risk skorunu yüzde olarak döndürür
    /// </summary>
    public string GetScoreAsPercentage()
    {
        return $"{TotalScore:F1}%";
    }

    /// <summary>
    /// Risk seviyesinin rengini döndürür (UI için)
    /// </summary>
    public string GetRiskColor()
    {
        return RiskLevel switch
        {
            "Minimal" => "#28a745", // Yeşil
            "Low" => "#5cb85c"
            , // Açık yeşil
            "Medium" => "#ffc107"
            , // Sarı
            "High" => "#fd7e14"
            , // Turuncu
            "Critical" => "#dc3545"
            , // Kırmızı
            _ => "#6c757d" // Gri
        };
    }

    // Value Object equality
    public override bool Equals(object? obj)
    {
        if (obj is not CompanyRiskScore other)
            return false;

        return TotalScore == other.TotalScore &&
               RiskLevel == other.RiskLevel &&
               ValidityDays == other.ValidityDays &&
               RiskFactors.SequenceEqual(other.RiskFactors);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + TotalScore.GetHashCode();
            hash = hash * 23 + RiskLevel.GetHashCode();
            hash = hash * 23 + ValidityDays.GetHashCode();

            foreach (var factor in RiskFactors)
            {
                hash = hash * 23 + factor.Key.GetHashCode();
                hash = hash * 23 + factor.Value.GetHashCode();
            }

            return hash;
        }
    }
}
