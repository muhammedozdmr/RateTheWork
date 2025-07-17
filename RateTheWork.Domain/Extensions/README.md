# Domain Extensions Katmanı

Bu klasör, Domain katmanı boyunca kullanılan extension metodlarını içerir. Extension metodlar, mevcut sınıflara yeni
işlevsellik eklememizi sağlar ve kodun daha okunabilir ve yeniden kullanılabilir olmasını sağlar.

## İçindekiler

### 1. EntityExtensions.cs

Domain entity'leri için yardımcı extension metodlar sağlar.

#### Özellikler:

- **User Extensions**:
    - `GetFullName()`: Kullanıcının tam adını döndürür
    - `GetAnonymousDisplayName()`: Anonimlik seviyesine göre görüntülenecek adı döndürür
    - `GetAccountAgeInDays()`: Hesap yaşını gün olarak hesaplar
    - `IsFullyVerified()`: Kullanıcının tam doğrulanmış olup olmadığını kontrol eder

- **Review Extensions**:
    - `GetNetVotes()`: Net beğeni sayısını hesaplar (upvotes - downvotes)
    - `GetPopularityScore()`: Yorumun popülerlik skorunu hesaplar
    - `GetAgeInDays()`: Yorumun yaşını gün olarak döndürür
    - `GetReliabilityBadge()`: Yorumun güvenilirlik rozetini belirler

- **Company Extensions**:
    - `GetAverageRating()`: Şirketin ortalama puanını döndürür
    - `GetEmployeeSizeCategory()`: Çalışan sayısına göre kategori belirler
    - `GetCompanyAgeInYears()`: Şirketin yaşını yıl olarak hesaplar
    - `GetTrustLevel()`: Güvenilirlik seviyesini döndürür

- **Badge Extensions**:
    - `IsActive()`: Rozetin aktif olup olmadığını kontrol eder
    - `GetRemainingDays()`: Kalan geçerlilik süresini hesaplar

- **Common Extensions**:
    - `GetAge<T>()`: Entity'nin yaşını hesaplar
    - `IsCreatedToday<T>()`: Bugün oluşturulup oluşturulmadığını kontrol eder
    - `IsCreatedThisWeek<T>()`: Bu hafta oluşturulup oluşturulmadığını kontrol eder
    - `IsCreatedThisMonth<T>()`: Bu ay oluşturulup oluşturulmadığını kontrol eder

### 2. EnumExtensions.cs

Enum'lar için güçlü extension metodlar sağlar.

#### Özellikler:

- `GetDescription()`: Enum'un Description attribute değerini döndürür
- `GetDisplayName()`: Display name'i döndürür
- `GetValues<T>()`: Tüm enum değerlerini liste olarak döndürür
- `ToDictionary<T>()`: Enum'u dictionary'e çevirir
- `ToSelectList<T>()`: SelectList item'larına çevirir
- `Parse<T>()`: String'den enum'a güvenli dönüşüm
- `HasFlag<T>()`: Flag kontrolü
- `AddFlag<T>()` / `RemoveFlag<T>()`: Flag yönetimi
- `IsValid<T>()`: Geçerlilik kontrolü
- `ToInt<T>()` / `FromInt<T>()`: Int dönüşümleri

### 3. QueryableExtensions.cs

LINQ sorguları için yardımcı extension metodlar sağlar.

#### Özellikler:

- `WhereActive<T>()`: Aktif kayıtları filtreler
- `WhereDateRange<T>()`: Tarih aralığına göre filtreler
- `WhereLastDays<T>()`: Son N günün kayıtlarını getirir
- `OrderByUpdated<T>()`: Güncelleme tarihine göre sıralar
- `Paginate<T>()`: Sayfalama uygular
- `WhereIf<T>()`: Koşullu Where uygular
- `OrderByDynamic<T>()`: Dinamik sıralama
- `IncludeMultiple<T>()`: Çoklu include işlemi
- `DistinctBy<T>()`: Property'ye göre distinct
- `WhereNotDeleted<T>()`: Soft delete edilmemiş kayıtları getirir
- `Search<T>()`: Full text search simülasyonu

### 4. RepositoryExtensions.cs

Repository pattern için özelleştirilmiş extension metodlar sağlar.

#### Özellikler:

- **ReviewVote Extensions**:
    - `GetUserVoteForReviewAsync()`: Kullanıcının bir yoruma verdiği oyu getirir
    - `GetUpvoteCountAsync()` / `GetDownvoteCountAsync()`: Oy sayılarını hesaplar
    - `GetUserVotesForReviewsAsync()`: Birden fazla yorum için kullanıcı oylarını getirir

- **Review Extensions**:
    - `GetReviewsByUserAsync()`: Kullanıcının yorumlarını getirir
    - `GetReviewsByCompanyAsync()`: Şirket yorumlarını getirir
    - `UpdateReviewVoteCountsAsync()`: Oy sayılarını günceller

- **Generic Repository Extensions**:
    - `GetByIdIncludingAsync<T>()`: Include ile ID'ye göre getirme
    - `ExistsAsync<T>()`: Varlık kontrolü
    - `GetLatestAsync<T>()`: En son kaydı getirir
    - `GetRecentAsync<T>()`: Son N kaydı getirir

- **Company Extensions**:
    - `GetCompaniesBySectorAsync()`: Sektöre göre şirketleri getirir
    - `GetTopRatedCompaniesAsync()`: En yüksek puanlı şirketleri getirir

- **User Extensions**:
    - `GetByEmailAsync()`: Email'e göre kullanıcı getirir
    - `IsEmailUniqueAsync()`: Email benzersizlik kontrolü
    - `GetActiveUsersAsync()`: Aktif kullanıcıları getirir

### 5. StringExtensions.cs

String manipülasyonu için kapsamlı extension metodlar sağlar.

#### Özellikler:

- `ToSlug()`: URL dostu format oluşturur
- `ToTitleCase()`: İlk harfleri büyük yapar
- `Truncate()` / `TruncateWords()`: Metin kesme işlemleri
- `MaskEmail()` / `MaskPhoneNumber()` / `MaskTcKimlikNo()`: Hassas bilgi maskeleme
- `StripHtml()`: HTML tag'lerini temizler
- `ToBase64()` / `FromBase64()`: Base64 dönüşümleri
- `RemoveWhitespace()`: Boşlukları kaldırır
- `ToCamelCase()` / `ToPascalCase()`: Naming convention dönüşümleri
- `WordCount()`: Kelime sayısını hesaplar
- `Reverse()`: Metni ters çevirir
- `SumDigits()`: Sayıları toplar
- `ToEnum<T>()`: Enum'a güvenli dönüşüm
- `IsNullOrEmpty()` / `IsNotNullOrEmpty()`: Null kontrolü

### 6. ValidationExtensions.cs

Domain doğrulamaları için extension metodlar sağlar.

#### Özellikler:

- `IsValidEmail()`: Email format kontrolü
- `IsValidPhoneNumber()`: Türkiye telefon numarası kontrolü
- `IsValidTcKimlikNo()`: TC Kimlik No algoritma kontrolü
- `IsValidVergiNo()`: Vergi numarası kontrolü
- `IsValidUrl()` / `IsValidLinkedInUrl()`: URL doğrulamaları
- `IsValidCompanyName()`: Şirket adı doğrulaması
- `IsValidUsername()`: Kullanıcı adı doğrulaması
- `IsStrongPassword()`: Güçlü şifre kontrolü
- `IsValidIban()`: IBAN format kontrolü
- `IsValidReviewContent()`: Yorum içeriği doğrulaması
- `IsValidDateRange()`: Tarih aralığı kontrolü
- `IsValidBirthDate()`: Doğum tarihi kontrolü
- `IsAllowedFileExtension()`: Dosya uzantısı kontrolü
- `IsValidCoordinate()`: Koordinat doğrulaması

## Kullanım Örnekleri

### Entity Extensions

```csharp
// Kullanıcı tam adını alma
var fullName = user.GetFullName();

// Anonim görüntüleme adı
var displayName = user.GetAnonymousDisplayName(AnonymityLevel.Medium);

// Yorum popülerlik skoru
var popularityScore = review.GetPopularityScore();

// Şirket kategorisi
var sizeCategory = company.GetEmployeeSizeCategory();
```

### Validation Extensions

```csharp
// Email doğrulama
if (email.IsValidEmail())
{
    // Email geçerli
}

// TC Kimlik No doğrulama
if (tcKimlikNo.IsValidTcKimlikNo())
{
    // TC Kimlik No geçerli
}

// Güçlü şifre kontrolü
if (password.IsStrongPassword())
{
    // Şifre yeterince güçlü
}
```

### String Extensions

```csharp
// URL dostu slug oluşturma
var slug = "Türkçe Başlık Örneği".ToSlug(); // "turkce-baslik-ornegi"

// Email maskeleme
var masked = "user@example.com".MaskEmail(); // "use***@example.com"

// Metin kesme
var truncated = longText.Truncate(100);
```

### Repository Extensions

```csharp
// Email ile kullanıcı arama
var user = await userRepository.GetByEmailAsync("user@example.com");

// En yüksek puanlı şirketler
var topCompanies = await companyRepository.GetTopRatedCompaniesAsync(10);

// Kullanıcının oy verdiği yorumlar
var votes = await reviewVoteRepository.GetUserVotesForReviewsAsync(userId, reviewIds);
```

### Queryable Extensions

```csharp
// Aktif kayıtları getirme
var activeUsers = users.WhereActive();

// Son 30 günün kayıtları
var recentReviews = reviews.WhereLastDays(30);

// Sayfalama
var pagedResults = companies.Paginate(page: 2, pageSize: 20);

// Dinamik arama
var searchResults = reviews.Search("arama terimi", r => r.CommentText, r => r.Title);
```

## Mimari Prensipler

1. **Pure Functions**: Extension metodlar side-effect içermez
2. **Null Safety**: Null değerler için güvenli kontroller
3. **Performans**: Gereksiz hesaplamalardan kaçınılır
4. **Testlenebilirlik**: Tüm metodlar kolayca test edilebilir
5. **Reusability**: Kodun yeniden kullanılabilirliği maksimize edilir

## Genişletme Kuralları

Yeni extension metod eklerken:

1. İlgili dosyaya ekleyin (entity, string, validation vb.)
2. XML documentation ekleyin
3. Null kontrolleri yapın
4. Pure function prensibine uyun
5. Unit test yazın
6. README'yi güncelleyin

## Bağımlılıklar

- System.Linq
- System.Text.RegularExpressions
- RateTheWork.Domain.Common
- RateTheWork.Domain.Constants
- RateTheWork.Domain.Entities
- RateTheWork.Domain.Enums
- RateTheWork.Domain.Interfaces.Repositories