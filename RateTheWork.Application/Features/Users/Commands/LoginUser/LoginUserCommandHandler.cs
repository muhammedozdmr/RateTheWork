using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Services;
using ValidationException = RateTheWork.Application.Common.Exceptions.ValidationException;

namespace RateTheWork.Application.Features.Users.Commands.LoginUser;

/// <summary>
/// Kullanıcı giriş komutu
/// </summary>
public record LoginUserCommand : IRequest<LoginUserResult>
{
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Şifre
    /// </summary>
    public string Password { get; init; } = string.Empty;
    
    /// <summary>
    /// Beni hatırla seçeneği (30 gün token süresi)
    /// </summary>
    public bool RememberMe { get; init; }
}

/// <summary>
/// Giriş sonucu DTO'su
/// </summary>
public record LoginUserResult
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;
    
    /// <summary>
    /// Refresh token (token yenileme için)
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
    
    /// <summary>
    /// Token geçerlilik süresi
    /// </summary>
    public DateTime ExpiresAt { get; init; }
    
    /// <summary>
    /// Giriş yapan kullanıcı bilgileri
    /// </summary>
    public UserLoginInfo User { get; init; } = null!;
}

/// <summary>
/// Giriş yapan kullanıcının temel bilgileri
/// </summary>
public record UserLoginInfo
{
    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public string? UserId { get; init; } = string.Empty;
    
    /// <summary>
    /// Anonim kullanıcı adı
    /// </summary>
    public string AnonymousUsername { get; init; } = string.Empty;
    
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Meslek
    /// </summary>
    public string Profession { get; init; } = string.Empty;
    
    /// <summary>
    /// Email doğrulandı mı?
    /// </summary>
    public bool IsEmailVerified { get; init; }
    
    /// <summary>
    /// Telefon doğrulandı mı?
    /// </summary>
    public bool IsPhoneVerified { get; init; }
    
    /// <summary>
    /// TC kimlik doğrulandı mı?
    /// </summary>
    public bool IsTcIdentityVerified { get; init; }
}

/// <summary>
/// LoginUser command handler - Giriş işlemini yönetir
/// </summary>
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IConfiguration _configuration;

    public LoginUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
        _configuration = configuration;
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcıyı email ile bul
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null)
        {
            // Güvenlik için genel hata mesajı
            throw new ValidationException("Email veya şifre hatalı.");
        }

        // 2. Şifre kontrolü
        if (!_passwordHashingService.VerifyPassword(request.Password, user.HashedPassword))
        {
            // Başarısız giriş denemesini logla (ileride)
            // TODO: Implement failed login attempt logging
            throw new ValidationException("Email veya şifre hatalı.");
        }

        // 3. Kullanıcı banlı mı?
        if (user.IsBanned)
        {
            throw new ForbiddenAccessException("Hesabınız askıya alınmıştır. Destek ekibiyle iletişime geçiniz.");
        }

        // 4. JWT token oluştur
        var (accessToken, expiresAt) = GenerateJwtToken(user, request.RememberMe);
        var refreshToken = GenerateRefreshToken();

        // 5. Refresh token'ı kaydet (ileride RefreshToken entity'si eklenecek)
        // TODO: Save refresh token to database
        // await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken { ... });
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Son giriş zamanını güncelle
        user.ModifiedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginUserResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserLoginInfo
            {
                UserId = user.Id,
                AnonymousUsername = user.AnonymousUsername,
                Email = user.Email,
                Profession = user.Profession,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                IsTcIdentityVerified = user.IsTcIdentityVerified
            }
        };
    }

    /// <summary>
    /// JWT token oluşturur
    /// </summary>
    private (string Token, DateTime ExpiresAt) GenerateJwtToken(Domain.Entities.User user, bool rememberMe)
    {
        // JWT ayarlarını al
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "RateTheWork";
        var audience = jwtSettings["Audience"] ?? "RateTheWork";
        
        // Token süresi: Remember Me -> 30 gün, Normal -> 1 gün
        var expiry = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(1);
        
        // Token claim'leri
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.AnonymousUsername),
            new("Profession", user.Profession),
            new("EmailVerified", user.IsEmailVerified.ToString()),
            new("PhoneVerified", user.IsPhoneVerified.ToString()),
            new("TcVerified", user.IsTcIdentityVerified.ToString()),
            new("jti", Guid.NewGuid().ToString()) // Token ID (refresh token için)
        };

        // Signing key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Token oluştur
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return (tokenHandler.WriteToken(token), expiry);
    }

    /// <summary>
    /// Güvenli refresh token oluşturur
    /// </summary>
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

/// <summary>
/// LoginUser validation kuralları
/// </summary>
public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(256).WithMessage("Email adresi 256 karakterden uzun olamaz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Şifre 100 karakterden uzun olamaz.");
    }
}