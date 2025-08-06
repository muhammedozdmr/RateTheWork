using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.ValueObjects.CV;

namespace RateTheWork.Infrastructure.Services;

public class AICVAnalysisService : IAICVAnalysisService
{
    private readonly ILogger<AICVAnalysisService> _logger;

    public AICVAnalysisService(ILogger<AICVAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<CVAnalysisResult> AnalyzeCV(string cvFileUrl, List<string> targetDepartments)
    {
        // Simüle edilmiş CV analizi
        // Gerçek projede AI/ML servisi ve dosya okuma işlemi olacak
        
        _logger.LogInformation("Analyzing CV from {Url} for departments: {Departments}", 
            cvFileUrl, string.Join(", ", targetDepartments));

        await Task.Delay(100); // Simüle edilmiş işlem süresi

        var overallScore = (decimal)CalculateRandomScore() * 100;
        var skillScores = ExtractRandomSkills()
            .ToDictionary(skill => skill, skill => (decimal)(new Random().NextDouble() * 100));
        var departmentMatchScores = CalculateDepartmentScores(targetDepartments)
            .ToDictionary(kvp => kvp.Key, kvp => (decimal)(kvp.Value * 100));

        var result = new CVAnalysisResult(
            overallScore: overallScore,
            skillScores: skillScores,
            strengths: new List<string> { "Technical skills", "Team player", "Problem solving" },
            weaknesses: new List<string> { "Limited industry experience" },
            recommendations: GenerateRecommendations(),
            departmentMatchScores: departmentMatchScores,
            summary: "Experienced professional with strong technical skills and team collaboration abilities"
        );

        return result;
    }

    public async Task<PositionMatchResult> AnalyzePositionMatch(string cvFileUrl, string jobDescription)
    {
        _logger.LogInformation("Analyzing position match for CV: {Url}", cvFileUrl);

        await Task.Delay(100); // Simüle edilmiş işlem süresi

        var matchScore = (decimal)CalculateRandomScore() * 100;
        var matchingSkills = new List<string> { "C#", ".NET", "SQL" };
        var missingSkills = new List<string> { "React", "Docker" };
        var additionalSkills = new List<string> { "Python", "AWS" };
        var requirementScores = new Dictionary<string, decimal>
        {
            ["Technical Skills"] = 85,
            ["Experience"] = 75,
            ["Education"] = 90,
            ["Soft Skills"] = 80
        };

        var result = new PositionMatchResult(
            matchScore: matchScore,
            matchingSkills: matchingSkills,
            missingSkills: missingSkills,
            additionalSkills: additionalSkills,
            recommendation: "Strong candidate - recommend for interview",
            requirementScores: requirementScores
        );

        return result;
    }

    public async Task<CVBasicInfo> ExtractBasicInfo(string cvFileUrl)
    {
        _logger.LogInformation("Extracting basic info from CV: {Url}", cvFileUrl);

        await Task.Delay(50); // Simüle edilmiş işlem süresi

        var experiences = new List<WorkExperience>
        {
            new WorkExperience(
                companyName: "Tech Corp",
                position: "Senior Software Developer",
                startDate: DateTime.UtcNow.AddYears(-2),
                endDate: null,
                description: "Leading development of microservices architecture"
            ),
            new WorkExperience(
                companyName: "Software Inc",
                position: "Software Developer",
                startDate: DateTime.UtcNow.AddYears(-5),
                endDate: DateTime.UtcNow.AddYears(-2),
                description: "Full-stack development with .NET and React"
            )
        };

        var educations = new List<Education>
        {
            new Education(
                institution: "Technical University",
                degree: "Bachelor's Degree",
                field: "Computer Science",
                startDate: DateTime.UtcNow.AddYears(-9),
                endDate: DateTime.UtcNow.AddYears(-5),
                gpa: 3.5m
            )
        };

        var result = new CVBasicInfo(
            fullName: "John Doe",
            email: "john.doe@example.com",
            phone: "+90 555 123 4567",
            location: "Istanbul, Turkey",
            skills: ExtractRandomSkills(),
            experiences: experiences,
            educations: educations,
            languages: new List<string> { "Turkish (Native)", "English (Professional)" },
            summary: "Experienced software developer with 5+ years in .NET development"
        );

        return result;
    }

    private double CalculateRandomScore()
    {
        // Simüle edilmiş skor hesaplama
        var random = new Random();
        return Math.Round(0.5 + (random.NextDouble() * 0.5), 2); // 0.50 - 1.00 arası
    }

    private Dictionary<string, double> CalculateDepartmentScores(List<string> departments)
    {
        var scores = new Dictionary<string, double>();
        var random = new Random();

        foreach (var dept in departments)
        {
            scores[dept] = Math.Round(0.4 + (random.NextDouble() * 0.6), 2); // 0.40 - 1.00 arası
        }

        return scores;
    }

    private List<string> ExtractRandomSkills()
    {
        var allSkills = new[] 
        { 
            "C#", ".NET", "JavaScript", "SQL", "Azure", "AWS", 
            "Docker", "Kubernetes", "React", "Angular", "Python", "Java" 
        };
        
        var random = new Random();
        var count = random.Next(3, 8);
        
        return allSkills.OrderBy(x => random.Next()).Take(count).ToList();
    }

    private List<string> GenerateRecommendations()
    {
        return new List<string>
        {
            "Consider adding more specific project achievements",
            "Include quantifiable results in experience section",
            "Add more relevant certifications for target position"
        };
    }
}