# RateTheWork Domain Katmanı

## 🎯 Genel Bakış

RateTheWork Domain katmanı, uygulamanın iş mantığının kalbini oluşturur. Domain-Driven Design (DDD) prensipleri ve Clean
Architecture yaklaşımı ile tasarlanmıştır. Bu katman, uygulamanın iş kurallarını, entity'leri, value object'leri ve
domain servislerini içerir.

## 📂 Klasör Yapısı

```
RateTheWork.Domain/
├── Common/              # Temel sınıflar ve ortak kullanılan yapılar
├── Constants/           # Domain sabitleri
├── Entities/            # Domain entity'leri
├── Enums/              # Domain enum'ları
├── Events/             # Domain event'leri
├── Exceptions/         # Domain'e özel exception'lar
├── Extensions/         # Extension method'lar
├── Interfaces/         # Domain interface'leri
├── Services/           # Domain servisleri
├── Specifications/     # Specification pattern implementasyonları
└── ValueObjects/       # Value object'ler
```

## 🏗️ Temel Kavramlar

### Domain-Driven Design (DDD)

Bu proje DDD yaklaşımını benimser:

- **Entity**: Benzersiz kimliğe sahip domain nesneleri
- **Value Object**: Kimliksiz, değer bazlı nesneler
- **Aggregate**: İlişkili entity'lerin grubu
- **Domain Service**: Entity'lere ait olmayan iş mantığı
- **Domain Event**: Domain'de gerçekleşen önemli olaylar
- **Specification**: İş kurallarını temsil eden yapılar

### Clean Architecture

Domain katmanı, dış katmanlara bağımlı değildir:

- Framework bağımsızdır
- Veritabanı bağımsızdır
- UI bağımsızdır
- Sadece iş mantığına odaklanır

## 💡 Kullanım Prensipleri

### 1. Entity Oluşturma

```csharp
// Doğru: Factory method kullanımı
var review = Review.Create(userId, companyId, content, rating);

// Yanlış: Direkt constructor kullanımı
var review = new Review(); // ❌
```

### 2. Value Object Kullanımı

```csharp
// Doğru: Immutable value object
var email = Email.Create("user@example.com");

// Yanlış: Mutable property
email.Value = "new@example.com"; // ❌
```

### 3. Domain Service Kullanımı

```csharp
// Doğru: Kompleks iş mantığı için domain service
var score = await reviewScoringService.CalculateQualityScoreAsync(review);

// Yanlış: Entity içinde external bağımlılık
review.CalculateScore(externalService); // ❌
```

### 4. Specification Pattern

```csharp
// Doğru: Reusable business rule
var activeCompanies = await repository.GetAsync(new ActiveCompanySpecification());

// Yanlış: Inline business logic
var activeCompanies = await repository.GetAsync(c => c.IsActive && !c.IsDeleted); // ❌
```

## 🔧 Temel Özellikler

### Entity Yaşam Döngüsü

1. **Oluşturma**: Factory method'lar ile
2. **Güncelleme**: Domain method'lar ile
3. **Doğrulama**: Business rule exception'lar ile
4. **Event Yayınlama**: Domain event'ler ile

### Audit Trail

- Tüm entity'ler otomatik audit bilgisi tutar
- CreatedAt, ModifiedAt, CreatedBy, ModifiedBy
- Soft delete desteği

### Domain Events

- Entity state değişimlerini takip eder
- Event sourcing için altyapı sağlar
- Eventual consistency desteği

### Business Rule Validation

- Entity invariant'larını korur
- Anlamlı exception mesajları
- Domain'e özel validation kuralları

## 📋 Domain Katmanı Kuralları

### ✅ Yapılması Gerekenler

- Factory pattern kullanın
- Immutable value object'ler oluşturun
- Business logic'i domain'de tutun
- Anlamlı domain exception'lar fırlatın
- Unit test yazın

### ❌ Yapılmaması Gerekenler

- External dependency enjekte etmeyin
- Framework'e bağımlı kod yazmayın
- Anemic domain model oluşturmayın
- Data annotation kullanmayın
- Infrastructure concern'leri dahil etmeyin

## 🧪 Test Edilebilirlik

Domain katmanı tamamen test edilebilir olmalıdır:

- Pure function'lar tercih edin
- Side effect'leri minimize edin
- Dependency injection kullanın
- Mock'lanabilir interface'ler tasarlayın

## 📊 Domain İstatistikleri

- **Entity Sayısı**: 11
- **Value Object Sayısı**: 30+
- **Domain Service Sayısı**: 9
- **Specification Sayısı**: 20+
- **Domain Event Sayısı**: 40+
- **Custom Exception Sayısı**: 50+

## 🔍 Detaylı Dokümantasyon

Her alt klasör için detaylı README dosyaları mevcuttur:

- [Common/](./Common/README.md) - Temel sınıflar
- [Constants/](./Constants/README.md) - Domain sabitleri
- [Entities/](./Entities/README.md) - Domain entity'leri
- [Enums/](./Enums/README.md) - Enumeration'lar
- [Events/](./Events/README.md) - Domain event'leri
- [Exceptions/](./Exceptions/README.md) - Exception handling
- [Extensions/](./Extensions/README.md) - Extension method'lar
- [Interfaces/](./Interfaces/README.md) - Domain interface'leri
- [Services/](./Services/README.md) - Domain servisleri
- [Specifications/](./Specifications/README.md) - Specification pattern
- [ValueObjects/](./ValueObjects/README.md) - Value object'ler

## 🚀 Başlangıç

1. Domain katmanını anlamak için önce [Common/](./Common/README.md) klasörünü inceleyin
2. Entity yaşam döngüsünü [Entities/](./Entities/README.md) dokümantasyonundan öğrenin
3. Value Object pattern'ini [ValueObjects/](./ValueObjects/README.md) bölümünden anlayın
4. Domain service kullanımını [Services/](./Services/README.md) kısmından öğrenin

## 📞 İletişim ve Katkı

Domain katmanı ile ilgili sorularınız veya önerileriniz için:

- Issue açabilirsiniz
- Pull request gönderebilirsiniz
- Dokümantasyonu geliştirebilirsiniz

---

*Bu dokümantasyon, RateTheWork Domain katmanının genel yapısını ve kullanım prensiplerini açıklar. Detaylı bilgi için
ilgili alt klasör dokümantasyonlarını inceleyiniz.*