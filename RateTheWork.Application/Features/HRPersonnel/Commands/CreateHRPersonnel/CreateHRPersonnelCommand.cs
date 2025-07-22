using MediatR;
using RateTheWork.Application.Common.Models;

namespace RateTheWork.Application.Features.HRPersonnel.Commands.CreateHRPersonnel;

/// <summary>
/// İK personeli oluşturma komutu
/// </summary>
public record CreateHRPersonnelCommand : IRequest<Result<CreateHRPersonnelResponse>>
{
    public string UserId { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? LinkedInProfile { get; init; }
    public string? ProfilePhotoUrl { get; init; }
}

/// <summary>
/// İK personeli oluşturma yanıtı
/// </summary>
public record CreateHRPersonnelResponse
{
    public string PersonnelId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public decimal TrustScore { get; init; }
}
