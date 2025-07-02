using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Admin Kullanıcı: Admin paneline erişen yöneticilerin hesap bilgilerini tutmak için.
public class AdminUser : AuditableBaseEntity
{
    public string Username { get; set; } // Admin kullanıcı adı
    public string HashedPassword { get; set; } // Şifrenin hash'i
    public string Email { get; set; } // Adminin e-posta adresi
    public string Role { get; set; } // Rolü (örn: "SuperAdmin", "Moderator", "ContentManager")
    public bool IsActive { get; set; } = true; // Admin hesabı aktif mi?

    public AdminUser(string username, string hashedPassword, string email, string role)
    {
        Username = username;
        HashedPassword = hashedPassword;
        Email = email;
        Role = role;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}
