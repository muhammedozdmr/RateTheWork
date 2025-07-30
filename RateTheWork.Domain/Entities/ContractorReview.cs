using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Common;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Contractor/Freelancer yorumu entity'si - Şirketlerle çalışan kişi/firmaların yorumları
/// </summary>
public class ContractorReview : AuditableBaseEntity, IAggregateRoot
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private ContractorReview() : base()
    {
    }

    // Properties
    public string CompanyId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string ReviewedById { get; private set; } = string.Empty; // Alias for UserId
    public Guid? ContractorId { get; private set; } // Legacy property
    public string Status { get; private set; } = "Approved"; // Status of the review
    public string ProjectDescription { get; private set; } = string.Empty;
    public decimal ProjectBudget { get; private set; }
    public string ProjectDuration { get; private set; } = string.Empty; // "1-3 ay", "3-6 ay", vb.
    public DateTime ProjectStartDate { get; private set; }
    public DateTime ProjectEndDate { get; private set; }

    // Puanlama kriterleri (1-5 arası)
    public decimal PaymentTimelinessRating { get; private set; } // Ödeme zamanlaması
    public decimal CommunicationRating { get; private set; } // İletişim kalitesi
    public decimal ProjectManagementRating { get; private set; } // Proje yönetimi
    public decimal TechnicalCompetenceRating { get; private set; } // Teknik yeterlilik
    public decimal OverallRating { get; private set; } // Genel puan

    public string ReviewText { get; private set; } = string.Empty;
    public bool WouldWorkAgain { get; private set; } // Tekrar çalışır mıydınız?
    public bool IsVerified { get; private set; } = false; // Sözleşme/fatura ile doğrulandı mı?
    public string? VerificationDocumentUrl { get; private set; }
    public bool IsAnonymous { get; private set; } = true;
    public bool IsActive { get; private set; } = true;

    public int Upvotes { get; private set; } = 0;
    public int Downvotes { get; private set; } = 0;
    public decimal HelpfulnessScore { get; private set; } = 0;

    // Navigation
    public virtual Company? Company { get; private set; }
    public virtual Company? Contractor { get; private set; } // Legacy navigation
    public virtual User? User { get; private set; }
    public virtual User? ReviewedBy { get; private set; } // Navigation via ReviewedById

    /// <summary>
    /// Puanı double olarak al (legacy uyumluluk)
    /// </summary>
    public double Rating => (double)OverallRating;

    /// <summary>
    /// Yeni contractor yorumu oluşturur
    /// </summary>
    public static ContractorReview Create
    (
        string companyId
        , string userId
        , string projectDescription
        , decimal projectBudget
        , string projectDuration
        , DateTime projectStartDate
        , DateTime projectEndDate
        , decimal paymentTimelinessRating
        , decimal communicationRating
        , decimal projectManagementRating
        , decimal technicalCompetenceRating
        , string reviewText
        , bool wouldWorkAgain
        , bool isAnonymous = true
        , string? verificationDocumentUrl = null
    )
    {
        // Validations
        if (string.IsNullOrWhiteSpace(companyId))
            throw new ArgumentNullException(nameof(companyId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(projectDescription))
            throw new ArgumentNullException(nameof(projectDescription));

        if (projectBudget < 0)
            throw new BusinessRuleException("Proje bütçesi negatif olamaz.");

        if (projectEndDate < projectStartDate)
            throw new BusinessRuleException("Proje bitiş tarihi başlangıç tarihinden önce olamaz.");

        if (string.IsNullOrWhiteSpace(reviewText))
            throw new ArgumentNullException(nameof(reviewText));

        if (reviewText.Length < 50)
            throw new BusinessRuleException("Yorum en az 50 karakter olmalıdır.");

        if (reviewText.Length > 2000)
            throw new BusinessRuleException("Yorum 2000 karakteri aşamaz.");

        // Validate ratings (1-5 arası)
        ValidateRating(paymentTimelinessRating, nameof(paymentTimelinessRating));
        ValidateRating(communicationRating, nameof(communicationRating));
        ValidateRating(projectManagementRating, nameof(projectManagementRating));
        ValidateRating(technicalCompetenceRating, nameof(technicalCompetenceRating));

        // Calculate overall rating
        var overallRating = (paymentTimelinessRating + communicationRating +
                             projectManagementRating + technicalCompetenceRating) / 4;

        var review = new ContractorReview
        {
            CompanyId = companyId, UserId = userId, ReviewedById = userId, // Set ReviewedById same as UserId
            ProjectDescription = projectDescription.Trim()
            , ProjectBudget = projectBudget, ProjectDuration = projectDuration, ProjectStartDate = projectStartDate
            , ProjectEndDate = projectEndDate, PaymentTimelinessRating = paymentTimelinessRating
            , CommunicationRating = communicationRating, ProjectManagementRating = projectManagementRating
            , TechnicalCompetenceRating = technicalCompetenceRating, OverallRating = Math.Round(overallRating, 1)
            , ReviewText = reviewText.Trim(), WouldWorkAgain = wouldWorkAgain, IsAnonymous = isAnonymous
            , VerificationDocumentUrl = verificationDocumentUrl
            , IsVerified = !string.IsNullOrWhiteSpace(verificationDocumentUrl), IsActive = true, Status = "Approved"
        };

        // Domain Event
        review.AddDomainEvent(new ContractorReviewCreatedEvent(
            review.Id,
            review.CompanyId,
            review.UserId,
            review.OverallRating,
            review.IsAnonymous,
            DateTime.UtcNow
        ));

        return review;
    }

    /// <summary>
    /// Yorumu doğrula
    /// </summary>
    public void Verify(string verificationDocumentUrl)
    {
        if (IsVerified)
            return;

        if (string.IsNullOrWhiteSpace(verificationDocumentUrl))
            throw new ArgumentNullException(nameof(verificationDocumentUrl));

        VerificationDocumentUrl = verificationDocumentUrl;
        IsVerified = true;
        SetModifiedDate();

        AddDomainEvent(new ContractorReviewVerifiedEvent(
            Id,
            CompanyId,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Upvote ekle
    /// </summary>
    public void AddUpvote()
    {
        Upvotes++;
        UpdateHelpfulnessScore();
        SetModifiedDate();
    }

    /// <summary>
    /// Downvote ekle
    /// </summary>
    public void AddDownvote()
    {
        Downvotes++;
        UpdateHelpfulnessScore();
        SetModifiedDate();
    }

    /// <summary>
    /// Yorumu deaktif et
    /// </summary>
    public void Deactivate(string reason)
    {
        IsActive = false;
        SetModifiedDate();

        AddDomainEvent(new ContractorReviewDeactivatedEvent(
            Id,
            CompanyId,
            reason,
            DateTime.UtcNow
        ));
    }

    private void UpdateHelpfulnessScore()
    {
        var total = Upvotes + Downvotes;
        if (total == 0)
        {
            HelpfulnessScore = 0;
            return;
        }

        HelpfulnessScore = Math.Round((decimal)Upvotes / total * 100, 2);
    }

    private static void ValidateRating(decimal rating, string ratingName)
    {
        if (rating < 1 || rating > 5)
            throw new BusinessRuleException($"{ratingName} 1 ile 5 arasında olmalıdır.");
    }
}

/// <summary>
/// Contractor yorumu oluşturuldu event'i
/// </summary>
public class ContractorReviewCreatedEvent : DomainEventBase
{
    public ContractorReviewCreatedEvent
    (
        string reviewId
        , string companyId
        , string userId
        , decimal overallRating
        , bool isAnonymous
        , DateTime createdAt
    ) : base()
    {
        ReviewId = reviewId;
        CompanyId = companyId;
        UserId = userId;
        OverallRating = overallRating;
        IsAnonymous = isAnonymous;
        CreatedAt = createdAt;
    }

    public string ReviewId { get; }
    public string CompanyId { get; }
    public string UserId { get; }
    public decimal OverallRating { get; }
    public bool IsAnonymous { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// Contractor yorumu doğrulandı event'i
/// </summary>
public class ContractorReviewVerifiedEvent : DomainEventBase
{
    public ContractorReviewVerifiedEvent
    (
        string reviewId
        , string companyId
        , DateTime verifiedAt
    ) : base()
    {
        ReviewId = reviewId;
        CompanyId = companyId;
        VerifiedAt = verifiedAt;
    }

    public string ReviewId { get; }
    public string CompanyId { get; }
    public DateTime VerifiedAt { get; }
}

/// <summary>
/// Contractor yorumu deaktif edildi event'i
/// </summary>
public class ContractorReviewDeactivatedEvent : DomainEventBase
{
    public ContractorReviewDeactivatedEvent
    (
        string reviewId
        , string companyId
        , string reason
        , DateTime deactivatedAt
    ) : base()
    {
        ReviewId = reviewId;
        CompanyId = companyId;
        Reason = reason;
        DeactivatedAt = deactivatedAt;
    }

    public string ReviewId { get; }
    public string CompanyId { get; }
    public string Reason { get; }
    public DateTime DeactivatedAt { get; }
}
