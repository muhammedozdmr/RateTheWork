using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RateTheWork.Domain.Extensions;

/// <summary>
/// String işlemleri için extension metodları
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// String'i slug formatına çevirir (URL dostu)
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Türkçe karakterleri değiştir
        value = value.Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("İ", "i")
            .Replace("Ğ", "g")
            .Replace("Ü", "u")
            .Replace("Ş", "s")
            .Replace("Ö", "o")
            .Replace("Ç", "c");

        // Küçük harfe çevir
        value = value.ToLowerInvariant();

        // Özel karakterleri kaldır
        value = Regex.Replace(value, @"[^a-z0-9\s-]", "");

        // Çoklu boşlukları tek boşluğa çevir
        value = Regex.Replace(value, @"\s+", " ").Trim();

        // Boşlukları tire ile değiştir
        value = value.Replace(" ", "-");

        // Çoklu tireleri tek tireye çevir
        value = Regex.Replace(value, @"-+", "-");

        return value;
    }

    /// <summary>
    /// İlk harfi büyük yapar
    /// </summary>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLowerInvariant());
    }

    /// <summary>
    /// Metni belirli uzunlukta keser ve sonuna ... ekler
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Kelime sayısına göre metni keser
    /// </summary>
    public static string TruncateWords(this string value, int wordCount, string suffix = "...")
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var words = value.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= wordCount)
            return value;

        return string.Join(" ", words.Take(wordCount)) + suffix;
    }

    /// <summary>
    /// Email adresini maskeler
    /// </summary>
    public static string MaskEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return email;

        var parts = email.Split('@');
        var username = parts[0];
        var domain = parts[1];

        if (username.Length <= 3)
            return $"{username[0]}***@{domain}";

        var visibleLength = Math.Min(3, username.Length / 2);
        var masked = username.Substring(0, visibleLength) + new string('*', username.Length - visibleLength);

        return $"{masked}@{domain}";
    }

    /// <summary>
    /// Telefon numarasını maskeler
    /// </summary>
    public static string MaskPhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (cleaned.Length < 7)
            return phoneNumber;

        var lastFourDigits = cleaned.Substring(cleaned.Length - 4);
        var maskedPart = new string('*', cleaned.Length - 4);

        return maskedPart + lastFourDigits;
    }

    /// <summary>
    /// TC Kimlik numarasını maskeler
    /// </summary>
    public static string MaskTcKimlikNo(this string tcKimlikNo)
    {
        if (string.IsNullOrWhiteSpace(tcKimlikNo) || tcKimlikNo.Length != 11)
            return tcKimlikNo;

        return tcKimlikNo.Substring(0, 3) + "*****" + tcKimlikNo.Substring(8);
    }

    /// <summary>
    /// Metindeki HTML taglerini temizler
    /// </summary>
    public static string StripHtml(this string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return Regex.Replace(html, "<.*?>", string.Empty);
    }

    /// <summary>
    /// Metni Base64 formatına encode eder
    /// </summary>
    public static string ToBase64(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Base64 string'i decode eder
    /// </summary>
    public static string FromBase64(this string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Metindeki tüm boşlukları kaldırır
    /// </summary>
    public static string RemoveWhitespace(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Regex.Replace(value, @"\s+", "");
    }

    /// <summary>
    /// Camel case'e çevirir
    /// </summary>
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var parts = value.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return string.Empty;

        var result = parts[0].ToLowerInvariant();

        for (int i = 1; i < parts.Length; i++)
        {
            result += parts[i].Substring(0, 1).ToUpperInvariant() + parts[i].Substring(1).ToLowerInvariant();
        }

        return result;
    }

    /// <summary>
    /// Pascal case'e çevirir
    /// </summary>
    public static string ToPascalCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var camelCase = value.ToCamelCase();

        if (string.IsNullOrEmpty(camelCase))
            return string.Empty;

        return camelCase.Substring(0, 1).ToUpperInvariant() + camelCase.Substring(1);
    }

    /// <summary>
    /// Metinde kelime sayısını döndürür
    /// </summary>
    public static int WordCount(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        return value.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Ters çevirir
    /// </summary>
    public static string Reverse(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return new string(value.Reverse().ToArray());
    }

    /// <summary>
    /// Metindeki sayıları toplar
    /// </summary>
    public static int SumDigits(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        return value.Where(char.IsDigit).Sum(c => int.Parse(c.ToString()));
    }

    /// <summary>
    /// Güvenli bir şekilde enum'a çevirir
    /// </summary>
    public static T? ToEnum<T>(this string value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, true, out var result) ? result : null;
    }

    /// <summary>
    /// String'in null veya boşluk olup olmadığını kontrol eder
    /// </summary>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// String'in dolu olup olmadığını kontrol eder
    /// </summary>
    public static bool IsNotNullOrEmpty(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}
