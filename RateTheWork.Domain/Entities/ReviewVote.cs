using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Review;
using RateTheWork.Domain.Events.ReviewVote;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Yorum oyu entity'si - Kullanıcıların yorumlara verdiği upvote/downvote'ları temsil eder.
/// </summary>
public class ReviewVote : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private ReviewVote() : base()
    {
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public string ReviewId { get; private set; } = string.Empty;
    public bool IsUpvote { get; set; }
    public DateTime VotedAt { get; private set; }
    public VoteSource Source { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsVerifiedUser { get; private set; }
    public int UserReputationAtTime { get; private set; }
    public bool WasChanged { get; private set; } = false;
    public DateTime? LastChangedAt { get; private set; }
    public int ChangeCount { get; private set; } = 0;
    public DateTime UpdatedAt { get; set; }
    public string? TargetType { get; private set; }
    public string? TargetId { get; private set; }
    public VoteType VoteType { get; private set; }

    /// <summary>
    /// Oy tipini günceller
    /// </summary>
    public void UpdateVoteType(bool isUpvote)
    {
        if (IsUpvote == isUpvote)
            return; // Aynı yönde ise değişiklik yok

        IsUpvote = isUpvote;
        VoteType = isUpvote ? VoteType.Upvote : VoteType.Downvote;
        WasChanged = true;
        LastChangedAt = DateTime.UtcNow;
        ChangeCount++;
        UpdatedAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReviewVoteChangedEvent(
            Id,
            UserId,
            ReviewId,
            !isUpvote, // Eski değer
            isUpvote, // Yeni değer
            ChangeCount++,
            DateTime.UtcNow
        ));
    }

    public void UpdateVote(bool isUpvote)
    {
        IsUpvote = isUpvote;
        UpdatedAt = DateTime.UtcNow;
        SetModifiedDate();
    }

    /// <summary>
    /// Yeni oy oluşturur (Factory method)
    /// </summary>
    public static ReviewVote Create
    (
        string userId
        , string reviewId
        , bool isUpvote
        , VoteSource source
        , string? targetType = "Review"
        , string? targetId = null
        , string? ipAddress = null
        , string? userAgent = null
        , bool isVerifiedUser = false
        , int userReputationAtTime = 0
    )
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(reviewId))
            throw new ArgumentNullException(nameof(reviewId));

        var vote = new ReviewVote
        {
            UserId = userId, ReviewId = reviewId, IsUpvote = isUpvote
            , VoteType = isUpvote ? VoteType.Upvote : VoteType.Downvote, VotedAt = DateTime.UtcNow
            , UpdatedAt = DateTime.UtcNow, Source = source, TargetType = targetType ?? "Review"
            , TargetId = targetId ?? reviewId, IpAddress = ipAddress, UserAgent = userAgent
            , IsVerifiedUser = isVerifiedUser, UserReputationAtTime = userReputationAtTime
        };

        // Domain Event
        vote.AddDomainEvent(new ReviewVoteCastEvent(
            vote.Id,
            reviewId,
            userId,
            isUpvote ? VoteType.Upvote : VoteType.Downvote
        ));

        return vote;
    }

    /// <summary>
    /// Web'den upvote oluşturur
    /// </summary>
    public static ReviewVote CreateUpvoteFromWeb
    (
        string userId
        , string reviewId
        , string ipAddress
        , string userAgent
        , bool isVerifiedUser = false
        , int userReputationAtTime = 0
    )
    {
        return Create(
            userId,
            reviewId,
            true, // isUpvote
            VoteSource.Web,
            "Review",
            reviewId,
            ipAddress,
            userAgent,
            isVerifiedUser,
            userReputationAtTime
        );
    }

    /// <summary>
    /// Web'den downvote oluşturur
    /// </summary>
    public static ReviewVote CreateDownvoteFromWeb
    (
        string userId
        , string reviewId
        , string ipAddress
        , string userAgent
        , bool isVerifiedUser = false
        , int userReputationAtTime = 0
    )
    {
        return Create(
            userId,
            reviewId,
            false, // isUpvote
            VoteSource.Web,
            "Review",
            reviewId,
            ipAddress,
            userAgent,
            isVerifiedUser,
            userReputationAtTime
        );
    }

    /// <summary>
    /// Mobil uygulamadan oy oluşturur
    /// </summary>
    public static ReviewVote CreateFromMobile
    (
        string userId
        , string reviewId
        , bool isUpvote
        , string? deviceId = null
        , bool isVerifiedUser = false
        , int userReputationAtTime = 0
    )
    {
        return Create(
            userId,
            reviewId,
            isUpvote,
            VoteSource.Mobile,
            "Review",
            reviewId,
            deviceId, // IP yerine deviceId kullanıyoruz
            "MobileApp",
            isVerifiedUser,
            userReputationAtTime
        );
    }

    /// <summary>
    /// API'den oy oluşturur
    /// </summary>
    public static ReviewVote CreateFromApi
    (
        string userId
        , string reviewId
        , bool isUpvote
        , string apiKey
        , string? ipAddress = null
        , bool isVerifiedUser = false
        , int userReputationAtTime = 0
    )
    {
        return Create(
            userId,
            reviewId,
            isUpvote,
            VoteSource.Api,
            "Review",
            reviewId,
            ipAddress,
            $"API:{apiKey}",
            isVerifiedUser,
            userReputationAtTime
        );
    }

    /// <summary>
    /// Oyu değiştir (upvote'dan downvote'a veya tersi)
    /// </summary>
    public void ChangeVote()
    {
        var oldIsUpvote = IsUpvote;
        IsUpvote = !IsUpvote;
        WasChanged = true;
        LastChangedAt = DateTime.UtcNow;
        ChangeCount++;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReviewVoteChangedEvent(
            Id,
            UserId,
            ReviewId,
            oldIsUpvote,
            IsUpvote,
            ChangeCount,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Kullanıcı doğrulama durumunu güncelle
    /// </summary>
    public void UpdateUserVerificationStatus(bool isVerified, int reputation)
    {
        IsVerifiedUser = isVerified;
        UserReputationAtTime = reputation;
        SetModifiedDate();
    }
}
