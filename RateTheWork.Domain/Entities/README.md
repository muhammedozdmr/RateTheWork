# Entities Klasörü

## 📋 Genel Bakış

Entities klasörü, RateTheWork Domain katmanının kalbi olan domain entity'lerini içerir. Bu entity'ler, uygulamanın iş
mantığını ve veri modelini temsil eder. Domain-Driven Design (DDD) prensipleri doğrultusunda, her entity kendi iş
kurallarını ve davranışlarını içerir.

## 📂 İçerik

```
Entities/
├── User.cs                 # Kullanıcı entity'si
├── Company.cs              # Şirket entity'si
├── Review.cs               # Yorum/değerlendirme entity'si
├── AdminUser.cs            # Admin kullanıcı entity'si
├── Badge.cs                # Rozet entity'si
├── UserBadge.cs            # Kullanıcı-rozet ilişkisi
├── ReviewVote.cs           # Yorum oyları entity'si
├── CompanyBranch.cs        # Şirket şubesi entity'si
├── Notification.cs         # Bildirim entity'si
├── Report.cs               # Şikayet entity'si
├── Warning.cs              # Uyarı entity'si
├── Ban.cs                  # Yasaklama entity'si
├── VerificationRequest.cs  # Doğrulama talebi entity'si
└── AuditLog.cs            # Denetim logu entity'si
```

## 🏗️ Entity Hiyerarşisi ve İlişkiler

```
BaseEntity
├── AuditableBaseEntity
│   ├── User
│   ├── AdminUser
│   ├── Badge
│   ├── UserBadge
│   ├── ReviewVote
│   ├── Notification
│   ├── Report
│   ├── Warning
│   ├── Ban
│   └── AuditLog
└── ApprovableBaseEntity
    ├── Company
    ├── CompanyBranch
    ├── Review
    └── VerificationRequest
```

## 📝 Entity Detayları

### 🧑 User.cs

Sisteme kayıtlı kullanıcıları temsil eder.

**Temel Özellikler:**

- Email, Username, PasswordHash
- PhoneNumber, TcIdentityNumber
- FirstName, LastName, BirthDate
- IsEmailVerified, IsPhoneVerified
- IsActive, IsBanned, BannedUntil

**İş Kuralları:**

- Email ve username benzersiz olmalı
- TC kimlik numarası opsiyonel ama varsa geçerli olmalı
- Yasaklı kullanıcılar işlem yapamaz

**Önemli Metodlar:**

```csharp
public static User Create(string email, string username, string passwordHash)
public void UpdateProfile(string firstName, string lastName, DateTime? birthDate)
public void VerifyEmail()
public void VerifyPhone()
public void Ban(DateTime until, string reason)
public void Unban()
```

### 🏢 Company.cs

Şirketleri ve iş yerlerini temsil eder.

**Temel Özellikler:**

- Name, Description, TaxId, MersisNo
- Sector, EmployeeCount, FoundedYear
- Address, PhoneNumber, Email, Website
- IsVerified, VerifiedAt, VerifiedBy
- ReviewStatistics (Value Object)

**İş Kuralları:**

- TaxId ve MersisNo benzersiz olmalı
- Admin onayı gerektirir
- Doğrulanmış şirketler özel rozet alır

**Önemli Metodlar:**

```csharp
public static Company Create(string name, TaxId taxId, CompanyType type, Sector sector)
public void UpdateBasicInfo(string name, string description)
public void UpdateContactInfo(PhoneNumber phone, Email email, string website)
public void UpdateStatistics(CompanyReviewStatistics statistics)
public void Verify(string verifiedBy)
```

### 📝 Review.cs

Kullanıcıların şirketler hakkındaki değerlendirmeleri.

**Temel Özellikler:**

- UserId, CompanyId, CommentType
- Comment, OverallRating, CategoryRatings
- AnonymityLevel, DisplayName
- IsDocumentVerified, VerificationDocumentUrl
- EditCount, LastEditedAt
- Upvotes, Downvotes, HelpfulnessScore

**İş Kuralları:**

- Min 50, max 2000 karakter yorum
- 1-5 arası puanlama (0.5 adımlarla)
- 24 saat içinde max 3 kez düzenlenebilir
- Aynı şirkete aynı tip yorumda 365 gün bekleme

**Önemli Metodlar:**

```csharp
public static Review Create(string userId, string companyId, string comment, decimal rating)
public void Edit(string newComment, decimal newRating)
public void VerifyWithDocument(string documentUrl)
public void Vote(bool isUpvote)
public void Report(string reason)
public bool CanEdit() => EditCount < 3 && (DateTime.UtcNow - CreatedAt).TotalHours <= 24
```

### 👨‍💼 AdminUser.cs

Sistem yöneticilerini temsil eder.

**Temel Özellikler:**

- Username, Email, PasswordHash
- Role, Permissions
- LastLoginAt, LastLoginIp
- FailedLoginAttempts, LockedUntil
- TwoFactorEnabled, TwoFactorSecret

**İş Kuralları:**

- 5 başarısız girişte hesap kilitlenir
- 2FA zorunlu olabilir
- Role bazlı yetkilendirme

**Önemli Metodlar:**

```csharp
public void RecordLogin(string ipAddress)
public void RecordFailedLogin()
public void LockAccount(TimeSpan duration)
public void UnlockAccount()
public void EnableTwoFactor(string secret)
public bool HasPermission(string permission)
```

### 🏅 Badge.cs & UserBadge.cs

Kullanıcı başarı rozetleri sistemi.

**Badge Türleri:**

- FirstReviewer (İlk yorum)
- ActiveReviewer (10+ yorum)
- TrustedReviewer (5+ doğrulanmış yorum)
- TopContributor (50+ yorum)
- CompanyExplorer (10+ farklı şirket)

**İş Kuralları:**

- Otomatik kazanım (sistem tarafından)
- Bir kez kazanılır, geri alınamaz
- Puan ve seviye sistemi

### 👍 ReviewVote.cs

Yorumlara verilen oylar.

**Özellikler:**

- UserId, ReviewId
- IsUpvote (true: helpful, false: not helpful)
- VotedAt

**İş Kuralları:**

- Bir kullanıcı bir yoruma bir oy verebilir
- Kendi yorumuna oy veremez
- Oy değiştirilebilir

### 🏪 CompanyBranch.cs

Şirket şubelerini temsil eder.

**Özellikler:**

- CompanyId, BranchName, BranchCode
- Address, PhoneNumber
- IsHeadquarters, IsActive

**İş Kuralları:**

- Her şirketin bir merkez şubesi olmalı
- Şube kodları şirket içinde benzersiz

### 🔔 Notification.cs

Kullanıcı bildirimlerini yönetir.

**Özellikler:**

- UserId, Title, Message
- Type (Info, Warning, Success, Error)
- IsRead, ReadAt
- ExpiresAt

**Bildirim Türleri:**

- Yorum onaylandı/reddedildi
- Yoruma oy verildi
- Rozet kazanıldı
- Uyarı alındı

### 📢 Report.cs

İçerik şikayetlerini yönetir.

**Özellikler:**

- ReportedBy, EntityType, EntityId
- Reason, Details
- Status (Pending, Reviewed, Resolved)
- ActionTaken, ResolvedBy

**Şikayet Nedenleri:**

- Spam, Küfür/Hakaret
- Yanıltıcı bilgi
- Kişisel bilgi paylaşımı
- Alakasız içerik

### ⚠️ Warning.cs

Kullanıcı uyarılarını yönetir.

**Özellikler:**

- UserId, Reason, Details
- Severity (Low, Medium, High)
- IsAcknowledged, AcknowledgedAt
- ExpiresAt

**İş Kuralları:**

- 3 uyarı = otomatik ban
- Uyarılar 90 gün sonra expire olur
- Kullanıcı uyarıyı onaylamalı

### 🚫 Ban.cs

Kullanıcı yasaklamalarını yönetir.

**Özellikler:**

- UserId, Reason, Details
- BannedBy, BannedAt
- ExpiresAt (null = kalıcı)
- AppealReason, AppealStatus

**Ban Türleri:**

- Geçici (gün bazlı)
- Kalıcı
- IP bazlı
- Cihaz bazlı

### ✅ VerificationRequest.cs

Belge doğrulama taleplerini yönetir.

**Özellikler:**

- UserId, CompanyId, RequestType
- DocumentUrl, DocumentType
- Status, ProcessedBy, ProcessedAt
- RejectionReason

**Doğrulama Türleri:**

- Çalışan doğrulama
- Kimlik doğrulama
- Şirket yetkilisi doğrulama

### 📊 AuditLog.cs

Sistem genelindeki önemli işlemlerin kaydı.

**Özellikler:**

- UserId, Action, EntityType, EntityId
- OldValues, NewValues (JSON)
- IpAddress, UserAgent
- Timestamp

**Log Kategorileri:**

- User actions
- Admin actions
- System events
- Security events

## 💡 Entity Design Patterns

### Factory Methods

```csharp
// ✅ Doğru: Factory method kullanımı
var user = User.Create(email, username, passwordHash);

// ❌ Yanlış: Direkt constructor
var user = new User { Email = email }; // Private constructor!
```

### Domain Methods

```csharp
// ✅ Doğru: Business logic encapsulation
review.Edit(newComment, newRating); // İş kuralları method içinde

// ❌ Yanlış: Anemic model
review.Comment = newComment; // Validation yok!
review.EditCount++; // Manuel state yönetimi!
```

### Value Objects

```csharp
// ✅ Doğru: Value object kullanımı
company.UpdateContactInfo(PhoneNumber.Create("+901234567890"), Email.Create("info@company.com"));

// ❌ Yanlış: Primitive obsession
company.PhoneNumber = "+901234567890"; // Validation yok!
```

## 🔐 Güvenlik Kontrolleri

### Authorization

```csharp
public void UpdateReview(string userId, string newComment)
{
    if (UserId != userId)
        throw new UnauthorizedAccessException("Sadece yorum sahibi düzenleyebilir");
    
    // İşlem devam eder...
}
```

### Validation

```csharp
public static Review Create(string userId, string companyId, string comment, decimal rating)
{
    if (string.IsNullOrWhiteSpace(comment))
        throw new ValidationException("Yorum boş olamaz");
    
    if (comment.Length < DomainConstants.Review.MinCommentLength)
        throw new ValidationException($"Yorum en az {DomainConstants.Review.MinCommentLength} karakter olmalı");
    
    // Diğer validasyonlar...
}
```

## 🧪 Test Senaryoları

### Unit Test Örnekleri

```csharp
[Test]
public void User_Should_Not_Create_With_Invalid_Email()
{
    Assert.Throws<ValidationException>(() => 
        User.Create("invalid-email", "username", "hash"));
}

[Test]
public void Review_Should_Track_Edit_Count()
{
    var review = Review.Create(userId, companyId, "Test comment", 4.5m);
    
    review.Edit("Updated comment", 4.0m);
    
    Assert.That(review.EditCount, Is.EqualTo(1));
    Assert.That(review.LastEditedAt, Is.Not.Null);
}
```

## 📊 Entity İstatistikleri

### En Kompleks Entity'ler

1. **Review**: 20+ property, 10+ method
2. **Company**: 18+ property, 8+ method
3. **User**: 15+ property, 12+ method

### En Çok İlişkisi Olan Entity'ler

1. **User**: Review, Badge, Notification, Warning, Ban
2. **Company**: Review, Branch, VerificationRequest
3. **Review**: User, Company, Vote, Report

## 🚨 Dikkat Edilmesi Gerekenler

1. **Lazy Loading**: Navigation property'ler lazy load edilmez
2. **Circular Dependencies**: Entity'ler arası circular reference'tan kaçının
3. **Aggregate Boundaries**: Her aggregate'in sınırları net olmalı
4. **Transaction Scope**: Aggregate sınırları transaction sınırlarını belirler
5. **Event Consistency**: Domain event'ler eventual consistency sağlar

---

*Bu dokümantasyon, Entities klasöründeki domain entity'lerinin detaylı açıklamasını içerir. Her entity'nin kendi
sorumlulukları ve iş kuralları vardır. DDD prensipleri doğrultusunda, entity'ler anemic model değil, davranış içeren
rich model'lerdir.*