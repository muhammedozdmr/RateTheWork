using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Events;

/// <summary>
/// Yorum oylandı event'i
/// </summary>
public record ReviewVotedEvent(
    string VoteId,
    string UserId,
    string ReviewId,
    ReviewVote.VoteType VoteType,
    ReviewVote.VoteSource Source,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Yorum oyu değiştirildi event'i
/// </summary>
public record ReviewVoteChangedEvent(
    string VoteId,
    string UserId,
    string ReviewId,
    ReviewVote.VoteType OldVoteType,
    ReviewVote.VoteType NewVoteType,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

