# Domain Exception Kullanım Kılavuzu

## Exception Hiyerarşisi

### Base Exception

- **DomainException**: Tüm domain exception'ları için abstract base class
    - Özellikler: `OccurredOn`, `Context`, `TraceId`, `Severity`, `UserMessageKey`
    - Fluent metodlar: `WithContext()`, `WithSeverity()`, `WithUserMessageKey()`

### Temel Exception'lar

#### BusinessRuleException

**Ne zaman kullanılır**: Tüm iş kuralı ihlalleri için

```csharp
// Basit kullanım
throw new BusinessRuleException("Kullanıcı kendi şirketini yorumlayamaz");

// Hata kodu ile
throw new BusinessRuleException("SELF_REVIEW_NOT_ALLOWED", "Kullanıcı kendi şirketini yorumlayamaz");

// Entity context ile
throw new BusinessRuleException("DUPLICATE_REVIEW", "Kullanıcı bu şirketi zaten yorumlamış", "Review", userId)
    .WithRule("OneReviewPerUserPerCompany");
```

#### EntityNotFoundException

**Ne zaman kullanılır**: Bir entity ID veya kriterlerle bulunamadığında

```csharp
// ID ile
throw new EntityNotFoundException("Company", companyId);

// Kriterler ile
throw new EntityNotFoundException("User", new Dictionary<string, object?> 
{
    ["Email"] = email,
    ["IsActive"] = true
});

// Birden fazla entity
throw EntityNotFoundException.ForMultiple("Review", missingReviewIds);
```

#### InvalidDomainStateException

**Ne zaman kullanılır**: Entity yanlış state'de olduğunda

```csharp
throw new InvalidDomainStateException("Company", company.Id, "Inactive", "AddReview")
    .WithStateDetails(new Dictionary<string, object?>
    {
        ["DeactivatedAt"] = company.DeactivatedAt,
        ["Reason"] = company.DeactivationReason
    });
```

### Özelleşmiş Exception'lar

#### ValidationException

**Ne zaman kullanılır**: Birden fazla hata içeren karmaşık validasyon senaryoları için

```csharp
// Tek property
throw DomainValidationException.ForProperty("Email", "Geçersiz email formatı", email);

// Birden fazla property
throw DomainValidationException.ForMultipleProperties(
    ("Email", "Geçersiz format"),
    ("PhoneNumber", "Zorunlu alan")
);
```

#### ExternalServiceException

**Ne zaman kullanılır**: Dış servis entegrasyon hataları için

```csharp
throw new ExternalServiceException(
    "VergiNumarasiAPI", 
    "VergiNumarasiDogrula", 
    "Servis kullanılamıyor",
    httpStatusCode: 503,
    isRetryable: true,
    retryAfter: TimeSpan.FromSeconds(30)
);
```

## Hata Kodu Konvansiyonları

### İş Kuralları

- `DUPLICATE_[ENTITY]`: Entity zaten mevcut
- `[ENTITY]_NOT_FOUND`: Entity bulunamadı
- `INVALID_[FIELD]`: Alan validasyonu başarısız
- `[ACTION]_NOT_ALLOWED`: İşleme izin verilmiyor
- `LIMIT_EXCEEDED`: Oran/sayı limiti aşıldı

### State İhlalleri

- `INVALID_STATE_TRANSITION`: State geçişine izin verilmiyor
- `ENTITY_NOT_ACTIVE`: Entity aktif değil
- `OPERATION_TIMEOUT`: İşlem çok uzun sürdü

### Entegrasyon

- `EXTERNAL_SERVICE_ERROR`: Dış servis hatası
- `INTEGRATION_TIMEOUT`: Dış servis zaman aşımı
- `INVALID_RESPONSE`: Dış servis geçersiz veri döndü

## Önem Derecesi Kılavuzu

- **Critical (Kritik)**: Sistemi bozan, veri bütünlüğü riski taşıyan hatalar
- **High (Yüksek)**: Önemli özellik hataları, güvenlik ihlalleri
- **Medium (Orta)**: İş kuralı ihlalleri, validasyon hataları
- **Low (Düşük)**: Küçük sorunlar, uyarılar

## En İyi Uygulamalar

1. **Çoğu domain ihlali için BusinessRuleException kullanın**
   ```csharp
   throw new BusinessRuleException("REVIEW_LIMIT_EXCEEDED", 
       "Kullanıcı günlük yorum limitine ulaştı")
       .WithSeverity(ExceptionSeverity.Low);
   ```

2. **Debug için context ekleyin**
   ```csharp
   throw new EntityNotFoundException("Company", companyId)
       .WithContext("UserId", userId)
       .WithContext("Operation", "CreateReview");
   ```

3. **Lokalizasyon için user message key kullanın**
   ```csharp
   throw new BusinessRuleException("INVALID_RATING", "Puan 1-5 arasında olmalıdır")
       .WithUserMessageKey("ERROR_INVALID_RATING_RANGE");
   ```

4. **Gerçekten gerekli olmadıkça yeni exception tipi oluşturmayın**
    - Çoğu senaryo uygun hata kodları ile BusinessRuleException kullanabilir
    - Sadece altyapı sorunları veya özel işlem gereksinimleri için yeni tipler oluşturun

5. **Her zaman ilgili entity ID'lerini ve işlem context'ini ekleyin**
   ```csharp
   throw new InvalidDomainStateException("Review", reviewId, "Hidden", "Edit")
       .WithContext("UserId", userId)
       .WithContext("HiddenAt", review.HiddenAt);
   ```

## Kullanım Örnekleri

### Yorum Oluşturma Senaryosu

```csharp
// Kullanıcı kendi şirketini yorumlama kontrolü
if (user.Id == company.OwnerId)
{
    throw new BusinessRuleException("SELF_REVIEW_NOT_ALLOWED", 
        "Kendi şirketinizi yorumlayamazsınız")
        .WithSeverity(ExceptionSeverity.Medium)
        .WithContext("UserId", user.Id)
        .WithContext("CompanyId", company.Id);
}

// Şirket aktiflik kontrolü
if (!company.IsActive)
{
    throw new InvalidDomainStateException("Company", company.Id, "Inactive", "CreateReview")
        .WithContext("DeactivatedAt", company.DeactivatedAt)
        .WithUserMessageKey("COMPANY_NOT_ACTIVE_FOR_REVIEW");
}

// Günlük limit kontrolü
if (dailyReviewCount >= 5)
{
    throw new BusinessRuleException("DAILY_REVIEW_LIMIT_EXCEEDED",
        "Günlük maksimum 5 yorum yapabilirsiniz")
        .WithSeverity(ExceptionSeverity.Low)
        .WithContext("CurrentCount", dailyReviewCount)
        .WithContext("Limit", 5);
}
```

### Şirket Doğrulama Senaryosu

```csharp
try
{
    var result = await taxNumberService.ValidateAsync(company.TaxId);
    if (!result.IsValid)
    {
        throw new BusinessRuleException("INVALID_TAX_NUMBER",
            "Vergi numarası doğrulanamadı")
            .WithContext("TaxId", company.TaxId)
            .WithContext("ValidationErrors", result.Errors);
    }
}
catch (HttpRequestException ex)
{
    throw new ExternalServiceException(
        "VergiDairesiAPI",
        "ValidateTaxNumber", 
        "Vergi dairesi servisi yanıt vermiyor",
        httpStatusCode: 503,
        isRetryable: true,
        retryAfter: TimeSpan.FromMinutes(1)
    );
}
```