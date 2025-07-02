using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Yorum Oyu: Kullanıcıların yorumlara verdiği upvote/downvote'ları temsil eder.
public class ReviewVote : BaseEntity
{
    public string UserId { get; set; } // Oyu veren kullanıcı ID'si
    public string ReviewId { get; set; } // Oy verilen yorum ID'si
    public bool IsUpvote { get; set; } // true = upvote, false = downvote

    public ReviewVote(string userId, string reviewId, bool isUpvote)
    {
        UserId = userId;
        ReviewId = reviewId;
        IsUpvote = isUpvote;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}