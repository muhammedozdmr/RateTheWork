using System.Text.RegularExpressions;

namespace RateTheWork.Domain.Extensions;

/// <summary>
/// Doğrulama için extension metodları
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Email formatının geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // RFC 5322 standardına uygun email regex
            var regex = new Regex(
                @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Telefon numarasının geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidPhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Türkiye telefon numarası formatı
        var cleanedNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // +90 ile başlıyorsa kaldır
        if (cleanedNumber.StartsWith("+90"))
            cleanedNumber = cleanedNumber.Substring(3);

        // 0 ile başlıyorsa kaldır
        if (cleanedNumber.StartsWith("0"))
            cleanedNumber = cleanedNumber.Substring(1);

        // 10 haneli olmalı ve sadece rakam içermeli
        return cleanedNumber.Length == 10 && cleanedNumber.All(char.IsDigit);
    }

    /// <summary>
    /// TC Kimlik numarasının geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidTcKimlikNo(this string tcKimlikNo)
    {
        if (string.IsNullOrWhiteSpace(tcKimlikNo))
            return false;

        // 11 haneli olmalı
        if (tcKimlikNo.Length != 11)
            return false;

        // Sadece rakam içermeli
        if (!tcKimlikNo.All(char.IsDigit))
            return false;

        // İlk hane 0 olamaz
        if (tcKimlikNo[0] == '0')
            return false;

        // TC Kimlik No algoritması
        var digits = tcKimlikNo.Select(c => int.Parse(c.ToString())).ToArray();

        var sumOdd = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var sumEven = digits[1] + digits[3] + digits[5] + digits[7];

        var digit10 = ((sumOdd * 7) - sumEven) % 10;
        if (digit10 != digits[9])
            return false;

        var sumAll = digits.Take(10).Sum();
        var digit11 = sumAll % 10;

        return digit11 == digits[10];
    }

    /// <summary>
    /// Vergi numarasının geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidVergiNo(this string vergiNo)
    {
        if (string.IsNullOrWhiteSpace(vergiNo))
            return false;

        // 10 haneli olmalı ve sadece rakam içermeli
        return vergiNo.Length == 10 && vergiNo.All(char.IsDigit);
    }

    /// <summary>
    /// URL'nin geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidUrl(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// LinkedIn URL'sinin geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidLinkedInUrl(this string url)
    {
        if (!url.IsValidUrl())
            return false;

        var uri = new Uri(url);
        return uri.Host.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase) &&
               (uri.AbsolutePath.StartsWith("/in/") || uri.AbsolutePath.StartsWith("/company/"));
    }

    /// <summary>
    /// Şirket adının geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidCompanyName(this string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return false;

        // En az 3 karakter olmalı
        if (companyName.Trim().Length < 3)
            return false;

        // Sadece özel karakterlerden oluşmamalı
        if (!companyName.Any(char.IsLetterOrDigit))
            return false;

        // Test/Fake şirket adları kontrolü
        var suspiciousPatterns = new[] { "test", "demo", "fake", "xxx", "aaa", "asdasd", "qwerty" };
        var lowerName = companyName.ToLowerInvariant();

        return !suspiciousPatterns.Any(pattern => lowerName.Contains(pattern));
    }

    /// <summary>
    /// Kullanıcı adının geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidUsername(this string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // Uzunluk kontrolü
        if (username.Length < 3 || username.Length > 30)
            return false;

        // Sadece harf, rakam, alt çizgi ve tire içermeli
        var regex = new Regex(@"^[a-zA-Z0-9_-]+$");
        return regex.IsMatch(username);
    }

    /// <summary>
    /// Şifrenin güçlü olup olmadığını kontrol eder
    /// </summary>
    public static bool IsStrongPassword(this string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // En az 8 karakter
        if (password.Length < 8)
            return false;

        var hasUpperCase = password.Any(char.IsUpper);
        var hasLowerCase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

        // En az 3 kriter sağlanmalı
        var criteriaMet = new[] { hasUpperCase, hasLowerCase, hasDigit, hasSpecialChar }.Count(x => x);
        return criteriaMet >= 3;
    }

    /// <summary>
    /// IBAN'ın geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidIban(this string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        // Boşlukları kaldır
        iban = iban.Replace(" ", "").ToUpperInvariant();

        // Türkiye IBAN'ı 26 karakterdir ve TR ile başlar
        if (!iban.StartsWith("TR") || iban.Length != 26)
            return false;

        // IBAN mod 97 algoritması için implementasyon
        // Gerçek uygulamada daha detaylı kontrol yapılmalı
        return iban.Substring(2).All(char.IsDigit);
    }

    /// <summary>
    /// Yorum metninin geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidReviewContent(this string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var trimmedContent = content.Trim();

        // Minimum uzunluk kontrolü
        if (trimmedContent.Length < 50)
            return false;

        // Maximum uzunluk kontrolü
        if (trimmedContent.Length > 5000)
            return false;

        // Sadece tekrarlayan karakterlerden oluşmamalı
        if (Regex.IsMatch(trimmedContent, @"^(.)\1+$"))
            return false;

        // En az birkaç kelime içermeli
        var wordCount = trimmedContent.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;
        if (wordCount < 5)
            return false;

        return true;
    }

    /// <summary>
    /// Tarih aralığının geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidDateRange(this DateTime startDate, DateTime endDate)
    {
        return startDate <= endDate && startDate <= DateTime.UtcNow;
    }

    /// <summary>
    /// Doğum tarihinin geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidBirthDate(this DateTime birthDate)
    {
        var age = DateTime.UtcNow.Year - birthDate.Year;
        if (birthDate > DateTime.UtcNow.AddYears(-age)) age--;

        // 18-100 yaş aralığında olmalı
        return age >= 18 && age <= 100;
    }

    /// <summary>
    /// Dosya uzantısının izin verilen türde olup olmadığını kontrol eder
    /// </summary>
    public static bool IsAllowedFileExtension(this string fileName, params string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            return false;

        return allowedExtensions.Any(ext => extension == ext.ToLowerInvariant());
    }

    /// <summary>
    /// Koordinatların geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValidCoordinate(this double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
    }
}
