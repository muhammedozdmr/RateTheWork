# Infrastructure Interfaces Taşınma Notu

Bu klasördeki tüm interface'ler Clean Architecture prensiplerine göre Domain katmanında bulunmamalıdır.

## Taşınması Gereken Interface'ler:

### Application Katmanına:

- `IEmailService` - Application use case'lerde kullanılacak
- `ISmsService` - Application use case'lerde kullanılacak
- `IPushNotificationService` - Application use case'lerde kullanılacak
- `ILocalizationService` - Application seviyesinde kullanılacak

### Infrastructure Katmanına:

- `IBackgroundJobService` - Pure infrastructure concern
- `ICacheService` - Pure infrastructure concern
- `IDistributedLockService` - Pure infrastructure concern
- `IFeatureFlagService` - Pure infrastructure concern
- `IMetricsService` - Pure infrastructure concern

## Neden Taşınmalı?

Domain katmanı:

- İş kurallarını ve domain mantığını içermelidir
- Dış sistemlerden bağımsız olmalıdır
- Infrastructure detaylarını bilmemelidir
- Sadece domain konseptlerini içermelidir

Bu interface'ler infrastructure veya application katmanlarına aittir çünkü dış sistemlerle (email, SMS, cache, vb.)
ilgilidirler.