namespace RateTheWork.Domain.Enums.Subscription;

/// <summary>
/// Üyelik tipleri
/// </summary>
public enum SubscriptionType
{
    /// <summary>
    /// Ücretsiz üyelik (İlk 6 ay tüm kullanıcılar için)
    /// </summary>
    Free

    ,

    /// <summary>
    /// Bireysel premium üyelik
    /// </summary>
    IndividualPremium

    ,

    /// <summary>
    /// Şirket üyeliği - Temel paket
    /// </summary>
    CompanyBasic

    ,

    /// <summary>
    /// Şirket üyeliği - Profesyonel paket
    /// </summary>
    CompanyProfessional

    ,

    /// <summary>
    /// Şirket üyeliği - Kurumsal paket
    /// </summary>
    CompanyEnterprise
}
