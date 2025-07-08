using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Events.ReviewVote;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Yorum oyu entity'si - Kullanıcıların yorumlara verdiği upvote/downvote'ları temsil eder.
/// </summary>
public class ReviewVote : BaseEntity
{
    // Vote Types
    public enum VoteType
    {
        Upvote,   // Faydalı
        Downvote  // Faydalı değil
    }

    // Vote Sources
    public enum VoteSource
    {
        Direct,           // Doğrudan yorum sayfasından
        ReviewList,       // Yorum listesinden
        CompanyProfile,   // Şirket profilinden
        UserProfile,      // Kullanıcı profilinden
        SearchResults     // Arama sonuçlarından
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public string ReviewId { get; private set; } = string.Empty;
    public bool IsUpvote { get; private set; }
    public DateTime VotedAt { get; private set; }
    public VoteSource Source { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsVerifiedUser { get; private set; }
    public int UserReputationAtTime { get; private set; }
    public bool WasChanged { get; private set; } = false;
    public DateTime? LastChangedAt { get; private set; }
    public int ChangeCount { get; private set; } = 0;

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private ReviewVote() : base()
    {
    }

    /// <summary>
    /// Yeni oy oluşturur (Factory method)
    /// </summary>
    public static ReviewVote Create(
        string userId,
        string reviewId,
        bool isUpvote,
        VoteSource source = VoteSource.Direct,
        bool isVerifiedUser = false,
        int userReputation = 0,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(reviewId))
            throw new ArgumentNullException(nameof(reviewId));

        var vote = new ReviewVote
        {
            UserId = userId,
            ReviewId = reviewId,
            IsUpvote = isUpvote,
            VotedAt = DateTime.UtcNow,
            Source = source,
            IsVerifiedUser = isVerifiedUser,
            UserReputationAtTime = userReputation,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            WasChanged = false,
            ChangeCount = 0
        };

        // Domain Event
        vote.AddDomainEvent(new ReviewVotedEvent(
            vote.Id,
            userId,
            reviewId,
            isUpvote,
            source.ToString(),
            vote.VotedAt,
            DateTime.UtcNow
        ));

        return vote;
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
            DateTime.UtcNow,
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
