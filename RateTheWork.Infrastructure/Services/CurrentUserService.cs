using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? AnonymousUsername => _httpContextAccessor.HttpContext?.User?.FindFirstValue("anonymous_username");

    public string? UserName
    {
        get
        {
            var anonymousUsername = AnonymousUsername;
            if (!string.IsNullOrEmpty(anonymousUsername))
                return anonymousUsername;

            var firstName = _httpContextAccessor.HttpContext?.User?.FindFirstValue("first_name");
            var lastName = _httpContextAccessor.HttpContext?.User?.FindFirstValue("last_name");

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                return $"{firstName} {lastName}";

            return Email?.Split('@')[0]; // E-posta kullanıcı adı kısmına geri dön
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public List<string> Roles
    {
        get
        {
            var roles = new List<string>();
            var user = _httpContextAccessor.HttpContext?.User;

            if (user != null)
            {
                roles.AddRange(user.FindAll(ClaimTypes.Role).Select(c => c.Value));
            }

            return roles;
        }
    }

    public string? IpAddress
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Yönlendirilmiş IP kontrolü (proxy/yük dengeleyici arkasında ise)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Gerçek IP başlığı kontrolü (bazı proxy'ler bunu kullanır)
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Uzak IP'ye geri dön
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
