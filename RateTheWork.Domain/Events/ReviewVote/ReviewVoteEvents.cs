using RateTheWork.Domain.Enums.Review;

namespace RateTheWork.Domain.Events.ReviewVote;

/// <summary>
/// 1. Yorum oylandı event'i
/// </summary>
public record ReviewVotedEvent(
    string? VoteId
    , string UserId
    , string ReviewId
    , bool IsUpvote
    , string VoteSource
    , DateTime VotedAt
) : DomainEventBase;

/// <summary>
/// 2. Yorum oyu değiştirildi event'i
/// </summary>
public record ReviewVoteChangedEvent(
    string? VoteId
    , string UserId
    , string ReviewId
    , bool OldIsUpvote
    , bool NewIsUpvote
    , int ChangeCount
    , DateTime ChangedAt
) : DomainEventBase;

/// <summary>
/// 3. Yorum oyları güncellendi event'i
/// </summary>
public record ReviewVoteCountsUpdatedEvent(
    string ReviewId
    , int OldUpvotes
    , int NewUpvotes
    , int OldDownvotes
    , int NewDownvotes
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 4.ssss
/// </summary>
public record ReviewVoteCastEvent(
    string VoteId
    , string ReviewId
    , string UserId
    , VoteType VoteType
) : DomainEventBase;
