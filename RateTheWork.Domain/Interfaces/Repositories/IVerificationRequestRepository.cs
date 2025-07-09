using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

public interface IVerificationRequestRepository : IBaseRepository<VerificationRequest>
{
    Task<List<VerificationRequest>> GetPendingRequestsAsync();
    Task<List<VerificationRequest>> GetRequestsByUserAsync(string userId);
    Task<List<VerificationRequest>> GetRequestsByReviewAsync(string reviewId);
    Task<VerificationRequest?> GetActiveRequestByReviewAsync(string reviewId);
}
