# RateTheWork Test Documentation

Bu dokümantasyon, RateTheWork projesi için yazılmış olan tüm test türlerini ve kullanım şekillerini açıklar.

## Test Yapısı Genel Bakış

```
RateTheWork/
├── RateTheWork.Domain.Tests/        # Domain katmanı unit testleri
├── RateTheWork.Application.Tests/   # Application katmanı unit testleri  
├── RateTheWork.Architecture.Tests/  # Mimari kuralları testleri
├── RateTheWork.Integration.Tests/   # Integration testleri
└── RateTheWork.E2E.Tests/          # End-to-End testleri
```

## 1. Unit Tests

### Domain Tests
Domain katmanındaki entity, value object ve domain logic'lerin test edilmesi.

**Özellikler:**
- FluentAssertions kullanımı
- İzole testler
- Domain kurallarının doğrulanması

**Örnek Test:**
```csharp
[Fact]
public void Email_Create_Should_Create_Valid_Email()
{
    // Arrange & Act
    var email = Email.Create("test@example.com");

    // Assert
    email.Should().NotBeNull();
    email.Value.Should().Be("test@example.com");
    email.Domain.Should().Be("example.com");
}
```

### Application Tests
Handler'lar, servisler ve application logic'in test edilmesi.

**Özellikler:**
- Moq ile dependency mocking
- AutoFixture ile test data oluşturma
- Command/Query handler testleri

## 2. Architecture Tests

Clean Architecture kurallarının otomatik olarak kontrol edilmesi.

### Dependency Tests
Katmanlar arası bağımlılık kurallarının kontrolü:
- Domain katmanı hiçbir katmana bağımlı olmamalı
- Application katmanı Infrastructure'a bağımlı olmamalı
- Handlers Domain'e bağımlı olmalı

### Naming Convention Tests
İsimlendirme standartlarının kontrolü:
- Interface'ler "I" ile başlamalı
- Handler'lar "Handler" ile bitmeli
- Repository'ler "Repository" ile bitmeli
- Service'ler "Service" ile bitmeli

### Layer Tests
Her katmanın sadece kendine ait tipleri içermesi:
- Domain'de service olmamalı
- Application'da handler'lar bulunmalı
- Infrastructure'da repository implementation'ları olmalı

**Örnek Test:**
```csharp
[Fact]
public void Domain_Should_Not_HaveDependencyOnOtherProjects()
{
    var assembly = typeof(Domain.Entities.User).Assembly;

    var result = Types
        .InAssembly(assembly)
        .ShouldNot()
        .HaveDependencyOnAll(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

## 3. Integration Tests

API endpoint'lerinin veritabanı ile birlikte test edilmesi.

### Test Altyapısı
- **TestContainers** ile PostgreSQL container'ı
- **WebApplicationFactory** ile test server
- Gerçek veritabanı işlemleri
- HTTP istek/yanıt testleri

### Test Kategorileri

#### User Integration Tests
- Kullanıcı kaydı
- Giriş yapma
- Email doğrulama
- Profil işlemleri

#### Company Integration Tests
- Şirket oluşturma
- Şirket arama ve filtreleme
- Şirket güncelleme
- Yorumları görüntüleme

#### Job Posting Integration Tests
- İş ilanı oluşturma
- İş arama ve filtreleme
- İş başvurusu yapma
- Başvuru takibi

**Örnek Test:**
```csharp
[Fact]
public async Task RegisterUser_Should_CreateNewUser_WhenValidData()
{
    // Arrange
    var command = new RegisterUserCommand
    {
        Email = "test@example.com",
        Password = "Test123!@#",
        FirstName = "John",
        LastName = "Doe"
    };

    // Act
    var response = await Client.PostAsJsonAsync("/api/users/register", command);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    // Verify database
    var user = await ExecuteDbContextAsync(async db =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == command.Email));
    
    user.Should().NotBeNull();
}
```

## 4. End-to-End (E2E) Tests

Kullanıcı perspektifinden tam uygulama akışlarının test edilmesi.

### Test Altyapısı
- **Playwright** ile browser otomasyonu
- **TestContainers** ile PostgreSQL
- Screenshot alma özelliği
- Gerçek kullanıcı etkileşimleri

### Test Senaryoları

#### User Registration E2E
- Kayıt formunu doldurma
- Validasyon hatalarını kontrol
- Email doğrulama akışı
- Şifre güç göstergesi

#### Job Search E2E
- İş arama ve filtreleme
- İş detaylarını görüntüleme
- İş başvurusu yapma
- İşi kaydetme
- Başvuru takibi

#### Company Review E2E
- Yorum yazma
- Yorum filtreleme
- Anonim yorum
- Yorum oylaması
- Şirket karşılaştırma

**Örnek Test:**
```csharp
[Fact]
public async Task CompleteUserRegistration_Should_CreateAccountAndVerifyEmail()
{
    // Navigate to registration
    await Page.GotoAsync("/");
    await Page.ClickAsync("text=Sign Up");

    // Fill form
    await Page.FillAsync("input[name='email']", "test@example.com");
    await Page.FillAsync("input[name='password']", "Test123!@#");
    
    // Submit
    await Page.ClickAsync("button[type='submit']");

    // Verify success
    var successMessage = await Page.WaitForSelectorAsync("text=Registration successful");
    successMessage.Should().NotBeNull();

    // Take screenshot
    await TakeScreenshotAsync("registration_success");
}
```

## Test Çalıştırma

### Tüm Testleri Çalıştırma
```bash
dotnet test
```

### Belirli Test Projesini Çalıştırma
```bash
dotnet test RateTheWork.Architecture.Tests
```

### Belirli Test Kategorisini Çalıştırma
```bash
dotnet test --filter Category=Integration
```

### Test Coverage Raporu
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Test Yazma Kuralları

1. **AAA Pattern**: Arrange, Act, Assert
2. **Descriptive Names**: Test_Should_ExpectedBehavior_When_Condition
3. **Single Responsibility**: Her test tek bir davranışı test etmeli
4. **Isolation**: Testler birbirinden bağımsız olmalı
5. **FluentAssertions**: Okunabilir assertion'lar için kullanılmalı

## CI/CD Entegrasyonu

GitHub Actions veya benzeri CI/CD araçlarında:

```yaml
- name: Run Unit Tests
  run: dotnet test --filter "Category!=Integration&Category!=E2E"

- name: Run Integration Tests
  run: dotnet test --filter Category=Integration

- name: Run E2E Tests
  run: dotnet test --filter Category=E2E
```

## Sorun Giderme

### PostgreSQL Container Başlatılamıyor
- Docker'ın çalıştığından emin olun
- Port çakışması olmadığını kontrol edin

### Playwright Browser Bulunamıyor
```bash
playwright install chromium
```

### Test Timeout
- E2E testlerde timeout süresini artırın
- Test veritabanını temizleyin

## Performans İpuçları

1. **Parallel Execution**: xUnit'in paralel test özelliğini kullanın
2. **Database Cleanup**: Her test sonrası veritabanını temizleyin
3. **Shared Fixtures**: Pahalı setup işlemleri için class fixture kullanın
4. **Selective Testing**: Geliştirme sırasında sadece ilgili testleri çalıştırın

## Gelecek Geliştirmeler

- [ ] Performance testleri eklenmesi
- [ ] Load testing senaryoları
- [ ] Security testing
- [ ] Accessibility testing
- [ ] Mobile E2E testleri