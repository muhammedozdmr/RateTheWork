using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByAnonymousUsernameAsync(string username);
    Task<User?> GetByTcIdentityAsync(string tcIdentity);
    Task<List<Review>> GetUserReviewsAsync(string userId, int page = 1, int pageSize = 10);
    Task<List<UserBadge>> GetUserBadgesAsync(string userId);
    Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task<bool> HasUserReviewedCompanyAsync(string userId, string companyId, string commentType);
    Task<bool> IsUserBannedAsync(string userId);
    Task<int> GetUserWarningCountAsync(string userId);
    Task<bool> IsEmailTakenAsync(string email);
    Task<bool> IsUsernameTakenAsync(string username);
}
