namespace RateTheWork.Domain.Services;

public class TcIdentityValidationService : ITcIdentityValidationService
{
    public bool IsValidTcIdentity(string tcIdentity)
    {
        // TC Kimlik No sadece sayılardan meydana gelir
        if (string.IsNullOrWhiteSpace(tcIdentity) || !tcIdentity.All(char.IsDigit))
            return false;

        // TC Kimlik No 11 haneden oluşur
        if (tcIdentity.Length != 11)
            return false;

        // TC Kimlik Numarasının ilk rakamı 0 olamaz
        if (tcIdentity[0] == '0')
            return false;

        var digits = tcIdentity.Select(c => int.Parse(c.ToString())).ToArray();

        // İlk ilişki: 1,3,5,7. basamaklar * 7 - 2,4,6,8. basamaklar
        var oddSum = digits[0] + digits[2] + digits[4] + digits[6];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var firstCheck = ((oddSum * 7) - evenSum) % 10;

        if (firstCheck != digits[9])
            return false;

        // İkinci ilişki: İlk 10 basamağın toplamının 10'a bölümünden kalan
        var totalSum = digits.Take(10).Sum();
        var secondCheck = totalSum % 10;

        return secondCheck == digits[10];
    }

    public async Task<bool> ValidateWithGovernmentServiceAsync(string tcIdentity, string firstName, string lastName, DateTime birthDate)
    {
        // Şimdilik algoritma kontrolü yapıyoruz
        // Hükümet servisi entegrasyonu sonra eklenecek
        
        if (!IsValidTcIdentity(tcIdentity))
            return false;

        // TODO: Hükümet API'sine entegrasyon
        // Şimdilik true dönüyoruz, gerçek implementasyon sonra
        await Task.Delay(100); // Simulate API call
        return true;
    }
}
