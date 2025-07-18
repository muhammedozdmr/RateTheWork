namespace RateTheWork.Domain.ValueObjects.Review;

/// <summary>
/// Oy durumu
/// </summary>
public class VoteStatus
{
    public bool HasVoted { get; set; }
    public bool? IsUpvote { get; set; }
    public DateTime? VotedAt { get; set; }
}
