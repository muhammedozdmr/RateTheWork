# RateTheWork.Infrastructure

Bu katman, RateTheWork uygulamasının altyapı implementasyonlarını içerir. Clean Architecture prensiplerine uygun olarak,
bu katman dış sistemlerle entegrasyon ve cross-cutting concerns'leri handle eder.

## 📁 Klasör Yapısı

```
RateTheWork.Infrastructure/
├── Interfaces/                 # Infrastructure interface'leri
│   ├── IBackgroundJobService.cs
│   ├── ICacheService.cs
│   ├── IDistributedLockService.cs
│   ├── IFeatureFlagService.cs
│   └── IMetricsService.cs
├── Implementations/            # Interface implementasyonları
├── Services/                   # Infrastructure service'leri
├── Cache/                      # Cache implementasyonları
├── Jobs/                       # Background job implementasyonları
├── Metrics/                    # Metrics ve monitoring
├── Configuration/              # Configuration sınıfları
└── README.md
```

## 🔧 Sorumluluklar

### Cache (Önbellek)

- Redis implementasyonu
- In-memory cache
- Cache stratejileri ve expiration policies

### Background Jobs (Arkaplan İşleri)

- Hangfire implementasyonu
- Scheduled jobs
- Recurring jobs
- Job monitoring

### Metrics (Metrikler)

- Application metrics
- Performance monitoring
- Health checks
- Logging integration

### Feature Flags (Özellik Bayrakları)

- Feature toggle implementasyonu
- A/B testing support
- Configuration management

### Distributed Lock (Dağıtık Kilit)

- Redis distributed lock
- Concurrency control
- Resource synchronization

## 🚀 Kullanım

Bu projedeki interface'ler Application katmanında kullanılır ve Dependency Injection container aracılığıyla inject
edilir.

```csharp
// Cache kullanımı
await _cacheService.SetAsync("key", value, TimeSpan.FromMinutes(30));
var cachedValue = await _cacheService.GetAsync<MyType>("key");

// Background job
await _backgroundJobService.EnqueueAsync<IMyService>(x => x.ProcessAsync());

// Metrics
_metricsService.IncrementCounter("user.login.success");
_metricsService.RecordGauge("active.users", activeUserCount);
```

## 📦 Bağımlılıklar

- **Microsoft.Extensions.*** - .NET Core DI ve Configuration
- **StackExchange.Redis** - Redis client
- **Hangfire** - Background job processing
- **System.Diagnostics.DiagnosticSource** - Metrics ve monitoring

## 🔒 Güvenlik

- Sensitive data encryption
- API key management
- Connection string security
- Rate limiting

## 📊 Monitoring

- Application metrics
- Performance counters
- Error tracking
- Health checks

## 🧪 Test Stratejisi

- Unit tests için mock implementasyonlar
- Integration tests için test containers
- Performance tests

Bu katman, uygulamanın external dependencies'lerini abstract eder ve değiştirilebilir implementasyonlar sağlar.