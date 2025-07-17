using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.Interfaces.Validators;

namespace RateTheWork.Domain.Services;

/// <summary>
/// TC Kimlik doğrulama service implementasyonu
/// </summary>
public class TcIdentityValidationService : ITcIdentityValidationService
{
    private readonly ITcIdentityValidator _tcIdentityValidator;

    public TcIdentityValidationService(ITcIdentityValidator tcIdentityValidator)
    {
        _tcIdentityValidator = tcIdentityValidator;
    }

    public bool IsValidTcIdentity(string tcIdentity)
    {
        // Validator'ı kullan
        return _tcIdentityValidator.IsValidFormat(tcIdentity);
    }

    public async Task<bool> ValidateWithMernisAsync
        (string tcIdentity, string firstName, string lastName, DateTime birthDate)
    {
        // Önce format kontrolü
        if (!IsValidTcIdentity(tcIdentity))
            return false;

        // İsim kontrolü
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return false;

        // Doğum tarihi kontrolü (18 yaşından büyük olmalı)
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;

        if (age < 18 || age > 120)
            return false;

        try
        {
            // TODO: Gerçek MERNİS entegrasyonu
            // KPSPublicSoapClient kullanarak doğrulama yapılacak

            // Şimdilik simülasyon
            await Task.Delay(100);

            // Test TC kimlikleri için özel kontrol
            if (tcIdentity == "11111111110") // Test TC
                return true;

            // Gerçek implementasyon için:
            // var client = new KPSPublicSoapClient(KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
            // var result = await client.TCKimlikNoDogrulaAsync(
            //     long.Parse(tcIdentity),
            //     firstName.ToUpper(new CultureInfo("tr-TR")),
            //     lastName.ToUpper(new CultureInfo("tr-TR")),
            //     birthDate.Year
            // );
            // return result.Body.TCKimlikNoDogrulaResult;

            // Şimdilik rastgele başarı/başarısızlık döndür (demo için)
            var random = new Random();
            return random.Next(100) > 20; // %80 başarı oranı
        }
        catch (Exception ex)
        {
            // Log exception
            // _logger.LogError(ex, "MERNİS doğrulama hatası");
            return false;
        }
    }

    public string MaskTcIdentity(string tcIdentity)
    {
        if (string.IsNullOrWhiteSpace(tcIdentity) || tcIdentity.Length != 11)
            return "***********";

        // İlk 3 ve son 2 hane görünsün
        return $"{tcIdentity.Substring(0, 3)}****{tcIdentity.Substring(9, 2)}";
    }

    public bool IsValidForeignIdentity(string identityNumber)
    {
        // Yabancı kimlik numarası validasyonu
        if (string.IsNullOrWhiteSpace(identityNumber))
            return false;

        // Yabancı kimlik no 11 haneli ve ilk karakteri 9
        if (identityNumber.Length != 11 || identityNumber[0] != '9')
            return false;

        // Sadece rakamlardan oluşmalı
        return identityNumber.All(char.IsDigit);
    }
}

/// <summary>
/// TC Kimlik validator implementasyonu
/// </summary>
public class TcIdentityValidator : ITcIdentityValidator
{
    public bool IsValidFormat(string tcIdentity)
    {
        // TC Kimlik No sadece sayılardan oluşur
        if (string.IsNullOrWhiteSpace(tcIdentity) || !tcIdentity.All(char.IsDigit))
            return false;

        // TC Kimlik No 11 haneden oluşur
        if (tcIdentity.Length != 11)
            return false;

        // TC Kimlik Numarasının ilk rakamı 0 olamaz
        if (tcIdentity[0] == '0')
            return false;

        var digits = tcIdentity.Select(c => int.Parse(c.ToString())).ToArray();

        // Algoritma kontrolü
        // 1, 3, 5, 7, 9. hanelerin toplamının 7 katı ile 
        // 2, 4, 6, 8. hanelerin toplamının farkının 10'a bölümünden kalan 10. haneye eşit olmalı
        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var firstCheck = ((oddSum * 7) - evenSum) % 10;

        if (firstCheck != digits[9])
            return false;

        // İlk 10 hanenin toplamının 10'a bölümünden kalan 11. haneye eşit olmalı
        var totalSum = digits.Take(10).Sum();
        var secondCheck = totalSum % 10;

        return secondCheck == digits[10];
    }

    public ValidationResult Validate(string tcIdentity)
    {
        if (string.IsNullOrWhiteSpace(tcIdentity))
        {
            return ValidationResult.Failure(new ValidationError
            {
                PropertyName = nameof(tcIdentity), ErrorMessage = "TC Kimlik numarası boş olamaz."
                , ErrorCode = "TC_IDENTITY_REQUIRED"
            });
        }

        if (!IsValidFormat(tcIdentity))
        {
            return ValidationResult.Failure(new ValidationError
            {
                PropertyName = nameof(tcIdentity), ErrorMessage = "Geçersiz TC Kimlik numarası."
                , ErrorCode = "TC_IDENTITY_INVALID_FORMAT", AttemptedValue = MaskValue(tcIdentity)
            });
        }

        return ValidationResult.Success();
    }

    public Task<ValidationResult> ValidateAsync(string tcIdentity, CancellationToken cancellationToken = default)
    {
        // Senkron validasyonu async olarak döndür
        return Task.FromResult(Validate(tcIdentity));
    }

    private string MaskValue(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 5)
            return "***";

        return $"{value.Substring(0, 3)}***{value.Substring(value.Length - 2)}";
    }
}
