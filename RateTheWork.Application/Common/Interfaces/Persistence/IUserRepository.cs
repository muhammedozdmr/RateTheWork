using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Interfaces.Persistence;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByAnonymousUsernameAsync(string anonymousUsername);
    Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null);
    Task<bool> IsAnonymousUsernameUniqueAsync(string anonymousUsername, string? excludeUserId = null);
    Task<User?> GetByEmailVerificationTokenAsync(string token);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<int> GetActiveUserCountAsync();
    Task<bool> HasUserReviewedCompanyAsync(string userId, string companyId, string commentType);
    
    // Blockchain metodları
    Task<User?> GetByBlockchainWalletAddressAsync(string walletAddress);
    Task<List<User>> GetUsersWithBlockchainIdentityAsync(int skip = 0, int take = 100);
    Task<int> CountUsersWithBlockchainIdentityAsync();
    Task<bool> IsBlockchainWalletAddressUniqueAsync(string walletAddress, string? excludeUserId = null);
}