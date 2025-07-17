# Common Klasörü

## 📋 Genel Bakış

Common klasörü, RateTheWork Domain katmanının temel yapı taşlarını içerir. Bu klasördeki sınıflar, tüm domain
entity'lerinin türediği base class'ları ve ortak davranışları tanımlar. Domain-Driven Design (DDD) prensiplerini takip
ederek, entity yaşam döngüsü, audit trail, domain events ve aggregate root kavramlarını implement eder.

## 📂 İçerik

```
Common/
├── BaseEntity.cs           # Tüm entity'lerin temel sınıfı
├── AuditableBaseEntity.cs  # Audit bilgisi tutan entity'ler için
├── ApprovableBaseEntity.cs # Onay mekanizması gerektiren entity'ler için
└── AggregateRoot.cs        # DDD Aggregate Root implementasyonu
```

## 🏗️ Sınıf Hiyerarşisi

```
BaseEntity
    ├── AuditableBaseEntity
    │       └── ApprovableBaseEntity
    └── AggregateRoot
```

## 📝 Detaylı Açıklamalar

### BaseEntity.cs

Tüm domain entity'lerinin türediği temel sınıftır.

**Özellikler:**

- `Id` (string): GUID formatında benzersiz kimlik
- `CreatedAt` (DateTime): Oluşturulma zamanı
- `ModifiedAt` (DateTime?): Son güncelleme zamanı
- `DomainEvents` (IReadOnlyCollection<IDomainEvent>): Domain event listesi

**Temel Özellikler:**

- Otomatik ID üretimi (GUID)
- Domain event desteği
- Equality karşılaştırması (ID bazlı)
- Operator overloading (==, !=)

**Kullanım Örneği:**

```csharp
public class Product : BaseEntity
{
    public string Name { get; private set; }
    
    private Product() : base() { }
    
    public static Product Create(string name)
    {
        var product = new Product
        {
            Name = name
        };
        
        product.AddDomainEvent(new ProductCreatedEvent(product.Id));
        return product;
    }
}
```

### AuditableBaseEntity.cs

Audit trail (denetim izi) gerektiren entity'ler için kullanılır. BaseEntity'den türer.

**Ek Özellikler:**

- `CreatedBy` (string?): Oluşturan kullanıcı ID'si
- `ModifiedBy` (string?): Son güncelleyen kullanıcı ID'si
- `DeletedAt` (DateTime?): Silinme zamanı (soft delete)
- `DeletedBy` (string?): Silen kullanıcı ID'si
- `IsDeleted` (bool): Soft delete durumu

**Özel Metodlar:**

- `SetCreatedAudit(userId)`: Oluşturma audit bilgilerini set eder
- `SetModifiedAudit(userId)`: Güncelleme audit bilgilerini set eder
- `SoftDelete(userId)`: Soft delete işlemi
- `Restore(userId)`: Soft delete geri alma

**Kullanım Örneği:**

```csharp
public class Review : AuditableBaseEntity
{
    public void Update(string content, string userId)
    {
        Content = content;
        SetModifiedAudit(userId); // Audit bilgisi otomatik güncellenir
    }
    
    public void Delete(string userId)
    {
        SoftDelete(userId); // Fiziksel silme yerine soft delete
    }
}
```

### ApprovableBaseEntity.cs

Admin onayı gerektiren entity'ler için kullanılır. AuditableBaseEntity'den türer.

**Ek Özellikler:**

- `IsApproved` (bool): Onay durumu
- `ApprovalStatus` (string): "Pending", "Approved", "Rejected"
- `ApprovedBy` (string?): Onaylayan kullanıcı ID'si
- `ApprovedAt` (DateTime?): Onay tarihi
- `ApprovalNotes` (string?): Onay notları
- `RejectedBy` (string?): Reddeden kullanıcı ID'si
- `RejectedAt` (DateTime?): Red tarihi
- `RejectionReason` (string?): Red nedeni

**Özel Metodlar:**

- `Approve(approvedBy, notes?)`: Entity'yi onaylar
- `Reject(rejectedBy, reason)`: Entity'yi reddeder
- `ResetApproval()`: Onay durumunu sıfırlar

**Kullanım Örneği:**

```csharp
public class Company : ApprovableBaseEntity
{
    public async Task ProcessApproval(string adminId, bool isApproved, string reason)
    {
        if (isApproved)
        {
            Approve(adminId, "Şirket bilgileri doğrulandı");
            AddDomainEvent(new CompanyApprovedEvent(Id));
        }
        else
        {
            Reject(adminId, reason);
            AddDomainEvent(new CompanyRejectedEvent(Id, reason));
        }
    }
}
```

### AggregateRoot.cs

Domain-Driven Design'ın Aggregate Root konseptini implement eder. BaseEntity'den türer.

**Ek Özellikler:**

- `Version` (int): Optimistic concurrency control için versiyon numarası

**Özel Metodlar:**

- `IncrementVersion()`: Versiyon numarasını artırır
- `AddDomainEventWithVersioning(event)`: Event ekler ve versiyonu artırır

**Kullanım Örneği:**

```csharp
public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();
    
    public void AddItem(Product product, int quantity)
    {
        var item = OrderItem.Create(product, quantity);
        _items.Add(item);
        
        // Hem event ekler hem de version'ı artırır
        AddDomainEventWithVersioning(new OrderItemAddedEvent(Id, item));
    }
}
```

## 🔑 Önemli Kavramlar

### Domain Events

- Entity state değişimlerini takip eder
- Event sourcing ve CQRS pattern'leri için altyapı sağlar
- Loose coupling sağlar

### Soft Delete

- Veriler fiziksel olarak silinmez
- IsDeleted flag'i ile işaretlenir
- Geri alma (restore) imkanı sağlar
- Audit trail korunur

### Optimistic Concurrency Control

- Concurrent update'leri handle eder
- Version numarası ile çakışmaları tespit eder
- Data integrity sağlar

### Audit Trail

- Kim, ne zaman, ne yaptı bilgisini tutar
- Compliance ve güvenlik için kritik
- Otomatik olarak yönetilir

## 💡 Best Practices

### ✅ Doğru Kullanım

```csharp
// Entity oluştururken factory method kullanın
var company = Company.Create("Acme Corp", taxId);

// Audit bilgisini method içinde set edin
public void UpdateName(string name, string userId)
{
    Name = name;
    SetModifiedAudit(userId);
}

// Domain event'leri anlamlı yerlerde ekleyin
AddDomainEvent(new CompanyNameChangedEvent(Id, oldName, newName));
```

### ❌ Yanlış Kullanım

```csharp
// Direkt constructor kullanmayın
var company = new Company(); // ❌

// Audit bilgisini dışarıdan set etmeyin
company.ModifiedBy = userId; // ❌

// Her property değişiminde event eklemeyin
Name = name;
AddDomainEvent(new GenericChangeEvent()); // ❌
```

## 🧪 Test Edilebilirlik

Common sınıfları test edilebilir olacak şekilde tasarlanmıştır:

```csharp
[Test]
public void BaseEntity_Should_Generate_Unique_Id()
{
    var entity1 = new TestEntity();
    var entity2 = new TestEntity();
    
    Assert.That(entity1.Id, Is.Not.EqualTo(entity2.Id));
}

[Test]
public void AuditableEntity_Should_Track_Modifications()
{
    var entity = new Review();
    var userId = "user123";
    
    entity.SetModifiedAudit(userId);
    
    Assert.That(entity.ModifiedBy, Is.EqualTo(userId));
    Assert.That(entity.ModifiedAt, Is.Not.Null);
}
```

## 🚨 Dikkat Edilmesi Gerekenler

1. **Immutability**: Property'ler private set olmalı
2. **Encapsulation**: Business logic method'lar içinde olmalı
3. **Domain Events**: Anlamlı yerlerde ve zamanlarda eklenmeli
4. **Audit Trail**: Her zaman güncel tutulmalı
5. **Soft Delete**: Cascade delete senaryolarına dikkat edilmeli

## 📊 Kullanım İstatistikleri

- **BaseEntity**: Tüm 11 entity tarafından kullanılıyor
- **AuditableBaseEntity**: 8 entity tarafından kullanılıyor
- **ApprovableBaseEntity**: 2 entity tarafından kullanılıyor (Company, Review)
- **AggregateRoot**: 3 aggregate root tarafından kullanılıyor

---

*Bu dokümantasyon, Common klasöründeki base class'ların kullanımını ve önemini açıklar. Domain entity'leri oluştururken
bu sınıfları doğru kullanmak, tutarlı ve sürdürülebilir bir domain modeli oluşturmak için kritiktir.*