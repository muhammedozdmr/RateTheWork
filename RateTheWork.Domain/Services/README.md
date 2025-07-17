# Services Klasörü

## 📋 Genel Bakış

Services klasörü, RateTheWork Domain katmanındaki domain service'lerini içerir. Domain service'ler, tek bir entity'ye
ait olmayan veya birden fazla entity/aggregate üzerinde çalışan iş mantığını kapsüller. Domain-Driven Design (DDD)
prensipleri doğrultusunda, entity'lere sığmayan kompleks iş kuralları bu service'lerde implement edilir.

## 📂 İçerik

```
Services/
├── UserDomainService.cs         # Kullanıcı işlemleri domain servisi
├── CompanyDomainService.cs      # Şirket işlemleri domain servisi
├── ReviewDomainService.cs       # Yorum işlemleri domain servisi
├── ReviewScoringService.cs      # Yorum puanlama servisi
├── ReviewValidationService.cs   # Yorum doğrulama servisi
├── ReviewAnalyticsService.cs    # Yorum analitik servisi
├── ContentModerationService.cs  # İçerik moderasyon servisi
├── BadgeDomainService.cs        # Rozet yönetim servisi
├── VoteService.cs               # Oylama servisi
├── TcIdentityValidationService.cs # TC kimlik doğrulama servisi
└── ValidationModels.cs          # Validation için yardımcı modeller
```

## 🎯 Domain Service Prensipleri

### 1. Stateless (Durumsuz)

Domain service'ler state tutmazlar, sadece logic içerirler.

### 2. Business Logic Focus

Infrastructure concern'lerden bağımsız, saf iş mantığı.

### 3. Entity Coordination

Birden fazla entity/aggregate arasında koordinasyon sağlar.

### 4. Domain Language

Ubiquitous language kullanır, teknik terimlerden kaçınır.

## 📝 Service Detayları

### 👤 UserDomainService

Kullanıcı ile ilgili kompleks iş kurallarını yönetir.

**Temel Sorumluluklar:**

- Kullanıcı aktiflik kontrolü
- Yorum yapma yetkisi kontrolü
- Güvenilirlik skoru hesaplama
- Anonimlik seviyesi belirleme
- Çalışan doğrulama işlemleri
- Aktivite özeti oluşturma
- Tercih analizi
- Davranış skoru hesaplama

**Önemli Metodlar:**

```csharp
public async Task<bool> IsUserActiveAsync(string userId)
{
    // Email doğrulaması, ban durumu, aktiflik kontrolü
}

public async Task<decimal> CalculateUserReliabilityScoreAsync(string userId)
{
    // Hesap yaşı, yorum sayısı, doğrulama oranı, beğeni oranı
    // Tutarlılık skoru hesaplamaları
}

public async Task<UserBehaviorScore> CalculateUserBehaviorScoreAsync(string userId)
{
    // Aktivite, kalite, tutarlılık, etkileşim, güvenilirlik skorları
}
```

**İş Kuralları:**

- Yeni kullanıcılar 24 saat beklemeli
- Günde maksimum 5 yorum
- Güvenilirlik skoru 0-100 arası
- Email doğrulaması zorunlu

### 🏢 CompanyDomainService

Şirket entity'si ile ilgili kompleks işlemler.

**Temel Sorumluluklar:**

- Şirket doğrulama süreci
- Risk skoru hesaplama
- İstatistik güncelleme
- Büyüme analizi
- Kategori belirleme
- Benzer şirket önerileri
- Şirket birleştirme işlemleri

**Önemli Metodlar:**

```csharp
public async Task<CompanyRiskScore> CalculateRiskScoreAsync(string companyId)
{
    // Yorum sayısı, ortalama puan, olumsuz yorum oranı
    // Son dönem trend analizi, doğrulama durumu
}

public async Task<CompanyGrowthAnalysis> AnalyzeGrowthAsync(string companyId, DateRange period)
{
    // Yorum artış oranı, puan değişimi, kategori performansı
}

public async Task MergeCompaniesAsync(string sourceId, string targetId, string mergedBy)
{
    // Yorumları taşı, istatistikleri birleştir, eski şirketi deaktive et
}
```

**Risk Faktörleri:**

- Düşük ortalama puan
- Yüksek olumsuz yorum oranı
- Ani puan düşüşleri
- Doğrulanmamış şirket durumu

### 📝 ReviewDomainService

Yorum işlemleri için merkezi domain servisi.

**Temel Sorumluluklar:**

- Yorum oluşturma yetki kontrolü
- Cooldown period kontrolü
- Anonimlik seviyesi belirleme
- Spam kontrolü
- Yorum düzenleme kuralları
- Otomatik gizleme işlemleri

**Önemli Metodlar:**

```csharp
public async Task<bool> CanUserReviewCompanyAsync(string userId, string companyId, CommentType type)
{
    // Aktif kullanıcı, ban durumu, email doğrulama
    // 365 günlük cooldown period kontrolü
    // Günlük limit kontrolü
}

public async Task<string> DetermineDisplayNameAsync(string userId, AnonymityLevel level)
{
    // Full: Gerçek isim
    // Partial: İlk isim + Soyadı baş harfi
    // High: "Anonim Kullanıcı"
}
```

**Cooldown Kuralları:**

- Aynı şirkete aynı tip yorum: 365 gün
- Farklı tip yorum: Sınırsız
- Silinen yorumlar cooldown'a dahil

### 📊 ReviewScoringService

Yorum kalite ve yararlılık skorlaması.

**Skorlama Bileşenleri:**

- **Length Score**: Yorum uzunluğu (min 50 karakter)
- **Detail Score**: Detay seviyesi, kategori doldurma
- **Objectivity Score**: Objektiflik, dengeli yaklaşım
- **Helpfulness Score**: Yararlı bilgi içeriği

**Algoritma:**

```csharp
public async Task<ReviewQualityScore> CalculateQualityScoreAsync(Review review)
{
    var lengthScore = CalculateLengthScore(review.Comment.Length);
    var detailScore = CalculateDetailScore(review);
    var objectivityScore = await CalculateObjectivityScore(review);
    var helpfulnessScore = CalculateHelpfulnessScore(review);
    
    // Ağırlıklı ortalama: 0.2, 0.3, 0.3, 0.2
}
```

**Helpfulness Hesaplama:**

```csharp
HelpfulnessScore = 50 + ((upvotes - downvotes) / Math.Max(totalVotes, 1)) * 50
```

### ✅ ReviewValidationService

Yorum içeriği doğrulama ve kural kontrolü.

**Validation Kuralları:**

- Minimum/maksimum uzunluk
- Spam pattern kontrolü
- Küfür/hakaret kontrolü
- Kişisel bilgi paylaşımı
- Reklam içeriği kontrolü
- Kategori uyumluluğu

**Önemli Metodlar:**

```csharp
public async Task<ValidationResult> ValidateReviewContentAsync(string content, CommentType type)
{
    // Uzunluk kontrolü
    // Spam kontrolü
    // İçerik uygunluğu
    // Kategori spesifik kurallar
}

public bool ContainsPersonalInfo(string content)
{
    // Telefon, email, TC kimlik pattern'leri
    // Sosyal medya hesapları
    // Adres bilgileri
}
```

### 📈 ReviewAnalyticsService

Yorum trend ve analiz servisi.

**Analiz Türleri:**

- Zaman bazlı trendler
- Kategori dağılımları
- Sentiment analizi
- Kullanıcı segmentasyonu
- Sezonsal pattern'ler

**Önemli Metodlar:**

```csharp
public async Task<ReviewTrends> AnalyzeTrendsAsync(string companyId, DateRange period)
{
    // Dönemsel yorum sayıları
    // Ortalama puan değişimi
    // Kategori performansları
    // Peak zamanları
}

public async Task<Dictionary<CommentType, decimal>> GetCategoryAveragesAsync(string companyId)
{
    // Her kategori için ortalama puanlar
    // Ağırlıklı genel ortalama
}
```

### 🛡️ ContentModerationService

AI destekli içerik moderasyonu.

**Moderasyon Özellikleri:**

- Otomatik içerik kontrolü
- Sentiment analizi
- Anahtar kelime çıkarımı
- Kategori belirleme
- Spam detection
- Dil tespiti ve çeviri

**Önemli Metodlar:**

```csharp
public async Task<ModerationResult> ModerateContentAsync(string content, string language = "tr")
{
    // Küfür/hakaret kontrolü
    // Spam pattern analizi
    // Sentiment skorlama
    // Kategori eşleştirme
}

public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string content)
{
    // Positive, Negative, Neutral, Mixed
    // Confidence skorları
    // Dominant emotion
}
```

**Moderasyon Seviyeleri:**

- **Approved**: Temiz içerik
- **Warning**: Dikkat gerektiren
- **Rejected**: Reddedilen
- **Manual Review**: Manuel kontrol gerekli

### 🏅 BadgeDomainService

Kullanıcı rozet yönetimi.

**Rozet Türleri:**

- **FirstReviewer**: İlk yorum
- **ActiveReviewer**: 10+ yorum
- **TrustedReviewer**: 5+ doğrulanmış
- **TopContributor**: 50+ yorum
- **CompanyExplorer**: 10+ farklı şirket
- **HelpfulReviewer**: %80+ upvote

**Otomatik Kazanım:**

```csharp
public async Task CheckAndAwardBadgesAsync(string userId)
{
    // Kullanıcı istatistiklerini al
    // Her rozet için kriterleri kontrol et
    // Kazanılan rozetleri ata
    // Event yayınla
}
```

### 🗳️ VoteService

Yorum oylama işlemleri.

**Oylama Kuralları:**

- Bir kullanıcı bir yoruma bir oy
- Kendi yorumuna oy veremez
- Oy değiştirilebilir
- Anonim oylama

**İş Mantığı:**

```csharp
public async Task<VoteResult> VoteAsync(string userId, string reviewId, bool isUpvote)
{
    // Yetki kontrolü
    // Mevcut oy kontrolü
    // Oy güncelleme/ekleme
    // Helpfulness skoru güncelleme
}
```

### 🆔 TcIdentityValidationService

TC kimlik numarası doğrulama.

**Doğrulama Yöntemleri:**

- Algoritma kontrolü
- Format validation
- Checksum verification
- KPS entegrasyonu (opsiyonel)

**TC Kimlik Algoritması:**

```csharp
private bool ValidateAlgorithm(string tcNo)
{
    // İlk 10 hane toplamının birler basamağı 11. hane
    // 1,3,5,7,9 haneler toplamı * 7 - 2,4,6,8 haneler toplamı mod 10 = 10. hane
}
```

### 📋 ValidationModels.cs

Validation işlemleri için yardımcı modeller.

**İçerik:**

- ValidationResult
- ValidationError
- ValidationContext
- ValidationRule

## 💡 Best Practices

### ✅ Doğru Kullanım

#### 1. Dependency Injection

```csharp
public class ReviewDomainService : IReviewDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ReviewDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

#### 2. Async/Await Pattern

```csharp
public async Task<bool> CanUserReviewCompanyAsync(string userId, string companyId)
{
    var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
    var reviews = await _unitOfWork.Repository<Review>()
        .GetAsync(r => r.UserId == userId && r.CompanyId == companyId);
    
    // Business logic...
}
```

#### 3. Exception Handling

```csharp
public async Task<decimal> CalculateUserReliabilityScoreAsync(string userId)
{
    var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
    if (user == null)
        throw new EntityNotFoundException(nameof(User), userId);
    
    // Calculation logic...
}
```

### ❌ Yanlış Kullanım

#### 1. State Tutma

```csharp
// ❌ Yanlış - Service state tutuyor
public class BadService
{
    private User _currentUser; // State!
    
    public void SetUser(User user)
    {
        _currentUser = user;
    }
}
```

#### 2. Infrastructure Bağımlılığı

```csharp
// ❌ Yanlış - Email gönderme infrastructure concern
public class BadService
{
    public async Task NotifyUserAsync(string userId)
    {
        await _emailService.SendEmailAsync(...); // Infrastructure!
    }
}
```

#### 3. Anemic Service

```csharp
// ❌ Yanlış - Sadece CRUD işlemi
public class BadService
{
    public async Task<User> GetUserAsync(string id)
    {
        return await _repository.GetByIdAsync(id); // No business logic!
    }
}
```

## 🧪 Test Stratejileri

### Unit Test

```csharp
[Test]
public async Task CanUserReviewCompany_Should_Return_False_For_Banned_User()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var service = new ReviewDomainService(mockUnitOfWork.Object);
    
    var bannedUser = User.Create("test@example.com", "testuser", "hash");
    bannedUser.Ban(DateTime.UtcNow.AddDays(30), "Spam");
    
    mockUnitOfWork.Setup(x => x.Repository<User>().GetByIdAsync(It.IsAny<string>()))
        .ReturnsAsync(bannedUser);
    
    // Act
    var result = await service.CanUserReviewCompanyAsync("userId", "companyId", CommentType.Employee);
    
    // Assert
    Assert.That(result, Is.False);
}
```

### Integration Test

```csharp
[Test]
[IntegrationTest]
public async Task CalculateRiskScore_Should_Consider_All_Factors()
{
    // Real database, real calculations
    var service = new CompanyDomainService(_realUnitOfWork);
    
    var riskScore = await service.CalculateRiskScoreAsync(_testCompanyId);
    
    Assert.That(riskScore.TotalScore, Is.InRange(0, 100));
    Assert.That(riskScore.RiskFactors, Has.Count.GreaterThan(0));
}
```

## 📊 Service İstatistikleri

### En Kompleks Service'ler

1. **ContentModerationService**: 15+ method, AI integration
2. **ReviewDomainService**: 12+ method, kompleks iş kuralları
3. **UserDomainService**: 10+ method, çoklu hesaplama

### En Çok Kullanılan Service'ler

1. **ReviewValidationService**: Her yorum oluşturma/güncelleme
2. **UserDomainService**: Her kullanıcı işlemi
3. **ReviewScoringService**: Her yorum sonrası

### Performans Kritik Service'ler

1. **ReviewAnalyticsService**: Büyük veri işleme
2. **CompanyDomainService**: İstatistik hesaplamaları
3. **ContentModerationService**: AI model çağrıları

## 🚨 Dikkat Edilmesi Gerekenler

### 1. Transaction Boundaries

```csharp
// Service method'ları transaction boundary olmamalı
// Transaction yönetimi Application layer'da
public async Task UpdateCompanyStatisticsAsync(string companyId)
{
    // Hesaplamalar yap
    // Entity'yi güncelle
    // Event yayınla
    // Transaction commit Application layer'da!
}
```

### 2. Performance Considerations

```csharp
// N+1 problem'den kaçının
var reviews = await _unitOfWork.Repository<Review>()
    .GetAsync(r => r.CompanyId == companyId, 
        includeProperties: "User,Votes"); // Eager loading
```

### 3. Circular Dependencies

```csharp
// Domain service'ler birbirini çağırmamalı
// Ortak logic varsa ayrı bir service'e taşıyın
```

## 📈 Gelecek İyileştirmeler

1. **Caching**: Sık hesaplanan skorlar için
2. **Batch Processing**: Toplu badge kontrolü
3. **Event Sourcing**: Audit trail için
4. **Machine Learning**: Daha iyi spam detection
5. **Real-time Analytics**: Canlı trend analizi

---

*Bu dokümantasyon, Services klasöründeki domain service'lerinin detaylı açıklamasını içerir. Domain service'ler,
kompleks iş kurallarını yönetir ve entity'ler arasındaki koordinasyonu sağlar. Clean Architecture prensipleri
doğrultusunda, infrastructure concern'lerden bağımsız olarak tasarlanmışlardır.*