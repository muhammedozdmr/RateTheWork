namespace RateTheWork.Domain.Enums.Company;

/// <summary>
/// Şirket sektörleri
/// </summary>
public static class Sectors
{
    public static readonly List<string> All = new()
    {
        "Teknoloji",
        "Finans",
        "Sağlık",
        "Eğitim",
        "E-ticaret",
        "Perakende",
        "Üretim",
        "Lojistik",
        "İnşaat",
        "Otomotiv",
        "Turizm",
        "Medya",
        "Pazarlama",
        "Danışmanlık",
        "Hukuk",
        "Enerji",
        "Tarım",
        "Gıda",
        "Tekstil",
        "Telekomünikasyon",
        "Gayrimenkul",
        "Sigorta",
        "Havacılık",
        "Denizcilik",
        "Savunma Sanayi",
        "Kamu",
        "STK",
        "Diğer"
    };

    /// <summary>
    /// Sektör geçerli mi kontrolü
    /// </summary>
    public static bool IsValid(string sector)
    {
        return !string.IsNullOrWhiteSpace(sector) && All.Contains(sector);
    }

    /// <summary>
    /// Sektör listesini arama için filtrele
    /// </summary>
    public static List<string> Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return All;

        return All.Where(s => s.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}