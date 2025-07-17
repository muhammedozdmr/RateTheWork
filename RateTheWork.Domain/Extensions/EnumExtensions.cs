using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RateTheWork.Domain.Extensions;

/// <summary>
/// Enum'lar için extension metodları
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Enum'un Description attribute değerini döndürür
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Enum'un display name'ini döndürür
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null)
            return value.ToString();

        var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute != null)
            return displayAttribute.Name ?? value.ToString();

        // Display attribute yoksa Description attribute'a bak
        return value.GetDescription();
    }

    /// <summary>
    /// Enum tipinin tüm değerlerini liste olarak döndürür
    /// </summary>
    public static IEnumerable<T> GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Enum'u dictionary'e çevirir (value, description)
    /// </summary>
    public static Dictionary<int, string> ToDictionary<T>() where T : Enum
    {
        return GetValues<T>()
            .ToDictionary(
                e => Convert.ToInt32(e),
                e => e.GetDescription()
            );
    }

    /// <summary>
    /// Enum'u SelectList item'larına çevirir
    /// </summary>
    public static IEnumerable<(T Value, string Text)> ToSelectList<T>() where T : Enum
    {
        return GetValues<T>()
            .Select(e => (Value: e, Text: e.GetDescription()))
            .OrderBy(x => x.Text);
    }

    /// <summary>
    /// String değerden enum'a güvenli dönüşüm
    /// </summary>
    public static T? Parse<T>(string value, bool ignoreCase = true) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, ignoreCase, out var result) ? result : null;
    }

    /// <summary>
    /// Enum'un belirli bir flag içerip içermediğini kontrol eder
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        var valueInt = Convert.ToInt64(value);
        var flagInt = Convert.ToInt64(flag);

        return (valueInt & flagInt) == flagInt;
    }

    /// <summary>
    /// Flag enum'una değer ekler
    /// </summary>
    public static T AddFlag<T>(this T value, T flag) where T : Enum
    {
        var valueInt = Convert.ToInt64(value);
        var flagInt = Convert.ToInt64(flag);

        return (T)Enum.ToObject(typeof(T), valueInt | flagInt);
    }

    /// <summary>
    /// Flag enum'undan değer çıkarır
    /// </summary>
    public static T RemoveFlag<T>(this T value, T flag) where T : Enum
    {
        var valueInt = Convert.ToInt64(value);
        var flagInt = Convert.ToInt64(flag);

        return (T)Enum.ToObject(typeof(T), valueInt & ~flagInt);
    }

    /// <summary>
    /// Enum değerinin geçerli olup olmadığını kontrol eder
    /// </summary>
    public static bool IsValid<T>(this T value) where T : Enum
    {
        return Enum.IsDefined(typeof(T), value);
    }

    /// <summary>
    /// İki enum değerini karşılaştırır
    /// </summary>
    public static bool IsEqualTo<T>(this T value, T other) where T : Enum
    {
        return EqualityComparer<T>.Default.Equals(value, other);
    }

    /// <summary>
    /// Enum'un int değerini döndürür
    /// </summary>
    public static int ToInt<T>(this T value) where T : Enum
    {
        return Convert.ToInt32(value);
    }

    /// <summary>
    /// Int değerden enum'a dönüşüm
    /// </summary>
    public static T? FromInt<T>(int value) where T : struct, Enum
    {
        if (!Enum.IsDefined(typeof(T), value))
            return null;

        return (T)Enum.ToObject(typeof(T), value);
    }

    /// <summary>
    /// Enum'un özel attribute'unu döndürür
    /// </summary>
    public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null)
            return null;

        return field.GetCustomAttribute<TAttribute>();
    }

    /// <summary>
    /// Enum değerlerini gruplar
    /// </summary>
    public static IEnumerable<IGrouping<string, T>> GroupByCategory<T>(Func<T, string> categorySelector) where T : Enum
    {
        return GetValues<T>().GroupBy(categorySelector);
    }
}
