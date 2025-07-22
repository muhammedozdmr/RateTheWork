using RateTheWork.Domain.Enums.Review;

namespace RateTheWork.Domain.Events.ReviewVote;

/// <summary>
/// 1. Yorum oylandı event'i
/// </summary>
public class ReviewVotedEvent : DomainEventBase
{
    public ReviewVotedEvent
    (
        string? voteId
        , string userId
        , string reviewId
        , bool isUpvote
        , string voteSource
        , DateTime votedAt
    ) : base()
    {
        VoteId = voteId;
        UserId = userId;
        ReviewId = reviewId;
        IsUpvote = isUpvote;
        VoteSource = voteSource;
        VotedAt = votedAt;
    }

    public string? VoteId { get; }
    public string UserId { get; }
    public string ReviewId { get; }
    public bool IsUpvote { get; }
    public string VoteSource { get; }
    public DateTime VotedAt { get; }
}

/// <summary>
/// 2. Yorum oyu değiştirildi event'i
/// </summary>
public class ReviewVoteChangedEvent : DomainEventBase
{
    public ReviewVoteChangedEvent
    (
        string? voteId
        , string userId
        , string reviewId
        , bool oldIsUpvote
        , bool newIsUpvote
        , int changeCount
        , DateTime changedAt
    ) : base()
    {
        VoteId = voteId;
        UserId = userId;
        ReviewId = reviewId;
        OldIsUpvote = oldIsUpvote;
        NewIsUpvote = newIsUpvote;
        ChangeCount = changeCount;
        ChangedAt = changedAt;
    }

    public string? VoteId { get; }
    public string UserId { get; }
    public string ReviewId { get; }
    public bool OldIsUpvote { get; }
    public bool NewIsUpvote { get; }
    public int ChangeCount { get; }
    public DateTime ChangedAt { get; }
}

/// <summary>
/// 3. Yorum oyları güncellendi event'i
/// </summary>
public class ReviewVoteCountsUpdatedEvent : DomainEventBase
{
    public ReviewVoteCountsUpdatedEvent
    (
        string reviewId
        , int oldUpvotes
        , int newUpvotes
        , int oldDownvotes
        , int newDownvotes
        , DateTime updatedAt
    ) : base()
    {
        ReviewId = reviewId;
        OldUpvotes = oldUpvotes;
        NewUpvotes = newUpvotes;
        OldDownvotes = oldDownvotes;
        NewDownvotes = newDownvotes;
        UpdatedAt = updatedAt;
    }

    public string ReviewId { get; }
    public int OldUpvotes { get; }
    public int NewUpvotes { get; }
    public int OldDownvotes { get; }
    public int NewDownvotes { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 4. Yorum oyu kullanıldı event'i
/// </summary>
public class ReviewVoteCastEvent : DomainEventBase
{
    public ReviewVoteCastEvent
    (
        string voteId
        , string reviewId
        , string userId
        , VoteType voteType
    ) : base()
    {
        VoteId = voteId;
        ReviewId = reviewId;
        UserId = userId;
        VoteType = voteType;
    }

    public string VoteId { get; }
    public string ReviewId { get; }
    public string UserId { get; }
    public VoteType VoteType { get; }
}
