# RateTheWork Application Layer

Application katmanı, Domain ve Infrastructure katmanları arasında köprü görevi görür. Business logic'in orchestration'
ını sağlar.

## Klasör Yapısı

```
RateTheWork.Application/
├── Common/                          # Ortak kullanılan yapılar
│   ├── Behaviors/                   # MediatR Pipeline Behaviors
│   │   ├── LoggingBehavior.cs
│   │   ├── PerformanceBehavior.cs
│   │   ├── ValidationBehavior.cs
│   │   └── TransactionBehavior.cs
│   ├── Exceptions/                  # Application-specific exceptions
│   ├── Interfaces/                  # Infrastructure servisleri için interface'ler
│   ├── Mappings/                    # AutoMapper configuration
│   └── Models/                      # Shared models (Result, PagedList vb.)
├── Features/                        # CQRS Features (Use Cases)
│   ├── Subscriptions/              # Üyelik yönetimi
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── EventHandlers/
│   ├── CompanySubscriptions/       # Şirket üyelikleri
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── EventHandlers/
│   ├── JobPostings/                # İş ilanları
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── EventHandlers/
│   ├── HRPersonnel/                # İK personeli yönetimi
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── EventHandlers/
│   ├── JobApplications/            # İş başvuruları
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── EventHandlers/
│   ├── Companies/                  # Şirket yönetimi
│   ├── Reviews/                    # Yorum yönetimi
│   └── Users/                      # Kullanıcı yönetimi
├── Services/                        # Application Services
│   ├── Interfaces/
│   └── Implementations/
├── DTOs/                           # Data Transfer Objects
│   ├── Requests/
│   └── Responses/
├── Validators/                      # FluentValidation validators
└── DependencyInjection.cs          # Service registration

## Design Patterns

### 1. CQRS (Command Query Responsibility Segregation)
- Commands: State değiştiren işlemler
- Queries: Data okuma işlemleri
- Ayrı modeller ve handler'lar

### 2. Mediator Pattern
- MediatR kullanarak loose coupling
- Request/Response pattern
- Pipeline behaviors

### 3. Repository Pattern
- Domain layer'da interface tanımları
- Infrastructure layer'da implementation

### 4. Unit of Work Pattern
- Transaction yönetimi
- Atomik işlemler

## SOLID Prensipleri

### Single Responsibility
- Her handler tek bir iş yapar
- Service'ler tek bir domain'e odaklanır

### Open/Closed
- Extension için açık, modification için kapalı
- Pipeline behaviors ile genişletilebilir

### Liskov Substitution
- Interface'ler doğru şekilde implement edilir
- Base class'lar düzgün inherit edilir

### Interface Segregation
- Küçük, spesifik interface'ler
- Client'lar ihtiyaç duymadıkları metodlara bağımlı değil

### Dependency Inversion
- High-level modüller low-level modüllere bağımlı değil
- Abstraction'lara bağımlılık

## Clean Architecture Katmanları

1. **Domain Layer** (En içteki katman)
   - Entity'ler
   - Value Object'ler
   - Domain Event'ler
   - Domain Service'ler

2. **Application Layer** (Bu katman)
   - Use Case'ler
   - Application Service'ler
   - DTO'lar
   - Validation

3. **Infrastructure Layer**
   - Database implementation
   - External service integration
   - File system operations

4. **Presentation Layer**
   - API Controllers
   - Request/Response models
   - Authentication/Authorization