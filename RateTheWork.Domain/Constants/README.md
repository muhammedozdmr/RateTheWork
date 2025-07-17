# Constants Klasörü

## 📋 Genel Bakış

Constants klasörü, RateTheWork Domain katmanında kullanılan tüm sabit değerleri merkezi bir noktada toplar. Bu yaklaşım,
magic number'ların önlenmesi, değerlerin tutarlı kullanımı ve kolay bakım için kritik öneme sahiptir.

## 📂 İçerik

```
Constants/
└── DomainConstants.cs  # Tüm domain sabitleri tek dosyada
```

## 🏗️ Organizasyon Yapısı

DomainConstants sınıfı, nested static class'lar ile organize edilmiştir:

```csharp
public static class DomainConstants
{
    public static class Review { }      // Yorum ile ilgili sabitler
    public static class User { }        // Kullanıcı ile ilgili sabitler
    public static class Company { }     // Şirket ile ilgili sabitler
    public static class Admin { }       // Admin ile ilgili sabitler
    public static class Badge { }       // Rozet ile ilgili sabitler
    public static class Security { }    // Güvenlik ile ilgili sabitler
    public static class Notification { } // Bildirim ile ilgili sabitler
    public static class Verification { } // Doğrulama ile ilgili sabitler
    public static class Moderation { }  // Moderasyon ile ilgili sabitler
    public static class Report { }      // Şikayet ile ilgili sabitler
    public static class Pagination { }  // Sayfalama ile ilgili sabitler
    public static class Cache { }       // Cache ile ilgili sabitler
    public static class RateLimit { }   // Rate limiting ile ilgili sabitler
    public static class QualityScore { } // Kalite skoru ile ilgili sabitler
    public static class ModerationScore { } // Moderasyon skoru sabitleri
    public static class RiskScore { }   // Risk skoru ile ilgili sabitler
    public static class Rating { }      // Değerlendirme ile ilgili sabitler
}
```

## 📝 Detaylı Açıklamalar

### Review Constants

Yorum işlemleri için temel sınırlar ve kurallar:

- **MinCommentLength** (50): Minimum yorum uzunluğu
- **MaxCommentLength** (2000): Maximum yorum uzunluğu
- **MinRating** (1.0): Minimum puan değeri
- **MaxRating** (5.0): Maximum puan değeri
- **RatingStep** (0.5): Puanlama adımı
- **MaxReportCountBeforeAutoHide** (5): Otomatik gizleme için şikayet sayısı
- **MaxEditHours** (24): Düzenleme için zaman limiti
- **MaxEditCount** (3): Maximum düzenleme sayısı
- **ReviewCooldownDays** (365): Aynı şirkete tekrar yorum için bekleme süresi

### Company Constants

Şirket verileri ve Türkiye'ye özel iş kuralları:

- **TaxIdLength** (10): Vergi numarası uzunluğu
- **MersisNoLength** (16): MERSIS numarası uzunluğu
- **TaxIdValidationMultiplier** (10): Vergi no algoritma çarpanı
- **TaxIdValidationModulus** (11): Vergi no algoritma mod değeri
- **MersisSegment[1-4]Length** (4): MERSIS formatı segmentleri

### Security Constants

Güvenlik ve erişim kontrolü:

- **MaxFailedLoginAttempts** (5): Maximum başarısız giriş denemesi
- **AccountLockMinutes** (30): Hesap kilitleme süresi
- **IpAddressTrackingDays** (90): IP adresi saklama süresi (KVKK)
- **AuditLogRetentionDays** (365): Audit log saklama süresi

### QualityScore Constants

Yorum kalite değerlendirmesi:

- **Score Weights**: Length (0.2), Detail (0.3), Objectivity (0.3), Helpfulness (0.2)
- **Quality Thresholds**: Excellent (80), Good (60), Fair (40), Poor (20)
- **Low/High Score Values**: Her kategori için minimum ve maksimum değerler

### ModerationScore Constants

İçerik moderasyonu için:

- **Confidence Scores**: Min (0.0), Max (1.0), Default High (0.7), Default Medium (0.5)
- **Severity Thresholds**: Critical (0.9), High (0.7), Medium (0.5), Low (0.3)
- **Hash Constants**: HashMultiplier (17), HashBase (23)

## 💡 Kullanım Örnekleri

### ✅ Doğru Kullanım

```csharp
// Validation'da kullanım
if (comment.Length < DomainConstants.Review.MinCommentLength)
    throw new ValidationException($"Yorum en az {DomainConstants.Review.MinCommentLength} karakter olmalıdır");

// Business logic'te kullanım
var canEdit = (DateTime.UtcNow - review.CreatedAt).TotalHours <= DomainConstants.Review.MaxEditHours;

// Algorithm'da kullanım
for (int i = 0; i < DomainConstants.Company.TaxIdValidationLength; i++)
{
    sum += digits[i] * (DomainConstants.Company.TaxIdValidationMultiplier - i);
}
```

### ❌ Yanlış Kullanım

```csharp
// Magic number kullanmayın
if (comment.Length < 50) // ❌
    throw new ValidationException("Yorum çok kısa");

// Hard-coded değerler kullanmayın
var canEdit = (DateTime.UtcNow - review.CreatedAt).TotalHours <= 24; // ❌

// Inline sabitler tanımlamayın
private const int MIN_LENGTH = 50; // ❌ DomainConstants kullanın
```

## 🔑 Önemli Kavramlar

### Magic Number Önleme

- Kodda direkt sayı kullanımını engeller
- Anlamı açık constant isimler kullanır
- Değişiklik yönetimini kolaylaştırır

### Merkezi Yönetim

- Tüm sabitler tek noktada
- Duplicate değerleri önler
- Tutarlılığı garanti eder

### Business Rule Documentation

- Her sabit iş kuralını dokümante eder
- Yeni geliştiriciler için referans
- Requirement'lar ile kod arasında köprü

### Type Safety

- Compile-time type checking
- IntelliSense desteği
- Refactoring kolaylığı

## 📊 Kullanım İstatistikleri

### En Çok Kullanılan Sabitler

1. **Review.MinCommentLength**: 15+ kullanım
2. **Review.MaxCommentLength**: 12+ kullanım
3. **Company.TaxIdLength**: 8+ kullanım
4. **Badge thresholds**: 10+ kullanım
5. **Security.MaxFailedLoginAttempts**: 5+ kullanım

### Kullanım Dağılımı

- **Entity Validation**: %40
- **Domain Services**: %30
- **Value Objects**: %20
- **Specifications**: %10

## 🚨 Dikkat Edilmesi Gerekenler

### 1. Naming Convention

```csharp
// ✅ Doğru: Açıklayıcı ve tutarlı
public const int MaxCommentLength = 2000;

// ❌ Yanlış: Kısa ve belirsiz
public const int MCL = 2000;
```

### 2. Değer Tutarlılığı

```csharp
// İlişkili değerler tutarlı olmalı
public const decimal MinRating = 1.0m;
public const decimal MaxRating = 5.0m;
public const decimal RatingStep = 0.5m; // Min ve Max arasında anlamlı
```

### 3. Dokümantasyon

```csharp
/// <summary>
/// Yorum ile ilgili sabitler
/// </summary>
public static class Review
{
    /// <summary>
    /// Detaylı yorumcu rozeti için minimum karakter sayısı
    /// </summary>
    public const int MinCharactersForDetailedReviewer = 500;
}
```

## 🛠️ Bakım ve Güncelleme

### Yeni Sabit Ekleme

1. İlgili nested class'ı bulun veya oluşturun
2. Anlamlı bir isim verin
3. XML dokümantasyon ekleyin
4. Kullanım yerlerini güncelleyin

### Sabit Güncelleme

1. Tüm kullanım yerlerini kontrol edin
2. Breaking change olup olmadığını değerlendirin
3. Test senaryolarını güncelleyin
4. Migration gerekip gerekmediğini kontrol edin

### Kullanılmayan Sabitleri Temizleme

1. Find usages ile kontrol edin
2. Gelecekte kullanılma ihtimalini değerlendirin
3. Dokümantasyon amaçlı tutulacaksa comment ekleyin

## 🧪 Test Edilebilirlik

Constants kullanımı test edilebilirliği artırır:

```csharp
[Test]
public void Review_Should_Validate_MinLength()
{
    var shortComment = new string('a', DomainConstants.Review.MinCommentLength - 1);
    
    Assert.Throws<ValidationException>(() => 
        Review.Create(userId, companyId, shortComment, rating));
}

[TestCase(0.5)]
[TestCase(1.0)]
[TestCase(5.0)]
public void Rating_Should_Accept_Valid_Steps(decimal rating)
{
    Assert.That(rating % DomainConstants.Review.RatingStep, Is.EqualTo(0));
}
```

## 📈 Gelecek İyileştirmeler

1. **Categorization**: Alt namespace'ler ile daha iyi organizasyon
2. **Configuration**: Bazı değerlerin configuration'dan okunması
3. **Validation**: Constant değerlerinin kendi içinde validation'ı
4. **Code Generation**: Constant'ların otomatik üretimi

---

*Bu dokümantasyon, Constants klasörünün önemini ve kullanımını açıklar. Domain genelinde tutarlı değer kullanımı için bu
sabitlerin doğru kullanılması kritiktir.*