# E-Commerce Platform

## Architecture Overview
- **Backend**: .NET 8, Clean Architecture, DDD, CQRS
- **Frontend**: Blazor WebAssembly
- **Mobile**: .NET MAUI
- **Database**: PostgreSQL (Railway)
- **Search**: ElasticSearch
- **Cache**: Cloudflare Workers KV
- **Email**: SendGrid
- **SMS**: Twilio (to be integrated)

## Project Structure
Solution/
├── src/
│   ├── Domain/              # DDD Entities, Value Objects
│   ├── Application/         # CQRS Commands/Queries, MediatR
│   ├── Infrastructure/      # EF Core, External Services
│   ├── WebAPI/             # ASP.NET Core API
│   ├── Blazor.Client/      # Blazor WebAssembly
│   └── Mobile.MAUI/        # .NET MAUI App
├── tests/
└── docs/

## Deployment
- **Hosting**: Railway.app
- **Domain**: Cloudflare
- **Environment Variables**: Railway dashboard

## API Keys (Store in Railway Variables)
- `SENDGRID_API_KEY`: SendGrid API key
- `TWILIO_ACCOUNT_SID`: Twilio account SID
- `TWILIO_AUTH_TOKEN`: Twilio auth token
- `CLOUDFLARE_API_TOKEN`: CF API token
- `CLOUDFLARE_KV_NAMESPACE_ID`: KV namespace
- `DATABASE_URL`: PostgreSQL connection string
- `ELASTICSEARCH_URL`: ElasticSearch endpoint

## Development Commands
```bash
# Backend
dotnet run --project src/WebAPI

# Blazor
dotnet run --project src/Blazor.Client

# MAUI (iOS Simulator)
dotnet build -t:Run -f net8.0-ios

# Tests
dotnet test

# EF Migrations
dotnet ef migrations add MigrationName -p src/Infrastructure -s src/WebAPI
dotnet ef database update -p src/Infrastructure -s src/WebAPI

Railway Deployment
# Auto-deploys from main branch
git push origin main
```

## Coding Standards

- Use nullable reference types
- Follow DDD principles
- Commands/Queries separated (CQRS)
- All external calls through interfaces
- Background jobs with Hangfire
- Structured logging with Serilog
