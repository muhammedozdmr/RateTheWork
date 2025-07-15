using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Review;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Events.ReviewVote;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Yorum oyu entity'si - Kullanıcıların yorumlara verdiği upvote/downvote'ları temsil eder.
/// </summary>
public class ReviewVote : BaseEntity
{
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
    /// EF Core için parametresiz private constructor
    /// </summary>
    private ReviewVote() : base()
    {
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
    public static ReviewVote Create(
        string reviewId, 
        string userId, 
        bool isUpvote,
        string? targetType = "Review",
        string? targetId = null)
    {
        var vote = new ReviewVote
        {
            ReviewId = reviewId,
            UserId = userId,
            VoteType = isUpvote ? VoteType.Upvote : VoteType.Downvote,
            IsUpvote = isUpvote,
            VotedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TargetType = targetType,
            TargetId = targetId ?? reviewId
        };
        
        // Domain Event
        vote.AddDomainEvent(new ReviewVoteCastEvent(
            vote.Id,
            reviewId,
            userId,
            isUpvote ? VoteType.Upvote : VoteType.Downvote,
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
