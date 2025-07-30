# RateTheWork.Infrastructure Katmanı

Bu katman, RateTheWork uygulamasının altyapı implementasyonlarını içerir. Clean Architecture prensiplerine uygun olarak,
bu katman dış sistemlerle entegrasyon ve cross-cutting concerns'leri handle eder.

## 📁 Klasör Yapısı

```
RateTheWork.Infrastructure/
├── Cache/                      # Önbellek yönetimi
│   ├── CacheInvalidator.cs     # Cache invalidation stratejileri
│   └── CachePolicies.cs        # Cache policy tanımlamaları
├── Configuration/              # Yapılandırma seçenekleri
│   ├── CacheOptions.cs         # Cache ayarları
│   ├── CloudflareOptions.cs    # Cloudflare KV ayarları
│   ├── EmailOptions.cs         # SendGrid e-posta ayarları
│   ├── FirebaseOptions.cs      # Firebase push notification ayarları
│   ├── SecurityOptions.cs      # Güvenlik ayarları
│   ├── SerilogConfiguration.cs # Loglama yapılandırması
│   ├── SmsOptions.cs           # Twilio SMS ayarları
│   └── StorageOptions.cs       # Dosya depolama ayarları
├── HealthChecks/               # Sağlık kontrolleri
│   ├── CloudflareKVHealthCheck.cs    # Cloudflare KV kontrolü
│   ├── DatabaseMigrationHealthCheck.cs # DB migration kontrolü
│   ├── DatabaseWriteHealthCheck.cs    # DB yazma kontrolü
│   ├── HealthCheckResponseWriter.cs   # Health check yanıt formatı
│   └── RedisHealthCheck.cs            # Redis bağlantı kontrolü
├── Interfaces/                 # Infrastructure interface'leri
│   ├── IBackgroundJobService.cs       # Arka plan görev yönetimi
│   ├── ICacheService.cs               # Önbellek servisi
│   ├── IDistributedLockService.cs     # Dağıtık kilit servisi
│   ├── IFeatureFlagService.cs         # Özellik bayrakları
│   └── IMetricsService.cs             # Metrik toplama
├── Jobs/                       # Arka plan görevleri
│   ├── DataCleanupJob.cs       # Veri temizleme görevleri
│   ├── EmailQueueJob.cs        # E-posta kuyruğu işleyici
│   ├── JobScheduler.cs         # Görev zamanlayıcı
│   └── ReportGenerationJob.cs  # Rapor oluşturma görevleri
├── Metrics/                    # Metrik ve izleme
│   ├── MetricsMiddleware.cs    # HTTP metrik middleware'i
│   └── OpenTelemetryConfiguration.cs  # OpenTelemetry yapılandırması
├── Migrations/                 # Entity Framework migration'ları
├── Persistence/                # Veri erişim katmanı
│   ├── ApplicationDbContext.cs # Ana DbContext
│   ├── Configurations/         # Entity yapılandırmaları
│   ├── Interceptors/           # EF Core interceptor'ları
│   └── Repositories/           # Repository implementasyonları
├── Services/                   # Servis implementasyonları
│   ├── CloudflareKVService.cs          # Cloudflare KV secret yönetimi
│   ├── CurrentUserService.cs           # Mevcut kullanıcı bilgileri
│   ├── DateTimeService.cs              # Tarih/saat servisi
│   ├── DistributedLockService.cs       # Dağıtık kilit yönetimi
│   ├── EncryptionService.cs            # Şifreleme servisi (AES-256)
│   ├── FeatureFlagService.cs           # Özellik bayrakları yönetimi
│   ├── FirebasePushNotificationService.cs # Push bildirim servisi
│   ├── HangfireBackgroundJobService.cs  # Hangfire entegrasyonu
│   ├── InMemoryCacheService.cs         # Bellek içi önbellek
│   ├── LocalFileStorageService.cs       # Yerel dosya depolama
│   ├── MetricsService.cs                # Metrik toplama servisi
│   ├── PasswordHashingService.cs        # BCrypt şifre hashleme
│   ├── RateLimitingService.cs           # API rate limiting
│   ├── RedisCacheService.cs             # Redis önbellek servisi
│   ├── SendGridEmailService.cs          # SendGrid e-posta servisi
│   ├── TcIdentityValidationService.cs   # TC Kimlik doğrulama
│   └── TwilioSmsService.cs              # Twilio SMS servisi
└── DependencyInjection.cs      # DI yapılandırması
```

## 🔧 Temel Özellikler

### 1. Veri Erişim Katmanı

- **Entity Framework Core** ile PostgreSQL desteği
- **Repository Pattern** implementasyonu
- **Unit of Work Pattern** implementasyonu
- **Soft Delete** interceptor'ı
- **Auditable Entity** interceptor'ı
- **Performance** interceptor'ı

### 2. Önbellek Yönetimi

- **Redis** önbellek desteği
- **In-Memory** önbellek alternatifi
- **Cache Invalidation** stratejileri
- **Cache Policy** tanımlamaları

### 3. Arka Plan Görevleri

- **Hangfire** ile görev yönetimi
- Zamanlanmış görevler (Scheduled Jobs)
- Tekrarlayan görevler (Recurring Jobs)
- Görev kuyruğu yönetimi

### 4. Güvenlik Servisleri

- **BCrypt** ile şifre hashleme
- **AES-256** ile veri şifreleme
- **TC Kimlik** doğrulama servisi
- **Rate Limiting** servisi

### 5. Harici Servisler

- **SendGrid** e-posta servisi
- **Twilio** SMS servisi
- **Firebase** push notification servisi
- **Cloudflare KV** secret yönetimi

### 6. Metrik ve İzleme

- **OpenTelemetry** entegrasyonu
- HTTP istek metrikleri
- Performans izleme
- Sağlık kontrolleri

## 🚀 Kullanım Örnekleri

### Önbellek Kullanımı

```csharp
// Veri önbellekleme
await _cacheService.SetAsync("user:123", userData, TimeSpan.FromMinutes(30));

// Önbellekten veri okuma
var cachedUser = await _cacheService.GetOrCreateAsync<User>(
    "user:123",
    async () => await _userRepository.GetByIdAsync("123"),
    TimeSpan.FromMinutes(30)
);

// Önbellek temizleme
await _cacheService.RemoveAsync("user:123");
```

### E-posta Gönderimi
```csharp
// Basit e-posta
await _emailService.SendEmailAsync(
    to: "user@example.com",
    subject: "Hoş Geldiniz",
    body: "RateTheWork'e hoş geldiniz!",
    isHtml: true
);

// Template e-posta
await _emailService.SendTemplatedEmailAsync(
    to: "user@example.com",
    templateName: "welcome",
    templateData: new { Name = "Ahmet", CompanyName = "TechLiberty" }
);
```

### SMS Gönderimi

```csharp
// SMS gönderimi
bool success = await _smsService.SendSmsAsync(
    phoneNumber: "+905551234567",
    message: "Doğrulama kodunuz: 123456"
);

// Toplu SMS
var results = await _smsService.SendBulkSmsAsync(messages);
```

### Arka Plan Görevi

```csharp
// Anında görev
await _backgroundJobService.EnqueueAsync<IEmailService>(
    x => x.SendEmailAsync(to, subject, body, true)
);

// Zamanlanmış görev
await _backgroundJobService.ScheduleAsync<IReportService>(
    x => x.GenerateMonthlyReportAsync(companyId),
    TimeSpan.FromDays(30)
);

// Tekrarlayan görev
await _backgroundJobService.RecurringJobAsync<IDataCleanupService>(
    "daily-cleanup",
    x => x.CleanupOldDataAsync(),
    Cron.Daily
);
```

### Rate Limiting

```csharp
// API rate limit kontrolü
var result = await _rateLimitingService.CheckRateLimitAsync(
    key: $"api:{userId}",
    limit: 100,
    period: TimeSpan.FromMinutes(1)
);

if (!result.IsAllowed)
{
    throw new RateLimitExceededException($"Limit aşıldı. {result.ResetAt} tarihinde tekrar deneyin.");
}
```

### Dağıtık Kilit

```csharp
// Kritik bölge için kilit
await using var lockHandle = await _distributedLockService.AcquireAsync(
    key: $"process:order:{orderId}",
    timeout: TimeSpan.FromSeconds(30)
);

if (lockHandle != null)
{
    // Kritik işlemler
    await ProcessOrderAsync(orderId);
}
```

## 📦 Kullanılan Teknolojiler

- **Entity Framework Core 8.0** - ORM
- **PostgreSQL** - Veritabanı
- **Redis** - Önbellek ve dağıtık kilit
- **Hangfire** - Arka plan görevleri
- **SendGrid** - E-posta servisi
- **Twilio** - SMS servisi
- **Firebase** - Push notification
- **Cloudflare KV** - Secret yönetimi
- **OpenTelemetry** - Metrik ve izleme
- **Serilog** - Loglama

## 🔒 Güvenlik Özellikleri

- Hassas verilerin şifrelenmesi (AES-256)
- API anahtarlarının güvenli saklanması (Cloudflare KV)
- Bağlantı dizelerinin güvenliği
- Rate limiting ile DDoS koruması
- SQL Injection koruması (EF Core)

## 📊 Performans Optimizasyonları

- Veritabanı sorgu optimizasyonu
- Önbellek stratejileri
- Asenkron işlemler
- Connection pooling
- Lazy loading kontrolü

## 🧪 Test Desteği

- Mock implementasyonlar için interface'ler
- Integration test için test container desteği
- In-memory database desteği
- Fake service implementasyonları

## 🏗️ Railway Deployment Ayarları

Railway'de deployment için gerekli ortam değişkenleri:

```env
# Veritabanı
DATABASE_URL=postgresql://user:pass@host:port/db

# Redis (Opsiyonel)
REDIS_CONNECTION_STRING=redis://default:pass@host:port

# SendGrid
SENDGRID_API_KEY=SG.xxxx
SENDGRID_FROM_EMAIL=noreply@example.com
SENDGRID_FROM_NAME=RateTheWork

# Twilio
TWILIO_ACCOUNT_SID=ACxxxx
TWILIO_AUTH_TOKEN=xxxx
TWILIO_FROM_NUMBER=+1234567890

# Cloudflare KV
CLOUDFLARE_ACCOUNT_ID=xxxx
CLOUDFLARE_KV_NAMESPACE_ID=xxxx
CLOUDFLARE_API_TOKEN=xxxx

# Firebase (Opsiyonel)
FIREBASE_PROJECT_ID=xxxx
FIREBASE_PRIVATE_KEY=xxxx
FIREBASE_CLIENT_EMAIL=xxxx
```

## 📝 Notlar

- Bu katman, Application katmanındaki interface'lerin concrete implementasyonlarını sağlar
- Domain katmanına bağımlılığı vardır ancak Domain katmanı Infrastructure'a bağımlı değildir
- Tüm external dependency'ler bu katmanda izole edilir
- Yeni bir external servis eklenecekse, önce Application katmanında interface tanımlanmalıdır