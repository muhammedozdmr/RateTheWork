# Domain Layer Interfaces Kılavuzu

## Yapılan Değişiklikler

### 1. Repository Pattern İyileştirmeleri

#### Önceki Durum:

- `IBaseRepository<T>` - Tek bir interface'de 20+ metod (ISP ihlali)

#### Yeni Durum:

- `IReadRepository<T>` - Sadece okuma işlemleri
- `IWriteRepository<T>` - Sadece yazma işlemleri
- `IRepository<T>` - IReadRepository + IWriteRepository birleşimi
- `IBaseRepository<T>` - Geriye dönük uyumluluk için (deprecated olarak işaretlenecek)

### 2. Unit of Work Pattern İyileştirmesi

#### Önceki Durum:

```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICompanyRepository Companies { get; }
    // ... her repository için property
}
```

#### Yeni Durum:

```csharp
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    TRepository GetCustomRepository<TRepository>() where TRepository : class;
}
```

### 3. Yeni Repository Interface'leri

- `INotificationRepository` - Bildirim işlemleri
- `IBadgeRepository` - Rozet işlemleri
- `IReportRepository` - Şikayet işlemleri
- `ICompanyBranchRepository` - Şube işlemleri

### 4. Domain Service Refactoring

#### IReviewDomainService Parçalandı:

- `IReviewValidationService` - Validasyon işlemleri
- `IReviewScoringService` - Skorlama işlemleri
- `IReviewAnalyticsService` - Analitik işlemleri
- `IReviewDomainService` - Koordinasyon için ana interface (opsiyonel)

### 5. Infrastructure Interface'leri

**TAŞINMASI GEREKENLER** (Domain'de olmamalı):

- `/Infrastructure/` klasöründeki tüm interface'ler
- `IEmailService`, `ISmsService`, `IPushNotificationService` → Application Layer
- `ICacheService`, `IDistributedLockService`, `IMetricsService` → Infrastructure Layer

### 6. Event System İyileştirmeleri

- Duplicate `IDomainEvent` kaldırıldı
- `DomainEventBase` abstract record oluşturuldu
- Event'ler için standart properties: EventId, OccurredOn, Version

## Interface Organizasyon Kuralları

### 1. Domain Layer'da Olması Gerekenler:

- Repository interfaces (domain aggregate'leri için)
- Domain service interfaces
- Domain policy interfaces
- Specification interfaces
- Value object interfaces
- Domain event interfaces

### 2. Domain Layer'da OLMAMASI Gerekenler:

- Infrastructure service interfaces (cache, email, sms, vb.)
- Application service interfaces
- External system integration interfaces
- UI/Presentation layer interfaces

### 3. Interface Tasarım Prensipleri:

- **Single Responsibility**: Her interface tek bir sorumluluğa sahip olmalı
- **Interface Segregation**: Büyük interface'ler parçalanmalı
- **Dependency Inversion**: Domain diğer katmanlara bağımlı olmamalı
- **Open/Closed**: Interface'ler extension'a açık, modification'a kapalı olmalı

## Kullanım Örnekleri

### Repository Kullanımı:

```csharp
// Okuma işlemi
IReadRepository<Company> readRepo = ...;
var company = await readRepo.GetByIdAsync(id);

// Yazma işlemi
IWriteRepository<Company> writeRepo = ...;
await writeRepo.UpdateAsync(company);

// Hem okuma hem yazma
IRepository<Company> repo = ...;
var company = await repo.GetByIdAsync(id);
company.UpdateInfo(...);
await repo.UpdateAsync(company);
```

### Unit of Work Kullanımı:

```csharp
// Generic repository
var userRepo = unitOfWork.Repository<User>();

// Custom repository
var companyRepo = unitOfWork.GetCustomRepository<ICompanyRepository>();

// Transaction
await unitOfWork.BeginTransactionAsync();
try
{
    // operations...
    await unitOfWork.SaveChangesAsync();
    await unitOfWork.CommitTransactionAsync();
}
catch
{
    await unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### Domain Service Kullanımı:

```csharp
// Validasyon
bool canReview = await reviewValidationService.CanUserReviewCompanyAsync(userId, companyId, type);

// Skorlama
var qualityScore = await reviewScoringService.CalculateReviewQualityAsync(reviewId);

// Analitik
var trends = await reviewAnalyticsService.AnalyzeReviewTrendsAsync(companyId, start, end);
```

## Migration Planı

1. **Aşama 1**: Yeni interface'leri implement et
2. **Aşama 2**: Mevcut kodu yeni interface'lere migrate et
3. **Aşama 3**: Eski interface'leri deprecate olarak işaretle
4. **Aşama 4**: Infrastructure interface'lerini ilgili katmanlara taşı
5. **Aşama 5**: Eski interface'leri kaldır