# Railway Deployment Guide

Bu kılavuz RateTheWork uygulamasını Railway'e deploy etmek için gerekli adımları içerir.

## Ön Gereksinimler

1. [Railway CLI](https://docs.railway.app/develop/cli) kurulu olmalı
2. Railway hesabınız olmalı
3. Git repository'niz hazır olmalı

## Deployment Adımları

### 1. Railway Projesi Oluşturma

```bash
# Railway'e giriş yapın
railway login

# Yeni proje oluşturun
railway init

# Proje adı: ratethework (veya istediğiniz isim)
```

### 2. PostgreSQL Database Ekleme

```bash
# PostgreSQL plugin ekleyin
railway add

# "PostgreSQL" seçin
# DATABASE_URL otomatik olarak environment variable olarak eklenecek
```

### 3. Redis Ekleme (Opsiyonel)

Eğer Redis cache kullanmak isterseniz:

```bash
# Redis plugin ekleyin
railway add

# "Redis" seçin
# REDIS_CONNECTION_STRING otomatik olarak eklenecek
```

### 4. Environment Variables Ayarlama

```bash
# Cloudflare KV
railway variables set CLOUDFLARE_ACCOUNT_ID=your-account-id
railway variables set CLOUDFLARE_KV_NAMESPACE_ID=your-namespace-id
railway variables set CLOUDFLARE_API_TOKEN=your-api-token

# SendGrid
railway variables set SENDGRID_API_KEY=SG.xxxxxxxxxxxxxxxxxxxx
railway variables set SENDGRID_FROM_EMAIL=noreply@yourdomain.com
railway variables set SENDGRID_FROM_NAME="Your Company Name"

# Twilio
railway variables set TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
railway variables set TWILIO_AUTH_TOKEN=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
railway variables set TWILIO_FROM_NUMBER=+1234567890

# Security
railway variables set ENCRYPTION_KEY=your-32-character-encryption-key-here

# ASP.NET Core
railway variables set ASPNETCORE_ENVIRONMENT=Production
railway variables set ASPNETCORE_URLS=http://+:$PORT
```

### 5. Storage Volume Ekleme

Local file storage için persistent volume gerekli:

1. Railway Dashboard'a gidin
2. Projenizi seçin
3. Service Settings → Volumes
4. "Add Volume" tıklayın
5. Mount path: `/app/storage`
6. Size: 1GB (veya ihtiyacınıza göre)

### 6. Deploy Etme

```bash
# İlk deployment
railway up

# Veya GitHub integration ile otomatik deploy
# Railway Dashboard → Settings → GitHub repo bağlayın
```

### 7. Domain Ayarlama

#### Railway Domain
```bash
# Otomatik Railway domain oluşturma
railway domain
```

#### Custom Domain (Cloudflare)
1. Railway Dashboard → Settings → Domains
2. "Add Domain" tıklayın
3. Domain adınızı girin: `api.yourdomain.com`
4. Verilen CNAME kaydını Cloudflare'de ekleyin:
   ```
   Type: CNAME
   Name: api
   Target: your-app.up.railway.app
   Proxy: ON (Orange cloud)
   ```

### 8. Health Check Kontrolü

Deploy tamamlandıktan sonra kontrol edin:

```bash
# Railway domain ile
curl https://your-app.up.railway.app/health

# Custom domain ile
curl https://api.yourdomain.com/health
```

## Monitoring ve Logs

### Logs Görüntüleme
```bash
# Real-time logs
railway logs

# Son 100 satır
railway logs -n 100
```

### Railway Dashboard'dan
1. Project → Service seçin
2. "Logs" sekmesine gidin
3. Filtre ve arama yapabilirsiniz

## Troubleshooting

### 1. Database Connection Hatası
```bash
# DATABASE_URL'i kontrol edin
railway variables

# PostgreSQL plugin'in çalıştığından emin olun
railway status
```

### 2. Port Binding Hatası
ASP.NET Core'un PORT environment variable'ını dinlediğinden emin olun:
```csharp
// Program.cs
builder.WebHost.UseUrls($"http://+:{Environment.GetEnvironmentVariable("PORT") ?? "5000"}");
```

### 3. File Upload Hatası
Volume mount edildiğinden emin olun:
- Mount path: `/app/storage`
- appsettings.Production.json'da path doğru olmalı

### 4. Memory/CPU Limitleri
Free tier limitleri:
- 512MB RAM
- 0.5 vCPU
- $5 aylık kredi

Production için upgrade gerekebilir.

## CI/CD Pipeline

### GitHub Actions ile Otomatik Deploy

`.github/workflows/railway-deploy.yml`:
```yaml
name: Deploy to Railway

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Install Railway CLI
        run: npm install -g @railway/cli
      
      - name: Deploy to Railway
        run: railway up
        env:
          RAILWAY_TOKEN: ${{ secrets.RAILWAY_TOKEN }}
```

Railway token almak için:
1. Railway Dashboard → Account Settings
2. Tokens → Create Token
3. GitHub repo → Settings → Secrets → Add RAILWAY_TOKEN

## Backup ve Recovery

### Database Backup
```bash
# Manuel backup
railway run pg_dump $DATABASE_URL > backup.sql

# Restore
railway run psql $DATABASE_URL < backup.sql
```

### Otomatik Backup
Railway Pro plan'da daily backup özelliği var.

## Performance İpuçları

1. **Static Files**: Cloudflare CDN kullanın
2. **Database**: Connection pooling aktif olduğundan emin olun
3. **Redis Cache**: Sık kullanılan verileri cache'leyin
4. **Horizontal Scaling**: Railway replicas özelliğini kullanın

## Güvenlik Kontrol Listesi

- [ ] Tüm environment variable'lar set edildi
- [ ] HTTPS zorunlu (Cloudflare SSL)
- [ ] Database şifresi güçlü
- [ ] API anahtarları production-ready
- [ ] CORS policy ayarlandı
- [ ] Rate limiting aktif
- [ ] Health check endpoint'leri çalışıyor
- [ ] Error logging aktif
- [ ] Backup stratejisi belirlendi

## Destek

- Railway Discord: https://discord.gg/railway
- Railway Docs: https://docs.railway.app
- Status Page: https://status.railway.app