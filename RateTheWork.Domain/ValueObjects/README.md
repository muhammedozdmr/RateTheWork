# ValueObjects Klasörü

## 📋 Genel Bakış

ValueObjects klasörü, RateTheWork Domain katmanında kullanılan değer nesnelerini (Value Objects) içerir. Value
Object'ler, Domain-Driven Design'ın temel yapı taşlarından biridir. Entity'lerden farklı olarak kimliksiz (
identity-less) ve immutable (değiştirilemez) nesnelerdir. Değer eşitliği (value equality) prensibine göre çalışırlar.

## 📂 Klasör Yapısı

```
ValueObjects/
├── Common/              # Genel amaçlı value object'ler
│   ├── ValueObject.cs   # Base value object sınıfı
│   ├── Email.cs         # Email adresi
│   ├── PhoneNumber.cs   # Telefon numarası
│   ├── Address.cs       # Adres bilgisi
│   ├── Money.cs         # Para birimi ve tutar
│   ├── Rating.cs        # Puanlama değeri
│   ├── Coordinate.cs    # GPS koordinatları
│   └── DateRange.cs     # Tarih aralığı
├── Company/             # Şirket domain'ine özel
│   ├── TaxId.cs         # Vergi numarası
│   ├── MersisNo.cs      # MERSIS numarası
│   ├── CompanyReviewStatistics.cs  # Şirket istatistikleri
│   ├── CompanyRiskScore.cs         # Risk skoru
│   ├── CompanyGrowthAnalysis.cs    # Büyüme analizi
│   └── CompanyCategory.cs          # Şirket kategorisi
├── Review/              # Yorum domain'ine özel
│   ├── ReviewQualityScore.cs       # Yorum kalite skoru
│   ├── ReviewTrends.cs             # Yorum trendleri
│   ├── SentimentAnalysisResult.cs  # Duygu analizi sonucu
│   ├── ContentCategory.cs          # İçerik kategorisi
│   └── VoteStatus.cs               # Oylama durumu
├── User/                # Kullanıcı domain'ine özel
│   ├── UserBehaviorScore.cs        # Davranış skoru
│   ├── UserActivitySummary.cs      # Aktivite özeti
│   ├── BadgeProgress.cs            # Rozet ilerlemesi
│   └── UserPreferences.cs          # Kullanıcı tercihleri
└── Moderation/          # Moderasyon domain'ine özel
    ├── ModerationResult.cs         # Moderasyon sonucu
    └── ModerationDetails.cs        # Moderasyon detayları
```

## 🎯 Value Object Prensipleri

### 1. Immutability (Değiştirilemezlik)

```csharp
public sealed class Email : ValueObject
{
    public string Value { get; }  // Private set yok!
    
    private Email(string value)   // Private constructor
    {
        Value = value;
    }
    
    public static Email Create(string value)  // Factory method
    {
        // Validation...
        return new Email(value);
    }
}
```

### 2. Value Equality (Değer Eşitliği)

```csharp
var email1 = Email.Create("test@example.com");
var email2 = Email.Create("test@example.com");

// Referanslar farklı ama değerler aynı
Assert.That(email1, Is.EqualTo(email2));  // ✅ True
Assert.That(email1 == email2);            // ✅ True
```

### 3. Self-Validation (Kendi Kendini Doğrulama)

```csharp
public static Email Create(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentNullException(nameof(value));
        
    if (!IsValidEmail(value))
        throw new BusinessRuleException("Geçersiz email formatı");
        
    return new Email(value);
}
```

## 📝 Detaylı Value Object Açıklamaları

### Common Value Objects

#### 📧 Email.cs

Email adreslerini temsil eder ve doğrular.

**Özellikler:**

- RFC 5322 standardına uygun validation
- Case-insensitive karşılaştırma
- Domain kısmını ayırma

**Kullanım:**

```csharp
var email = Email.Create("user@example.com");
var domain = email.GetDomain(); // "example.com"
var isCompanyEmail = email.IsCompanyEmail("example.com"); // true
```

#### 📱 PhoneNumber.cs

Türkiye telefon numaralarını yönetir.

**Özellikler:**

- E.164 format desteği
- Türkiye telefon formatı validation
- Formatted display

**Kullanım:**

```csharp
var phone = PhoneNumber.Create("+905551234567");
var formatted = phone.GetFormatted(); // "+90 555 123 45 67"
var isValid = PhoneNumber.IsValidTurkishNumber("+905551234567"); // true
```

#### 📍 Address.cs

Fiziksel adres bilgilerini tutar.

**Özellikler:**

- Sokak, ilçe, şehir, posta kodu
- Koordinat desteği (opsiyonel)
- Full address string builder

**Kullanım:**

```csharp
var address = Address.Create(
    street: "Atatürk Cad. No:123",
    district: "Kadıköy",
    city: "İstanbul",
    postalCode: "34710",
    coordinates: Coordinate.Create(40.9887, 29.0342)
);
```

#### 💰 Money.cs

Para tutarı ve birimi.

**Özellikler:**

- Currency support (TRY, USD, EUR)
- Arithmetic operations
- Format display

**Kullanım:**

```csharp
var price = Money.Create(100.50m, Currency.TRY);
var discount = Money.Create(10m, Currency.TRY);
var final = price.Subtract(discount); // 90.50 TRY
```

#### ⭐ Rating.cs

1-5 arası puanlama değeri.

**Özellikler:**

- 0.5 adım desteği
- Min/Max validation
- Star display support

**Kullanım:**

```csharp
var rating = Rating.Create(4.5m);
var stars = rating.ToStars(); // "★★★★☆"
var isValid = Rating.IsValidStep(4.5m); // true (0.5 katları)
```

### Company Value Objects

#### 🏢 TaxId.cs

Türkiye vergi numarası.

**Özellikler:**

- 10 haneli vergi no validation
- Checksum algoritması
- Vergi dairesi kodu desteği

**Algoritma:**

```csharp
// Türkiye vergi numarası doğrulama algoritması
for (int i = 0; i < 9; i++)
{
    sum += digits[i] * (10 - i);
}
var checkDigit = (sum % 11 == 0) ? 0 : (11 - sum % 11);
```

#### 🔢 MersisNo.cs

MERSIS (Merkezi Sicil Kayıt Sistemi) numarası.

**Özellikler:**

- 16 haneli MERSIS validation
- Format desteği (XXXX-XXXX-XXXX-XXXX)
- Otomatik formatting

**Kullanım:**

```csharp
var mersis = MersisNo.Create("1234567890123456");
var formatted = mersis.GetFormatted(); // "1234-5678-9012-3456"
```

#### 📊 CompanyReviewStatistics.cs

Şirket yorum istatistikleri.

**Özellikler:**

- Ortalama puan
- Toplam yorum sayısı
- Doğrulanmış yorum sayısı
- Kategori bazlı ortalamalar
- Puan dağılımı (1-5 yıldız)

**Kullanım:**

```csharp
var stats = CompanyReviewStatistics.Create(
    averageRating: 4.2m,
    totalReviews: 150,
    verifiedReviews: 45,
    ratingDistribution: new Dictionary<int, int> 
    {
        [5] = 80, [4] = 40, [3] = 20, [2] = 7, [1] = 3
    }
);

var verificationRate = stats.GetVerificationRate(); // 0.30 (30%)
```

### Review Value Objects

#### 📈 ReviewQualityScore.cs

Yorum kalite değerlendirmesi.

**Bileşenler:**

- Length Score (Uzunluk)
- Detail Score (Detay)
- Objectivity Score (Objektiflik)
- Helpfulness Score (Yararlılık)

**Ağırlıklar:**

```csharp
OverallScore = 
    LengthScore * 0.2 +
    DetailScore * 0.3 +
    ObjectivityScore * 0.3 +
    HelpfulnessScore * 0.2;
```

#### 📊 ReviewTrends.cs

Yorum trend analizi.

**Özellikler:**

- Zaman bazlı trend analizi
- Kategori bazlı dağılım
- Sentiment değişimi
- Peak dönemleri

#### 😊 SentimentAnalysisResult.cs

Duygu analizi sonuçları.

**Sentiment Türleri:**

- Positive (Olumlu)
- Negative (Olumsuz)
- Neutral (Nötr)
- Mixed (Karışık)

**Confidence Scores:**

```csharp
var sentiment = SentimentAnalysisResult.Create(
    sentiment: Sentiment.Positive,
    confidenceScores: new Dictionary<string, double>
    {
        ["Positive"] = 0.85,
        ["Negative"] = 0.10,
        ["Neutral"] = 0.05
    }
);
```

### User Value Objects

#### 🎯 UserBehaviorScore.cs

Kullanıcı davranış puanı.

**Bileşenler:**

- Activity Score (Aktivite)
- Quality Score (Kalite)
- Consistency Score (Tutarlılık)
- Engagement Score (Etkileşim)
- Trustworthiness Score (Güvenilirlik)

**Değerlendirme:**

```csharp
var behavior = UserBehaviorScore.Create(
    overallScore: 85.5m,
    activityScore: 90m,
    qualityScore: 80m,
    consistencyScore: 85m,
    engagementScore: 88m,
    trustworthinessScore: 82m
);

var level = behavior.GetUserLevel(); // "Trusted Contributor"
```

#### 📊 UserActivitySummary.cs

Kullanıcı aktivite özeti.

**İçerik:**

- Toplam yorum sayısı
- Doğrulanmış yorum sayısı
- Helpful/Unhelpful oy sayıları
- En çok yorum yapılan sektörler
- Aylık aktivite dağılımı

### Moderation Value Objects

#### 🛡️ ModerationResult.cs

İçerik moderasyon sonucu.

**Özellikler:**

- Onay durumu (Approved/Rejected)
- Confidence score
- Flagged words listesi
- Kategori skorları
- Önerilen düzeltmeler

**Severity Levels:**

```csharp
return confidenceScore switch
{
    >= 0.9 => "Critical",
    >= 0.7 => "High",
    >= 0.5 => "Medium",
    >= 0.3 => "Low",
    _ => "Minimal"
};
```

## 💡 Best Practices

### ✅ Doğru Kullanım

#### 1. Factory Method Pattern

```csharp
// ✅ Doğru
var email = Email.Create("test@example.com");

// ❌ Yanlış
var email = new Email("test@example.com"); // Private constructor!
```

#### 2. Immutability

```csharp
// ✅ Doğru - Yeni instance döndür
public Email ChangeEmail(string newValue)
{
    return Email.Create(newValue);
}

// ❌ Yanlış - Mevcut instance'ı değiştirme
public void ChangeEmail(string newValue)
{
    this.Value = newValue; // Immutable olmalı!
}
```

#### 3. Validation

```csharp
// ✅ Doğru - Constructor'da validation
private TaxId(string value)
{
    if (!IsValidTaxId(value))
        throw new BusinessRuleException("Geçersiz vergi numarası");
    
    Value = value;
}

// ❌ Yanlış - Validation yok
public string TaxId { get; set; } // Primitive obsession!
```

## 🧪 Test Edilebilirlik

### Unit Test Örnekleri

```csharp
[Test]
public void Email_Should_Validate_Format()
{
    Assert.DoesNotThrow(() => Email.Create("valid@example.com"));
    Assert.Throws<BusinessRuleException>(() => Email.Create("invalid-email"));
}

[Test]
public void Rating_Should_Only_Accept_Valid_Steps()
{
    Assert.DoesNotThrow(() => Rating.Create(4.5m)); // ✅ 0.5 katı
    Assert.Throws<BusinessRuleException>(() => Rating.Create(4.3m)); // ❌ 0.5 katı değil
}

[Test]
public void Money_Should_Support_Arithmetic()
{
    var money1 = Money.Create(100m, Currency.TRY);
    var money2 = Money.Create(50m, Currency.TRY);
    
    var sum = money1.Add(money2);
    
    Assert.That(sum.Amount, Is.EqualTo(150m));
}
```

## 📊 Value Object İstatistikleri

### En Çok Kullanılan Value Object'ler

1. **Email**: 50+ kullanım
2. **PhoneNumber**: 40+ kullanım
3. **Rating**: 35+ kullanım
4. **Money**: 30+ kullanım
5. **Address**: 25+ kullanım

### Kategori Dağılımı

- **Common**: %40 (Genel amaçlı)
- **Company**: %25 (Şirket domain'i)
- **Review**: %20 (Yorum domain'i)
- **User**: %10 (Kullanıcı domain'i)
- **Moderation**: %5 (Moderasyon)

## 🚨 Dikkat Edilmesi Gerekenler

### 1. Primitive Obsession

```csharp
// ❌ Yanlış
public class User
{
    public string Email { get; set; }
    public string Phone { get; set; }
}

// ✅ Doğru
public class User
{
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
}
```

### 2. Anemic Value Objects

```csharp
// ❌ Yanlış - Sadece data container
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

// ✅ Doğru - Davranış içeren
public class Address : ValueObject
{
    // Properties...
    
    public string GetFullAddress()
    {
        return $"{Street}, {District}, {City} {PostalCode}";
    }
    
    public double CalculateDistanceTo(Address other)
    {
        // Haversine formula...
    }
}
```

### 3. Over-Engineering

```csharp
// ❌ Yanlış - Basit string için value object
public class ProductCode : ValueObject
{
    public string Value { get; }
    // 50 satır validation kodu...
}

// ✅ Doğru - Gerçek business value varsa
public class TaxId : ValueObject
{
    // Checksum algoritması
    // Format validation
    // Business kuralları
}
```

## 🔄 Value Object Dönüşümleri

### Entity ↔ Value Object

```csharp
// Entity'den Value Object'e
var stats = CompanyReviewStatistics.CreateFromReviews(company.Reviews);
company.UpdateStatistics(stats);

// Value Object'ten Entity'ye
var address = Address.Create(dto.Street, dto.City);
company.UpdateAddress(address);
```

### DTO ↔ Value Object

```csharp
// DTO'dan Value Object'e
var email = Email.Create(request.Email);

// Value Object'ten DTO'ya
response.Email = user.Email.Value;
response.EmailDomain = user.Email.GetDomain();
```

## 📈 Gelecek İyileştirmeler

1. **Record Types**: C# 9+ record type kullanımı
2. **Pattern Matching**: Switch expression optimizasyonları
3. **Nullable Reference Types**: Daha güvenli null handling
4. **Performance**: Struct bazlı value object'ler (küçük objeler için)

---

*Bu dokümantasyon, ValueObjects klasörünün detaylı açıklamasını içerir. Value Object'ler, domain modelinin önemli bir
parçasıdır ve primitive obsession'ı önleyerek, type safety sağlayarak ve business logic'i encapsulate ederek kodun
kalitesini artırır.*