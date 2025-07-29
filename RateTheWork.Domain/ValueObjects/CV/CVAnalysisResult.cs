namespace RateTheWork.Domain.ValueObjects.CV;

/// <summary>
/// CV analiz sonucu value object
/// </summary>
public class CVAnalysisResult
{
    public CVAnalysisResult(
        decimal overallScore,
        Dictionary<string, decimal> skillScores,
        List<string> strengths,
        List<string> weaknesses,
        List<string> recommendations,
        Dictionary<string, decimal> departmentMatchScores,
        string summary)
    {
        OverallScore = overallScore;
        SkillScores = skillScores;
        Strengths = strengths;
        Weaknesses = weaknesses;
        Recommendations = recommendations;
        DepartmentMatchScores = departmentMatchScores;
        Summary = summary;
        AnalyzedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Genel CV puanı (0-100)
    /// </summary>
    public decimal OverallScore { get; }
    
    /// <summary>
    /// Beceri bazlı puanlar
    /// </summary>
    public Dictionary<string, decimal> SkillScores { get; }
    
    /// <summary>
    /// Güçlü yönler
    /// </summary>
    public List<string> Strengths { get; }
    
    /// <summary>
    /// Geliştirilmesi gereken yönler
    /// </summary>
    public List<string> Weaknesses { get; }
    
    /// <summary>
    /// Öneriler
    /// </summary>
    public List<string> Recommendations { get; }
    
    /// <summary>
    /// Departman uyumluluk puanları
    /// </summary>
    public Dictionary<string, decimal> DepartmentMatchScores { get; }
    
    /// <summary>
    /// Analiz özeti
    /// </summary>
    public string Summary { get; }
    
    /// <summary>
    /// Analiz zamanı
    /// </summary>
    public DateTime AnalyzedAt { get; }
}

/// <summary>
/// Pozisyon uyumluluk sonucu
/// </summary>
public class PositionMatchResult
{
    public PositionMatchResult(
        decimal matchScore,
        List<string> matchingSkills,
        List<string> missingSkills,
        List<string> additionalSkills,
        string recommendation,
        Dictionary<string, decimal> requirementScores)
    {
        MatchScore = matchScore;
        MatchingSkills = matchingSkills;
        MissingSkills = missingSkills;
        AdditionalSkills = additionalSkills;
        Recommendation = recommendation;
        RequirementScores = requirementScores;
    }

    /// <summary>
    /// Uyumluluk puanı (0-100)
    /// </summary>
    public decimal MatchScore { get; }
    
    /// <summary>
    /// Eşleşen beceriler
    /// </summary>
    public List<string> MatchingSkills { get; }
    
    /// <summary>
    /// Eksik beceriler
    /// </summary>
    public List<string> MissingSkills { get; }
    
    /// <summary>
    /// İlave beceriler (pozisyonda istenmeyen ama adayda olan)
    /// </summary>
    public List<string> AdditionalSkills { get; }
    
    /// <summary>
    /// Öneri
    /// </summary>
    public string Recommendation { get; }
    
    /// <summary>
    /// Gereksinim bazlı puanlar
    /// </summary>
    public Dictionary<string, decimal> RequirementScores { get; }
}

/// <summary>
/// CV'den çıkarılan temel bilgiler
/// </summary>
public class CVBasicInfo
{
    public CVBasicInfo(
        string? fullName,
        string? email,
        string? phone,
        string? location,
        List<string> skills,
        List<WorkExperience> experiences,
        List<Education> educations,
        List<string> languages,
        string? summary)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
        Location = location;
        Skills = skills;
        Experiences = experiences;
        Educations = educations;
        Languages = languages;
        Summary = summary;
    }

    public string? FullName { get; }
    public string? Email { get; }
    public string? Phone { get; }
    public string? Location { get; }
    public List<string> Skills { get; }
    public List<WorkExperience> Experiences { get; }
    public List<Education> Educations { get; }
    public List<string> Languages { get; }
    public string? Summary { get; }
}

/// <summary>
/// İş deneyimi
/// </summary>
public class WorkExperience
{
    public WorkExperience(
        string companyName,
        string position,
        DateTime? startDate,
        DateTime? endDate,
        string? description)
    {
        CompanyName = companyName;
        Position = position;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
    }

    public string CompanyName { get; }
    public string Position { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }
    public string? Description { get; }
    
    public bool IsCurrent => !EndDate.HasValue;
    public int? DurationInMonths => StartDate.HasValue && EndDate.HasValue 
        ? (int)((EndDate.Value - StartDate.Value).TotalDays / 30)
        : null;
}

/// <summary>
/// Eğitim bilgisi
/// </summary>
public class Education
{
    public Education(
        string institution,
        string degree,
        string? field,
        DateTime? startDate,
        DateTime? endDate,
        decimal? gpa)
    {
        Institution = institution;
        Degree = degree;
        Field = field;
        StartDate = startDate;
        EndDate = endDate;
        GPA = gpa;
    }

    public string Institution { get; }
    public string Degree { get; }
    public string? Field { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }
    public decimal? GPA { get; }
}