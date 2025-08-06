using Microsoft.Extensions.Logging;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects.Moderation;
using RateTheWork.Domain.ValueObjects.Review;

namespace RateTheWork.Infrastructure.Services;

public class ContentModerationService : IContentModerationService
{
    private readonly ILogger<ContentModerationService> _logger;
    private readonly List<string> _bannedWords;

    public ContentModerationService(ILogger<ContentModerationService> logger)
    {
        _logger = logger;
        _bannedWords = new List<string>
        {
            // Örnek yasaklı kelimeler - gerçek projede yapılandırmadan okunmalı
            "spam", "abuse", "hate", "küfür", "hakaret"
        };
    }

    public async Task<ModerationResult> ModerateContentAsync(string content, string language = "tr")
    {
        var flaggedWords = FilterProhibitedWords(content);
        var score = CalculateContentScore(content);
        var categoryScores = new Dictionary<string, double>
        {
            ["toxicity"] = flaggedWords.Any() ? 0.8 : 0.1,
            ["spam"] = await IsSpamPatternAsync(content) ? 0.9 : 0.1,
            ["profanity"] = flaggedWords.Any(w => _bannedWords.Contains(w)) ? 0.9 : 0.1
        };

        var details = ModerationDetails.CreateAutomatic(
            new List<string> { "Content analysis", "Keyword filter", "Spam detection" },
            "AI"
        );
        
        // Set additional details
        details.ProfanityScore = categoryScores["profanity"];
        details.SpamScore = categoryScores["spam"];
        details.CategoryScores = categoryScores;

        var isApproved = !flaggedWords.Any() && !await IsSpamPatternAsync(content);
        var reason = isApproved ? "Content approved" : "Content contains prohibited content";

        var result = new ModerationResult(
            isApproved: isApproved,
            reason: reason,
            flaggedWords: flaggedWords,
            confidenceScore: score / 100.0,
            categoryScores: categoryScores,
            details: details,
            suggestedCorrections: isApproved ? null : new List<string> { "Remove prohibited words", "Improve content quality" },
            moderationType: "Automatic"
        );

        return result;
    }

    public async Task<string> TranslateContentAsync(string content, string fromLanguage, string toLanguage)
    {
        // Basit çeviri simülasyonu - gerçek projede çeviri API'si kullanılmalı
        _logger.LogInformation("Translating content from {FromLang} to {ToLang}", fromLanguage, toLanguage);
        await Task.Delay(10);
        return content; // Şimdilik aynı içeriği döndür
    }

    public async Task<string> SummarizeContentAsync(string content, int maxLength = 200)
    {
        await Task.Delay(10);
        if (string.IsNullOrWhiteSpace(content))
            return content;

        if (content.Length <= maxLength)
            return content;

        return content.Substring(0, maxLength) + "...";
    }

    public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string content)
    {
        await Task.Delay(10);
        var lowerContent = content.ToLowerInvariant();
        
        double positiveScore = 0.2;
        double negativeScore = 0.2;
        double neutralScore = 0.6;
        
        // Pozitif kelimeler
        if (lowerContent.Contains("excellent") || lowerContent.Contains("great") || 
            lowerContent.Contains("harika") || lowerContent.Contains("mükemmel"))
        {
            positiveScore = 0.8;
            negativeScore = 0.1;
            neutralScore = 0.1;
        }
        // Negatif kelimeler
        else if (lowerContent.Contains("terrible") || lowerContent.Contains("awful") || 
                 lowerContent.Contains("kötü") || lowerContent.Contains("berbat"))
        {
            positiveScore = 0.1;
            negativeScore = 0.8;
            neutralScore = 0.1;
        }

        var emotionScores = new Dictionary<string, double>
        {
            ["joy"] = positiveScore * 0.5,
            ["anger"] = negativeScore * 0.3,
            ["sadness"] = negativeScore * 0.4,
            ["surprise"] = 0.1,
            ["fear"] = negativeScore * 0.3
        };

        return SentimentAnalysisResult.Create(
            positiveScore: positiveScore,
            negativeScore: negativeScore,
            neutralScore: neutralScore,
            emotionScores: emotionScores
        );
    }

    public async Task<List<string>> ExtractKeywordsAsync(string content, int maxKeywords = 10)
    {
        await Task.Delay(10);
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .GroupBy(w => w.ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .Take(maxKeywords)
            .Select(g => g.Key)
            .ToList();

        return words;
    }

    public async Task<List<ContentCategory>> CategorizeContentAsync(string content)
    {
        await Task.Delay(10);
        var categories = new List<ContentCategory>();
        var lowerContent = content.ToLowerInvariant();
        
        if (lowerContent.Contains("salary") || lowerContent.Contains("maaş") || lowerContent.Contains("ücret"))
        {
            categories.Add(new ContentCategory 
            { 
                CategoryName = "Compensation", 
                ConfidenceScore = 0.8,
                SubCategories = new List<string> { "salary", "benefits" }
            });
        }
        
        if (lowerContent.Contains("environment") || lowerContent.Contains("culture") || 
            lowerContent.Contains("ortam") || lowerContent.Contains("kültür"))
        {
            categories.Add(new ContentCategory 
            { 
                CategoryName = "WorkEnvironment",
                ConfidenceScore = 0.8,
                SubCategories = new List<string> { "culture", "atmosphere" }
            });
        }
        
        if (lowerContent.Contains("management") || lowerContent.Contains("yönetim") || lowerContent.Contains("müdür"))
        {
            categories.Add(new ContentCategory 
            { 
                CategoryName = "Management",
                ConfidenceScore = 0.8,
                SubCategories = new List<string> { "leadership", "supervision" }
            });
        }

        if (!categories.Any())
        {
            categories.Add(new ContentCategory 
            { 
                CategoryName = "General",
                ConfidenceScore = 0.5,
                SubCategories = new List<string>()
            });
        }
        
        return categories;
    }

    public async Task<ModerationResult> ModerateReviewAsync(string commentText, string commentType)
    {
        var result = await ModerateContentAsync(commentText);
        
        // Yorum tipi özel kontrolleri
        if (commentType == "WorkEnvironment" && commentText.Length < 20)
        {
            var details = ModerationDetails.CreateAutomatic(
                new List<string> { "Content length check" },
                "Validation"
            );

            result = new ModerationResult(
                isApproved: false,
                reason: "Review too short for work environment feedback",
                flaggedWords: result.FlaggedWords,
                confidenceScore: 1.0,
                categoryScores: result.CategoryScores,
                details: details,
                moderationType: "Automatic"
            );
        }
        
        return result;
    }

    public async Task<ModerationResult> ModerateCompanyInfoAsync(string companyName, string? description = null)
    {
        var contentToModerate = companyName;
        if (!string.IsNullOrEmpty(description))
            contentToModerate += " " + description;
            
        var result = await ModerateContentAsync(contentToModerate);
        
        // Şirket bilgisi özel kontrolleri
        if (companyName.Length < 2)
        {
            var details = ModerationDetails.CreateAutomatic(
                new List<string> { "Company name validation" },
                "Validation"
            );

            result = new ModerationResult(
                isApproved: false,
                reason: "Company name too short",
                flaggedWords: new List<string>(),
                confidenceScore: 1.0,
                categoryScores: new Dictionary<string, double>(),
                details: details,
                moderationType: "Automatic"
            );
        }
        
        return result;
    }

    public async Task<ModerationResult> ModerateUsernameAsync(string username)
    {
        var result = await ModerateContentAsync(username);
        
        // Kullanıcı adı özel kontrolleri
        if (username.Length < 3 || username.Length > 20)
        {
            var details = ModerationDetails.CreateAutomatic(
                new List<string> { "Username length validation" },
                "Validation"
            );

            result = new ModerationResult(
                isApproved: false,
                reason: "Username must be between 3 and 20 characters",
                flaggedWords: new List<string>(),
                confidenceScore: 1.0,
                categoryScores: new Dictionary<string, double>(),
                details: details,
                moderationType: "Automatic"
            );
        }
        
        return result;
    }

    public async Task<List<ModerationResult>> ModerateBulkAsync(List<string> contents)
    {
        var results = new List<ModerationResult>();
        
        foreach (var content in contents)
        {
            results.Add(await ModerateContentAsync(content));
        }
        
        return results;
    }

    public async Task<string> SanitizeContentAsync(string content)
    {
        await Task.Delay(10);
        if (string.IsNullOrWhiteSpace(content))
            return content;

        var sanitized = content;
        
        foreach (var word in _bannedWords)
        {
            var replacement = new string('*', word.Length);
            sanitized = sanitized.Replace(word, replacement, StringComparison.OrdinalIgnoreCase);
        }

        return sanitized;
    }

    public async Task<bool> IsSpamAsync(string content, string userId)
    {
        await Task.Delay(10);
        // Basit spam kontrolü
        var lowerContent = content.ToLowerInvariant();
        
        // Aynı kelime çok fazla tekrar ediyorsa spam olabilir
        var words = lowerContent.Split(' ');
        var uniqueWords = words.Distinct().Count();
        
        if (words.Length > 10 && uniqueWords < words.Length / 3)
            return true;
        
        // URL çok fazla varsa spam olabilir
        if (content.Count(c => c == '@') > 3 || content.Contains("http") && content.Split("http").Length > 3)
            return true;
        
        return false;
    }

    public List<string> FilterProhibitedWords(string content)
    {
        var foundWords = new List<string>();
        var lowerContent = content.ToLowerInvariant();
        
        foreach (var word in _bannedWords)
        {
            if (lowerContent.Contains(word))
            {
                foundWords.Add(word);
            }
        }
        
        return foundWords;
    }

    public double CalculateContentScore(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        var score = 50.0;
        
        // Uzunluk bonusu
        if (content.Length > 100) score += 10;
        if (content.Length > 500) score += 10;
        
        // Yasaklı kelime cezası
        foreach (var word in _bannedWords)
        {
            if (content.Contains(word, StringComparison.OrdinalIgnoreCase))
                score -= 20;
        }
        
        return Math.Max(0, Math.Min(100, score));
    }

    public async Task<bool> IsSpamPatternAsync(string content)
    {
        await Task.Delay(10);
        // Spam pattern kontrolü
        var patterns = new[] { "!!!!", "$$$$", "####", "****" };
        
        foreach (var pattern in patterns)
        {
            if (content.Contains(pattern))
                return true;
        }
        
        return false;
    }

    public async Task<ModerationResult> ModerateTextAsync(string text, string language = "tr")
    {
        return await ModerateContentAsync(text, language);
    }
}