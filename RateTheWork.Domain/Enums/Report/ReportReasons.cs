namespace RateTheWork.Domain.Enums.Report;

public enum ReportReasons
{
    /// <summary>
    /// Uygunsuz içerik
    /// </summary>
    InappropriateContent,
        
    /// <summary>
    /// Yanlış veya yanıltıcı bilgi
    /// </summary>
    FalseInformation,
        
    /// <summary>
    /// Spam
    /// </summary>
    Spam,
        
    /// <summary>
    /// Taciz veya zorbalık
    /// </summary>
    Harassment,
        
    /// <summary>
    /// Kişisel saldırı
    /// </summary>
    PersonalAttack,
        
    /// <summary>
    /// Gizli bilgi paylaşımı
    /// </summary>
    ConfidentialInfo,
        
    /// <summary>
    /// Konu dışı içerik
    /// </summary>
    OffTopic,
        
    /// <summary>
    /// Mükerrer içerik
    /// </summary>
    Duplicate,
        
    /// <summary>
    /// Yüksek downvote oranı (sistem tarafından)
    /// </summary>
    HighDownvoteRatio,
        
    /// <summary>
    /// Oy manipülasyonu şüphesi (sistem tarafından)
    /// </summary>
    VoteManipulation,
        
    /// <summary>
    /// Şüpheli aktivite (sistem tarafından)
    /// </summary>
    SuspiciousActivity,
        
    /// <summary>
    /// Kişisel bilgi paylaşımı
    /// </summary>
    PersonalInformation,
        
    /// <summary>
    /// Yasadışı içerik
    /// </summary>
    IllegalContent,
        
    /// <summary>
    /// Sahte yorum
    /// </summary>
    FakeReview,
        
    /// <summary>
    /// Diğer
    /// </summary>
    Other
}
