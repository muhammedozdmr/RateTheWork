# API Keys Setup Guide / API Anahtarları Kurulum Kılavuzu

Bu kılavuz, RateTheWork uygulaması için gerekli olan SendGrid (email) ve Twilio (SMS) servislerinin API anahtarlarını nasıl edineceğinizi ve yapılandıracağınızı açıklar.

## SendGrid Email Service Setup

### 1. SendGrid Hesabı Oluşturma
1. [SendGrid.com](https://sendgrid.com) adresine gidin
2. "Start For Free" butonuna tıklayın
3. Hesap bilgilerinizi girin:
   - Email adresi
   - Şifre
   - Şirket bilgileri (opsiyonel)
4. Email adresinizi doğrulayın

### 2. Sender Identity Doğrulama
SendGrid'de email gönderebilmek için sender identity doğrulaması zorunludur:

1. Settings → Sender Authentication'a gidin
2. İki seçeneğiniz var:
   - **Domain Authentication** (Önerilen): Tüm domain'inizi doğrulayın
   - **Single Sender Verification**: Tek bir email adresini doğrulayın

#### Domain Authentication (Cloudflare için):
1. "Authenticate Your Domain" seçin
2. DNS provider olarak "Other Host" seçin
3. Verilen DNS kayıtlarını Cloudflare'de ekleyin:
   ```
   CNAME em1234.yourdomain.com -> sendgrid.net
   CNAME s1._domainkey.yourdomain.com -> s1.domainkey.u1234567.wl.sendgrid.net
   CNAME s2._domainkey.yourdomain.com -> s2.domainkey.u1234567.wl.sendgrid.net
   ```
4. DNS kayıtları yayıldıktan sonra "Verify" butonuna tıklayın

### 3. API Key Oluşturma
1. Settings → API Keys'e gidin
2. "Create API Key" butonuna tıklayın
3. API Key adı: "RateTheWork Production" (veya istediğiniz bir isim)
4. API Key Permissions: "Full Access" veya "Restricted Access" seçin
   - Restricted Access seçerseniz minimum şu izinleri verin:
     - Mail Send: Full Access
     - Template Engine: Read Access (eğer template kullanacaksanız)
5. "Create & View" butonuna tıklayın
6. **ÖNEMLİ**: API Key'i hemen kopyalayın! Bir daha gösterilmeyecek.

### 4. Railway'de Yapılandırma
```bash
# Railway CLI ile:
railway variables set SENDGRID_API_KEY=SG.xxxxxxxxxxxxxxxxxxxx
railway variables set SENDGRID_FROM_EMAIL=noreply@yourdomain.com
railway variables set SENDGRID_FROM_NAME="Your Company Name"
```

### 5. Email Template'leri (Opsiyonel)
SendGrid Dynamic Templates kullanmak isterseniz:
1. Email API → Dynamic Templates'e gidin
2. "Create Template" tıklayın
3. Template'inizi tasarlayın
4. Template ID'yi kopyalayın ve appsettings.json'a ekleyin

## Twilio SMS Service Setup

### 1. Twilio Hesabı Oluşturma
1. [Twilio.com](https://www.twilio.com/try-twilio) adresine gidin
2. "Sign up" butonuna tıklayın
3. Hesap bilgilerinizi girin:
   - Ad, Soyad
   - Email
   - Şifre
4. Telefon numaranızı doğrulayın (SMS ile)

### 2. Trial Hesap Aktivasyonu
1. Twilio Console'a yönlendirileceksiniz
2. "Get a trial phone number" butonuna tıklayın
3. Size atanan telefon numarasını not edin (+1234567890 formatında)

### 3. Account Credentials
1. Console Dashboard'da şunları bulacaksınız:
   - **Account SID**: ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
   - **Auth Token**: (Göstermek için tıklayın)
2. Bu bilgileri güvenli bir yerde saklayın

### 4. Telefon Numarası Satın Alma (Production için)
Trial hesapta sadece doğrulanmış numaralara SMS gönderebilirsiniz. Production için:
1. Phone Numbers → Buy a Number
2. Ülkenizi seçin (Türkiye: +90)
3. SMS capability olan bir numara seçin
4. Aylık ücreti onaylayın ve satın alın

### 5. Railway'de Yapılandırma
```bash
# Railway CLI ile:
railway variables set TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
railway variables set TWILIO_AUTH_TOKEN=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
railway variables set TWILIO_FROM_NUMBER=+1234567890
```

### 6. Türkiye İçin SMS Gönderimi
Türkiye'de SMS göndermek için ek adımlar gerekebilir:
1. Regulatory Compliance → Bundles
2. Türkiye için gerekli belgeleri yükleyin
3. Onay bekleyin (1-3 iş günü)

## Cloudflare KV Setup (Zaten Mevcut)

Cloudflare KV'yi secret management için kullanıyorsunuz. API Token'ınızın şu izinlere sahip olduğundan emin olun:
- Account: Workers KV Storage:Edit
- Zone: Zone:Read (eğer zone-specific KV kullanıyorsanız)

### KV Namespace Oluşturma
1. Cloudflare Dashboard → Workers → KV
2. "Create namespace" tıklayın
3. Namespace adı: "ratethework-secrets" (veya istediğiniz isim)
4. Namespace ID'yi kopyalayın

### Railway'de Yapılandırma
```bash
railway variables set CLOUDFLARE_ACCOUNT_ID=your-account-id
railway variables set CLOUDFLARE_KV_NAMESPACE_ID=your-namespace-id
railway variables set CLOUDFLARE_API_TOKEN=your-api-token
```

## Güvenlik Notları

1. **API Anahtarlarını Asla Kod İçinde Saklamayın**
   - Git'e commit etmeyin
   - Sadece environment variable olarak kullanın

2. **Railway Secret Management**
   - Tüm hassas bilgileri Railway variables olarak saklayın
   - Railway otomatik olarak bunları environment variable'a dönüştürür

3. **API Key Rotasyonu**
   - API anahtarlarınızı düzenli olarak yenileyin (3-6 ayda bir)
   - Eski anahtarları hemen devre dışı bırakın

4. **Rate Limiting**
   - SendGrid: Free tier'da günlük 100 email limiti var
   - Twilio: Trial hesapta $15 kredi verilir

## Test Etme

### Email Testi
```bash
# Local development'ta
curl -X POST http://localhost:5000/api/test/email \
  -H "Content-Type: application/json" \
  -d '{
    "to": "test@example.com",
    "subject": "Test Email",
    "body": "This is a test email from RateTheWork"
  }'
```

### SMS Testi
```bash
# Local development'ta
curl -X POST http://localhost:5000/api/test/sms \
  -H "Content-Type: application/json" \
  -d '{
    "to": "+905551234567",
    "message": "Test SMS from RateTheWork"
  }'
```

## Sorun Giderme

### SendGrid Sorunları
- **401 Unauthorized**: API Key yanlış veya eksik
- **403 Forbidden**: Sender doğrulaması yapılmamış
- **429 Too Many Requests**: Rate limit aşıldı

### Twilio Sorunları
- **Error 21211**: To number geçersiz
- **Error 21608**: Trial hesapta doğrulanmamış numara
- **Error 20003**: Authentication hatası

## Destek
- SendGrid Support: https://support.sendgrid.com
- Twilio Support: https://support.twilio.com
- Railway Discord: https://discord.gg/railway