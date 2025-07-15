using System.Text.RegularExpressions;
using RateTheWork.Domain.Interfaces.Security;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects;
using ModerationDetails = RateTheWork.Domain.Interfaces.Services.ModerationDetails;

namespace RateTheWork.Domain.Services;

/// <summary>
/// İçerik moderasyon servisi - Domain katmanı implementasyonu
/// Not: Gerçek AI entegrasyonu Infrastructure katmanında olacak
/// </summary>
public class ContentModerationService : IContentModerationService
{
    private readonly IAnonymizationService _anonymizationService;
    
    // Yasaklı kelimeler listesi (örnek)
    private readonly HashSet<string> _prohibitedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Gerçek uygulamada bunlar configuration'dan gelecek
        // Küfür ve hakaret içeren kelimeler
        "küfür1", "küfür2", "hakaret1",
        
        //Spam kelimeler
        "spam", "reklam", "link", "http", "www", ".com", "kazanç", "tıkla", "bedava", "ücretsiz para"
    };

    // Kişisel bilgi pattern'leri - Şüpheli pattern'ler
    private readonly List<Regex> _personalInfoPatterns = new()
    {
        new Regex(@"\b\d{11}\b", RegexOptions.Compiled), // TC Kimlik
        new Regex(@"\b\d{10}\b", RegexOptions.Compiled), // Vergi No
        new Regex(@"\b0?\d{3}[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}\b", RegexOptions.Compiled), // Telefon
        new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled), // Email
        new Regex(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", RegexOptions.Compiled), // Kredi Kartı
    };

    public ContentModerationService(IAnonymizationService anonymizationService)
    {
        _anonymizationService = anonymizationService;
    }

    public async Task<ModerationResult> ModerateContentAsync(string content, string language = "tr")
    {
        var result = new ModerationResult
        {
            IsApproved = true,
            Details = new ValueObjects.ModerationDetails()
        };

        // Boş içerik kontrolü
        if (string.IsNullOrWhiteSpace(content))
        {
            result.IsApproved = false;
            result.Reason = "İçerik boş olamaz";
            result.FlaggedWords.Add("empty_content");
            return result;
        }

        // Uzunluk kontrolü
        if (content.Length < 50)
        {
            result.IsApproved = false;
            result.Reason = "Yorum en az 50 karakter olmalıdır";
            result.FlaggedWords.Add("too_short");
            result.SuggestedCorrections.Add("Lütfen yorumunuzu daha detaylı yazın");
            return result;
        }

        if (content.Length > 5000)
        {
            result.IsApproved = false;
            result.Reason = "Yorum en fazla 5000 karakter olabilir";
            result.FlaggedWords.Add("too_long");
            //TODO: Add yapamıyorum halbuki set private değil !!
            result.SuggestedCorrections.Add("Lütfen yorumunuzu kısaltın");
            return result;
        }

        // Spam kontrolü
        var spamScore = await CalculateSpamScoreAsync(content);
        result.Details.SpamScore = spamScore;
        if (spamScore > 0.7)
        {
            result.IsApproved = false;
            result.Reason = "İçerik spam olarak tespit edildi";
            result.FlaggedWords.Add("spam_detected");
        }

        // Kişisel bilgi kontrolü
        var personalInfoScore = CheckPersonalInfo(content, result.Details);
        result.Details.ConfidentialInfoScore = personalInfoScore;
        if (personalInfoScore > 0.5)
        {
            result.IsApproved = false;
            result.Reason = "İçerikte kişisel bilgiler tespit edildi";
            result.FlaggedWords.Add("personal_info_detected");
            result.SuggestedCorrections.Add("Lütfen kişisel bilgileri kaldırın (TC Kimlik, telefon, email vb.)");
        }

        // Yasaklı kelime kontrolü
        var profanityScore = CheckProhibitedWords(content);
        result.Details.ProfanityScore = profanityScore;
        if (profanityScore > 0.3)
        {
            result.IsApproved = false;
            result.Reason = "İçerikte uygunsuz ifadeler tespit edildi";
            result.FlaggedWords.Add("prohibited_words");
        }

        // Tekrarlayan karakter kontrolü
        if (HasExcessiveRepetition(content))
        {
            result.Details.SpamScore = Math.Max(result.Details.SpamScore, 0.8);
            result.IsApproved = false;
            result.Reason = "İçerikte aşırı tekrar tespit edildi";
            result.FlaggedWords.Add("excessive_repetition");
            result.SuggestedCorrections.Add("Lütfen gereksiz tekrarları kaldırın");
        }

        // Büyük harf kullanımı kontrolü
        var capsRatio = CalculateCapsRatio(content);
        if (capsRatio > 0.5)
        {
            result.Details.PersonalAttackScore = 0.5;
            result.FlaggedWords.Add("excessive_caps");
            result.Details.SuggestedActions.Add("Lütfen büyük harf kullanımını azaltın");
            if (capsRatio > 0.8)
            {
                result.IsApproved = false;
                result.Reason = "Aşırı büyük harf kullanımı tespit edildi";
            }
        }

        // Saldırgan içerik kontrolü (basit versiyon)
        var attackScore = CheckPersonalAttacks(content);
        result.Details.PersonalAttackScore = Math.Max(result.Details.PersonalAttackScore, attackScore);
        if (attackScore > 0.5)
        {
            result.IsApproved = false;
            result.Reason = "İçerikte saldırgan ifadeler tespit edildi";
            result.FlaggedWords.Add("personal_attack");
            result.SuggestedCorrections.Add("Lütfen yapıcı bir dil kullanın");
        }

        // Toxicity hesaplama
        result.ToxicityScore = CalculateToxicityScore(result.Details);

        // Önerilen aksiyonlar
        if (!result.IsApproved)
        {
            result.Details.SuggestedActions.Add("İçeriği gözden geçirin ve kurallara uygun şekilde düzenleyin");
        }

        // Asenkron işlem simülasyonu
        await Task.Delay(10);
        
        return result;
    }

    public async Task<string> TranslateContentAsync(string content, string fromLanguage, string toLanguage)
    {
        // Bu metod gerçek uygulamada Infrastructure katmanında
        // Google Translate veya benzeri bir servisle entegre olacak
        
        // Şimdilik basit bir simülasyon
        await Task.Delay(50);
        
        // Gerçek çeviri yerine içeriği döndür
        return content;
    }

    public async Task<string> SummarizeContentAsync(string content, int maxLength = 200)
    {
        // AI özet servisi simülasyonu
        await Task.Delay(20);
        
        if (content.Length <= maxLength)
            return content;
        
        // Basit özet - ilk cümleleri al
        var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var summary = "";
        
        foreach (var sentence in sentences)
        {
            if (summary.Length + sentence.Length > maxLength)
                break;
            summary += sentence.Trim() + ". ";
        }
        
        return summary.Trim();
    }

    public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string content)
    {
        // Sentiment analiz simülasyonu
        await Task.Delay(30);
        
        var result = new SentimentAnalysisResult();
        
        // Basit pozitif/negatif kelime analizi
        var positiveWords = new[] { "güzel", "harika", "mükemmel", "iyi", "başarılı", "memnun", "mutlu" };
        var negativeWords = new[] { "kötü", "berbat", "rezalet", "yetersiz", "başarısız", "mutsuz", "sorunlu" };
        
        var lowerContent = content.ToLowerInvariant();
        var positiveCount = positiveWords.Count(word => lowerContent.Contains(word));
        var negativeCount = negativeWords.Count(word => lowerContent.Contains(word));
        
        var total = positiveCount + negativeCount;
        if (total == 0)
        {
            result.NeutralScore = 1.0;
            result.DominantSentiment = "Neutral";
        }
        else
        {
            result.PositiveScore = (double)positiveCount / total;
            result.NegativeScore = (double)negativeCount / total;
            result.NeutralScore = 1.0 - result.PositiveScore - result.NegativeScore;
            
            if (result.PositiveScore > result.NegativeScore)
                result.DominantSentiment = "Positive";
            else if (result.NegativeScore > result.PositiveScore)
                result.DominantSentiment = "Negative";
            else
                result.DominantSentiment = "Neutral";
        }
        
        // Emotion skorları (basit simülasyon)
        result.EmotionScores["joy"] = result.PositiveScore * 0.6;
        result.EmotionScores["sadness"] = result.NegativeScore * 0.4;
        result.EmotionScores["anger"] = result.NegativeScore * 0.3;
        result.EmotionScores["fear"] = result.NegativeScore * 0.3;
        
        return result;
    }

    public async Task<List<string>> ExtractKeywordsAsync(string content, int maxKeywords = 10)
    {
        // Keyword extraction simülasyonu
        await Task.Delay(20);
        
        // Basit TF-IDF benzeri yaklaşım
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Select(w => w.ToLowerInvariant())
            .Where(w => !IsStopWord(w));
        
        var wordFrequency = words
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Take(maxKeywords)
            .Select(g => g.Key)
            .ToList();
        
        return wordFrequency;
    }

    public async Task<List<ContentCategory>> CategorizeContentAsync(string content)
    {
        // İçerik kategorizasyon simülasyonu
        await Task.Delay(20);
        
        var categories = new List<ContentCategory>();
        var lowerContent = content.ToLowerInvariant();
        
        // Basit kategori tespiti
        if (lowerContent.Contains("maaş") || lowerContent.Contains("ücret") || lowerContent.Contains("zam"))
        {
            categories.Add(new ContentCategory 
            { 
                CategoryName = "Maaş ve Yan Haklar",
                ConfidenceScore = 0.8,
                SubCategories = new List<string> { "Maaş", "Prim", "Zam" }
            });
        }
        
        if (lowerContent.Contains("çalışma") || lowerContent.Contains("ortam") || lowerContent.Contains("ofis"))
        {
            categories.Add(new ContentCategory 
            { 
                CategoryName = "Çalışma Ortamı",
                ConfidenceScore = 0.7,
                SubCategories = new List<string> { "Ofis", "Kültür", "İş-Yaşam Dengesi" }
            });
        }
        
        if (lowerContent.Contains("yönetim") || lowerContent.Contains("müdür") || lowerContent.Contains("liderlik"))
        {
            categories.Add(new ContentCategory 
            { 
                CategoryName = "Yönetim",
                ConfidenceScore = 0.75,
                SubCategories = new List<string> { "Liderlik", "İletişim", "Destek" }
            });
        }
        
        return categories;
    }

    public async Task<ModerationResult> ModerateReviewAsync(string commentText, string commentType)
    {
         if (string.IsNullOrWhiteSpace(commentText))
            {
                return ModerationResult.CreateRejected(
                    "Yorum metni boş olamaz",
                    new List<string>(),
                    1.0
                );
            }

            var flaggedWords = FilterProhibitedWords(commentText);
            var contentScore = CalculateContentScore(commentText);
            var categoryScores = await AnalyzeCategoryScoresAsync(commentText);

            // Yasaklı kelime kontrolü
            if (flaggedWords.Any())
            {
                return ModerationResult.CreateRejected(
                    "Uygunsuz içerik tespit edildi",
                    flaggedWords,
                    0.9,
                    categoryScores,
                    new List<string> 
                    { 
                        "Lütfen uygunsuz kelimeleri kaldırın",
                        "Profesyonel bir dil kullanın"
                    }
                );
            }

            // Spam kontrolü
            if (await IsSpamPatternAsync(commentText))
            {
                return ModerationResult.CreateRejected(
                    "Spam içerik tespit edildi",
                    new List<string> { "spam pattern" },
                    0.8,
                    categoryScores,
                    new List<string> 
                    { 
                        "Reklam içeriği kaldırın",
                        "Kişisel bilgileri paylaşmayın"
                    }
                );
            }

            // Çok kısa yorum kontrolü
            if (commentText.Length < 50)
            {
                return ModerationResult.CreatePendingReview(
                    "Yorum çok kısa",
                    new List<string>(),
                    0.5,
                    categoryScores,
                    new List<string> 
                    { 
                        $"En az 50 karakter yazın (Mevcut: {commentText.Length})"
                    }
                );
            }

            // Başarılı
            return ModerationResult.CreateApproved(
                "İçerik uygun",
                contentScore,
                categoryScores
            );
    }

    public async Task<ModerationResult> ModerateCompanyInfoAsync(string companyName, string? description = null)
    {
        var flaggedWords = FilterProhibitedWords(companyName);
        if (description != null)
        {
            flaggedWords.AddRange(FilterProhibitedWords(description));
        }

        if (flaggedWords.Any())
        {
            return ModerationResult.CreateRejected(
                "Şirket bilgilerinde uygunsuz içerik",
                flaggedWords.Distinct().ToList(),
                0.9,
                null,
                new List<string> { "Şirket adı ve açıklaması profesyonel olmalıdır" }
            );
        }

        // Sahte şirket kontrolü
        if (IsSuspiciousCompanyName(companyName))
        {
            return ModerationResult.CreatePendingReview(
                "Şüpheli şirket adı",
                new List<string>(),
                0.6
            );
        }

        return ModerationResult.CreateApproved("Şirket bilgileri uygun");
    }

    public async Task<ModerationResult> ModerateUsernameAsync(string username)
    {
        var flaggedWords = FilterProhibitedWords(username);
            
        if (flaggedWords.Any())
        {
            return ModerationResult.CreateRejected(
                "Kullanıcı adında uygunsuz içerik",
                flaggedWords,
                1.0,
                null,
                new List<string> { "Lütfen farklı bir kullanıcı adı seçin" }
            );
        }

        // Özel karakter kontrolü
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$"))
        {
            return ModerationResult.CreateRejected(
                "Geçersiz karakterler",
                new List<string> { "special characters" },
                0.8,
                null,
                new List<string> { "Sadece harf, rakam, tire ve alt çizgi kullanın" }
            );
        }

        return ModerationResult.CreateApproved("Kullanıcı adı uygun");
    }

    public async Task<List<ModerationResult>> ModerateBulkAsync(List<string> contents)
    {
        var results = new List<ModerationResult>();
            
        foreach (var content in contents)
        {
            var result = await ModerateReviewAsync(content, "bulk");
            results.Add(result);
        }
            
        return results;
    }

    public async Task<string> SanitizeContentAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // HTML tag'leri temizle
        content = Regex.Replace(content, @"<[^>]+>", string.Empty);
            
        // Çoklu boşlukları temizle
        content = Regex.Replace(content, @"\s+", " ");
            
        // Script injection'ı önle
        content = content.Replace("<script", "&lt;script")
            .Replace("javascript:", "")
            .Replace("onclick", "")
            .Replace("onerror", "");
            
        return content.Trim();
    }

    public async Task<bool> IsSpamAsync(string content, string userId)
    {
        // Kullanıcının son yorumlarını kontrol et
        // TODO: Repository üzerinden kullanıcının son yorumlarını al
            
        // Spam pattern kontrolü
        return await IsSpamPatternAsync(content);
    }

    public List<string> FilterProhibitedWords(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new List<string>();

        var lowerContent = content.ToLowerInvariant();
        var foundWords = new List<string>();

        foreach (var word in _prohibitedWords)
        {
            if (lowerContent.Contains(word))
            {
                foundWords.Add(word);
            }
        }

        return foundWords.Distinct().ToList();
    }

    public double CalculateContentScore(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        double score = 1.0;

        // Uzunluk skoru
        if (content.Length < 50) score -= 0.3;
        else if (content.Length > 500) score += 0.1;

        // Büyük harf oranı
        var upperRatio = (double)content.Count(char.IsUpper) / content.Length;
        if (upperRatio > 0.5) score -= 0.2;

        // Noktalama işareti kullanımı
        if (content.Contains(".") || content.Contains(",")) score += 0.1;

        // Tekrarlayan karakter kontrolü
        if (_suspiciousPatterns[3].IsMatch(content)) score -= 0.3;

        return Math.Clamp(score, 0, 1);
    }

    // Private helper methods
    private async Task<double> CalculateSpamScoreAsync(string content)
    {
        double score = 0;
        var lowerContent = content.ToLowerInvariant();

        // URL/Link kontrolü
        if (Regex.IsMatch(content, @"https?://|www\.", RegexOptions.IgnoreCase))
        {
            score += 0.3;
        }

        // Tekrarlayan kelimeler
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var uniqueWords = words.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var repetitionRatio = 1.0 - ((double)uniqueWords / words.Length);
        score += repetitionRatio * 0.3;

        // Yasaklı spam kelimeleri
        var spamKeywords = new[] { "reklam", "tıkla", "kazan", "fırsat", "ücretsiz", "garanti", "para", "zengin" };
        foreach (var keyword in spamKeywords)
        {
            if (lowerContent.Contains(keyword))
            {
                score += 0.1;
            }
        }
        
        // Simülasyon için delay
        await Task.Delay(5);

        return Math.Min(score, 1.0);
    }

    private double CheckPersonalInfo(string content, ModerationDetails details)
    {
        double score = 0;
        
        foreach (var pattern in _personalInfoPatterns)
        {
            var matches = pattern.Matches(content);
            if (matches.Count > 0)
            {
                score += 0.3;
                details.DetectedPersonalInfo.Add($"Pattern matched: {pattern}");
            }
        }

        // İsim soyisim pattern'i (basit)
        if (Regex.IsMatch(content, @"\b[A-ZÇĞİÖŞÜ][a-zçğıöşü]+\s+[A-ZÇĞİÖŞÜ][a-zçğıöşü]+\b"))
        {
            score += 0.1;
            details.DetectedPersonalInfo.Add("Possible name detected");
        }

        return Math.Min(score, 1.0);
    }

    private double CheckProhibitedWords(string content)
    {
        double score = 0;
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            if (_prohibitedWords.Contains(word))
            {
                score += 0.2;
            }
        }

        return Math.Min(score, 1.0);
    }

    private double CheckPersonalAttacks(string content)
    {
        var attackWords = new[] { "aptal", "salak", "gerizekalı", "mal", "ahmak", "rezil", "berbat" };
        var lowerContent = content.ToLowerInvariant();
        
        double score = 0;
        foreach (var word in attackWords)
        {
            if (lowerContent.Contains(word))
                score += 0.3;
        }
        
        return Math.Min(score, 1.0);
    }

    private bool HasExcessiveRepetition(string content)
    {
        // Ardışık tekrarlayan karakterler
        if (Regex.IsMatch(content, @"(.)\1{4,}"))
            return true;

        // Tekrarlayan kelimeler
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length - 2; i++)
        {
            if (words[i] == words[i + 1] && words[i] == words[i + 2])
                return true;
        }

        return false;
    }

    private double CalculateCapsRatio(string content)
    {
        var letters = content.Where(char.IsLetter).ToList();
        if (letters.Count == 0) return 0;
        
        var upperCount = letters.Count(char.IsUpper);
        return (double)upperCount / letters.Count;
    }

    private double CalculateToxicityScore(ModerationDetails details)
    {
        // Ağırlıklı ortalama
        var score = (details.ProfanityScore * 0.3) +
                   (details.HateSpeechScore * 0.3) +
                   (details.PersonalAttackScore * 0.2) +
                   (details.SpamScore * 0.1) +
                   (details.ConfidentialInfoScore * 0.1);

        return Math.Min(score, 1.0);
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new[] { "ve", "veya", "ile", "için", "ama", "ancak", "çünkü", "ki", "da", "de" };
        return stopWords.Contains(word);
    }
    
    private async Task<bool> IsSpamPatternAsync(string content)
    {
        var lowerContent = content.ToLowerInvariant();

        // URL sayısı kontrolü
        var urlCount = _suspiciousPatterns[2].Matches(content).Count;
        if (urlCount > 2) return true;

        // Telefon numarası kontrolü
        if (_suspiciousPatterns[0].IsMatch(content)) return true;

        // Email kontrolü
        if (_suspiciousPatterns[1].IsMatch(content)) return true;

        // Spam kelimeleri
        var spamKeywords = new[] { "tıkla", "kazanç", "bedava", "ücretsiz", "fırsat" };
        var spamCount = spamKeywords.Count(keyword => lowerContent.Contains(keyword));
        if (spamCount >= 2) return true;

        return false;
    }

    private bool IsSuspiciousCompanyName(string companyName)
    {
        var suspicious = new[] { "test", "deneme", "fake", "sahte", "xxx" };
        var lower = companyName.ToLowerInvariant();
            
        return suspicious.Any(s => lower.Contains(s));
    }

    private async Task<Dictionary<string, double>> AnalyzeCategoryScoresAsync(string content)
    {
        // Gerçek uygulamada AI/ML modeli kullanılır
        // Şimdilik basit bir scoring
        var scores = new Dictionary<string, double>
        {
            ["toxicity"] = 0.1,
            ["spam"] = 0.2,
            ["coherence"] = 0.8,
            ["relevance"] = 0.7
        };

        // Basit analiz
        if (FilterProhibitedWords(content).Any())
            scores["toxicity"] = 0.8;

        if (await IsSpamPatternAsync(content))
            scores["spam"] = 0.9;

        return scores;
    }
}
