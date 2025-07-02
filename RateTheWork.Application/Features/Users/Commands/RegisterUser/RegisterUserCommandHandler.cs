using MediatR;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Services;

namespace RateTheWork.Application.Features.Users.Commands.RegisterUser;

/// <summary>
/// Yeni kullanıcı kaydı için kullanılan command sınıfı.
/// MediatR pattern ile CQRS implementasyonu için kullanılır.
/// </summary>
public record RegisterUserCommand : IRequest<RegisterUserResult>
{
    /// <summary>
    /// Kullanıcının giriş yapacağı email adresi
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Kullanıcının şifresi (plain text olarak gelir, handler'da hash'lenir)
    /// </summary>
    public string Password { get; init; } = string.Empty;
    
    /// <summary>
    /// Kullanıcının gerçek adı (şifrelenecek)
    /// </summary>
    public string FirstName { get; init; } = string.Empty;
    
    /// <summary>
    /// Kullanıcının gerçek soyadı (şifrelenecek)
    /// </summary>
    public string LastName { get; init; } = string.Empty;
    
    /// <summary>
    /// TC Kimlik numarası (şifrelenecek ve doğrulanacak)
    /// </summary>
    public string TcIdentityNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Doğum tarihi (yaş kontrolü ve TC doğrulama için kullanılır)
    /// </summary>
    public DateTime BirthDate { get; init; }
    
    /// <summary>
    /// Kullanıcının mesleği (şifrelenmez, profilde görünür)
    /// </summary>
    public string Profession { get; init; } = string.Empty;
    
    /// <summary>
    /// Cinsiyet bilgisi (Erkek/Kadın/Diğer)
    /// </summary>
    public string Gender { get; init; } = string.Empty;
    
    /// <summary>
    /// Açık adres bilgisi (şifrelenecek)
    /// </summary>
    public string Address { get; init; } = string.Empty;
    
    /// <summary>
    /// Şehir bilgisi (şifrelenecek)
    /// </summary>
    public string City { get; init; } = string.Empty;
    
    /// <summary>
    /// İlçe bilgisi (şifrelenecek)
    /// </summary>
    public string District { get; init; } = string.Empty;
    
    /// <summary>
    /// Telefon numarası (şifrelenecek, SMS doğrulama için kullanılacak)
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>
/// Kullanıcı kaydı sonucunu döndüren DTO
/// </summary>
public record RegisterUserResult
{
    /// <summary>
    /// Başarılı kayıt durumunda oluşturulan kullanıcı ID'si
    /// </summary>
    public string UserId { get; init; } = string.Empty;
    
    /// <summary>
    /// Sistem tarafından otomatik oluşturulan anonim kullanıcı adı
    /// </summary>
    public string AnonymousUsername { get; init; } = string.Empty;
    
    /// <summary>
    /// Kayıt olan kullanıcının email adresi
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// İşlemin başarılı olup olmadığını belirtir
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Hata durumunda kullanıcıya gösterilecek mesaj
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Kullanıcı kaydı işlemini gerçekleştiren handler sınıfı.
/// Tüm business logic burada yer alır.
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ITcIdentityValidationService _tcIdentityValidationService;

    /// <summary>
    /// RegisterUserCommandHandler constructor
    /// </summary>
    /// <param name="unitOfWork">Veritabanı işlemleri için Unit of Work pattern</param>
    /// <param name="encryptionService">Hassas verileri şifrelemek için kullanılan servis</param>
    /// <param name="passwordHashingService">Şifreleri hash'lemek için kullanılan servis</param>
    /// <param name="tcIdentityValidationService">TC Kimlik doğrulama servisi</param>
    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        IPasswordHashingService passwordHashingService,
        ITcIdentityValidationService tcIdentityValidationService)
    {
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _passwordHashingService = passwordHashingService;
        _tcIdentityValidationService = tcIdentityValidationService;
    }

    /// <summary>
    /// Kullanıcı kaydı işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Kullanıcı kayıt bilgileri</param>
    /// <param name="cancellationToken">İşlem iptali için token</param>
    /// <returns>Kayıt sonucu</returns>
    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. TC Kimlik Doğrulama - Devlet sistemleri ile entegrasyon
        var isTcValid = await _tcIdentityValidationService.ValidateWithGovernmentServiceAsync(
            request.TcIdentityNumber,
            request.FirstName,
            request.LastName,
            request.BirthDate);

        if (!isTcValid)
        {
            return new RegisterUserResult
            {
                Success = false,
                ErrorMessage = "TC Kimlik bilgileri doğrulanamadı."
            };
        }

        // 2. Email Kontrolü - Aynı email ile başka kayıt var mı?
        var isEmailTaken = await _unitOfWork.Users.IsEmailTakenAsync(request.Email);
        if (isEmailTaken)
        {
            return new RegisterUserResult
            {
                Success = false,
                ErrorMessage = "Bu email adresi zaten kullanılmaktadır."
            };
        }

        // 3. TC Kimlik Duplicate Kontrolü - Bir kişi sadece bir kez kayıt olabilir
        var existingUser = await _unitOfWork.Users.GetByTcIdentityAsync(request.TcIdentityNumber);
        if (existingUser != null)
        {
            return new RegisterUserResult
            {
                Success = false,
                ErrorMessage = "Bu TC Kimlik numarası ile zaten kayıt bulunmaktadır."
            };
        }

        // 4. Anonim Kullanıcı Adı Oluştur - Kullanıcının gerçek kimliğini gizler
        var anonymousUsername = await GenerateUniqueAnonymousUsername();

        // 5. Hassas Verileri Şifrele - Azure Key Vault kullanarak AES şifreleme
        var encryptedFirstName = _encryptionService.Encrypt(request.FirstName);
        var encryptedLastName = _encryptionService.Encrypt(request.LastName);
        var encryptedTcIdentity = _encryptionService.Encrypt(request.TcIdentityNumber);
        var encryptedBirthDate = _encryptionService.Encrypt(request.BirthDate.ToString("yyyy-MM-dd"));
        var encryptedAddress = _encryptionService.Encrypt(request.Address);
        var encryptedCity = _encryptionService.Encrypt(request.City);
        var encryptedDistrict = _encryptionService.Encrypt(request.District);
        var encryptedPhoneNumber = _encryptionService.Encrypt(request.PhoneNumber);

        // 6. Password Hash'le - BCrypt kullanarak güvenli hash
        var hashedPassword = _passwordHashingService.HashPassword(request.Password);

        // 7. User Entity Oluştur
        var user = new User(
            anonymousUsername: anonymousUsername,
            hashedPassword: hashedPassword,
            email: request.Email,
            encryptedFirstName: encryptedFirstName,
            encryptedLastName: encryptedLastName,
            profession: request.Profession,
            encryptedTcIdentityNumber: encryptedTcIdentity,
            encryptedAddress: encryptedAddress,
            encryptedCity: encryptedCity,
            encryptedDistrict: encryptedDistrict,
            encryptedPhoneNumber: encryptedPhoneNumber,
            gender: request.Gender,
            encryptedBirthDate: encryptedBirthDate
        );

        // 8. Email Doğrulama Token'ı Oluştur - 24 saat geçerli
        user.EmailVerificationToken = GenerateVerificationToken();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        // 9. Veritabanına Kaydet
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Email gönderimi için event publish et
        // await _mediator.Publish(new UserRegisteredEvent(user.Id, user.Email), cancellationToken);

        return new RegisterUserResult
        {
            UserId = user.Id,
            AnonymousUsername = user.AnonymousUsername,
            Email = user.Email,
            Success = true
        };
    }

    /// <summary>
    /// Benzersiz anonim kullanıcı adı oluşturur.
    /// Format: [Sıfat][İsim][4 haneli sayı] örn: "GizliKullanıcı1234"
    /// </summary>
    /// <returns>Benzersiz kullanıcı adı</returns>
    private async Task<string> GenerateUniqueAnonymousUsername()
    {
        // Türkçe sıfatlar - kullanıcının anonimliğini vurgular
        var adjectives = new[] { "Gizli", "Anonim", "Saklı", "Gizemli", "Bilinmeyen", "Karanlık", "Sessiz", "Derin" };
        
        // Türkçe isimler - profesyonel görünüm sağlar
        var nouns = new[] { "Kullanıcı", "Değerlendirici", "Yorumcu", "Eleştirmen", "Gözlemci", "Analist", "Uzman", "Danışman" };
        
        string username;
        bool isUnique;
        
        // Benzersiz bir kullanıcı adı bulana kadar döngü
        do
        {
            var random = new Random();
            var adjective = adjectives[random.Next(adjectives.Length)];
            var noun = nouns[random.Next(nouns.Length)];
            var number = random.Next(1000, 9999); // 4 haneli rastgele sayı
            
            username = $"{adjective}{noun}{number}";
            
            // Veritabanında kontrol et
            isUnique = !await _unitOfWork.Users.IsUsernameTakenAsync(username);
        } 
        while (!isUnique);

        return username;
    }

    /// <summary>
    /// Email doğrulama için benzersiz token oluşturur.
    /// GUID kullanarak tahmin edilemez bir token üretir.
    /// </summary>
    /// <returns>32 karakterlik doğrulama token'ı</returns>
    private string GenerateVerificationToken()
    {
        // GUID'den tire işaretlerini kaldırarak 32 karakterlik token oluştur
        return Guid.NewGuid().ToString("N");
    }
}
