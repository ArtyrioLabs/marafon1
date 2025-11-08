# Імпорт самопідписаного SSL сертифікату в AWS Certificate Manager (ACM)

## Передумови
- ✅ Самопідписаний сертифікат створено: `certs/secret-nick.pfx`
- ✅ Пароль сертифіката: `SecretNick2025!`
- ✅ Thumbprint: `EF564FFADAD4014FBD40B613AF7C9EBC87E8D314`

---

## Метод 1: Через AWS Console (Рекомендовано для початківців)

### Крок 1: Конвертувати PFX в PEM формат

PowerShell команди для конвертації (виконайте у вашій директорії проекту):

```powershell
# Завантажити сертифікат з PFX
$pfxPath = ".\certs\secret-nick.pfx"
$pfxPassword = ConvertTo-SecureString -String "SecretNick2025!" -Force -AsPlainText
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($pfxPath, $pfxPassword, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)

# Експортувати сертифікат (public key) в Base64
$certBytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
$certBase64 = [System.Convert]::ToBase64String($certBytes)
$certPem = "-----BEGIN CERTIFICATE-----`n"
for ($i = 0; $i -lt $certBase64.Length; $i += 64) {
    $len = [Math]::Min(64, $certBase64.Length - $i)
    $certPem += $certBase64.Substring($i, $len) + "`n"
}
$certPem += "-----END CERTIFICATE-----"
$certPem | Out-File -FilePath ".\certs\certificate.pem" -Encoding ASCII

Write-Host "Certificate saved to: .\certs\certificate.pem" -ForegroundColor Green
```

**⚠️ УВАГА:** Для приватного ключа потрібен OpenSSL, оскільки PowerShell не дозволяє експортувати приватний ключ напряму в PEM форматі.

### Альтернатива: Використати OpenSSL для конвертації

Якщо у вас встановлено OpenSSL:

```powershell
# Конвертувати PFX в PEM
openssl pkcs12 -in .\certs\secret-nick.pfx -out .\certs\certificate.pem -nokeys -passin pass:SecretNick2025!
openssl pkcs12 -in .\certs\secret-nick.pfx -out .\certs\private.key -nocerts -nodes -passin pass:SecretNick2025!
```

### Крок 2: Імпортувати в AWS ACM через Console

1. **Відкрийте AWS Console:**
   - Перейдіть на: https://eu-central-1.console.aws.amazon.com/acm/home?region=eu-central-1

2. **Import certificate:**
   - Натисніть **"Import certificate"**

3. **Certificate body:**
   - Скопіюйте вміст файлу `certs/certificate.pem`
   - Вставте у поле **"Certificate body"**

4. **Certificate private key:**
   - Скопіюйте вміст файлу `certs/private.key`
   - Вставте у поле **"Certificate private key"**

5. **Certificate chain:**
   - Залиште пустим (для самопідписаного сертифікату не потрібно)

6. **Tags (optional):**
   - Додайте тег: `Name` = `secret-nick-ssl`

7. **Натисніть "Next"** і потім **"Import"**

8. **Скопіюйте Certificate ARN:**
   - Після імпорту скопіюйте ARN (щось типу: `arn:aws:acm:eu-central-1:123456789012:certificate/xxx-yyy-zzz`)

---

## Метод 2: Через AWS CLI (Швидкий)

**Якщо у вас встановлено AWS CLI і OpenSSL:**

### Крок 1: Конвертувати PFX в PEM

```powershell
# Експортувати сертифікат
openssl pkcs12 -in .\certs\secret-nick.pfx -out .\certs\certificate.pem -nokeys -passin pass:SecretNick2025!

# Експортувати приватний ключ
openssl pkcs12 -in .\certs\secret-nick.pfx -out .\certs\private.key -nocerts -nodes -passin pass:SecretNick2025!
```

### Крок 2: Імпортувати в ACM через CLI

```powershell
aws acm import-certificate `
  --certificate fileb://certs/certificate.pem `
  --private-key fileb://certs/private.key `
  --region eu-central-1 `
  --tags Key=Name,Value=secret-nick-ssl
```

**Результат:**
```json
{
    "CertificateArn": "arn:aws:acm:eu-central-1:123456789012:certificate/xxx-yyy-zzz"
}
```

**Скопіюйте Certificate ARN** для наступного кроку!

---

## Метод 3: Простіший спосіб - Використати Cloudflare Origin Certificate

**Рекомендований для production!**

### Переваги:
- ✅ Безкоштовний сертифікат на 15 років
- ✅ Автоматично довіряється Cloudflare
- ✅ Не потрібен OpenSSL

### Кроки:

1. **В Cloudflare Dashboard:**
   - Перейдіть в **SSL/TLS** → **Origin Server**
   - Натисніть **"Create Certificate"**

2. **Generate:**
   - Private key type: **RSA (2048)**
   - Hostnames: `secret-nick.duckdns.org`, `*.secret-nick.duckdns.org`
   - Certificate Validity: **15 years**
   - Натисніть **"Create"**

3. **Зберегти:**
   - Скопіюйте **Origin Certificate** → збережіть у `certs/cloudflare-cert.pem`
   - Скопіюйте **Private Key** → збережіть у `certs/cloudflare-key.pem`

4. **Імпортувати в ACM:**
   ```powershell
   aws acm import-certificate `
     --certificate fileb://certs/cloudflare-cert.pem `
     --private-key fileb://certs/cloudflare-key.pem `
     --region eu-central-1 `
     --tags Key=Name,Value=secret-nick-cloudflare-ssl
   ```

5. **Змінити SSL режим в Cloudflare:**
   - SSL/TLS → Overview → вибрати **"Full (strict)"**

---

## Крок 3: Додати HTTPS Listener до ALB

Після імпорту сертифікату в ACM:

### Через AWS Console:

1. **Відкрийте EC2 Console:**
   - Перейдіть на: https://eu-central-1.console.aws.amazon.com/ec2/home?region=eu-central-1#LoadBalancers:

2. **Виберіть ваш ALB:**
   - Знайдіть ALB з назвою `app-alb` або подібне
   - Виберіть його

3. **Додати Listener:**
   - Перейдіть на вкладку **"Listeners"**
   - Натисніть **"Add listener"**

4. **Налаштувати HTTPS Listener:**
   - Protocol: **HTTPS**
   - Port: **443**
   - Default action: **Forward to** → виберіть ваш Target Group
   - Secure listener settings:
     - Security policy: **ELBSecurityPolicy-TLS13-1-2-2021-06**
     - Default SSL/TLS certificate: **From ACM**
     - Виберіть імпортований сертифікат з ARN
   - Натисніть **"Add"**

5. **Додати HTTP to HTTPS Redirect (Optional):**
   - Виберіть HTTP:80 listener
   - Натисніть **"Edit"**
   - Змініть Default action:
     - Type: **Redirect**
     - Protocol: **HTTPS**
     - Port: **443**
     - Status code: **HTTP 301**
   - Натисніть **"Save changes"**

### Через AWS CLI:

```powershell
# Отримати ARN ALB
$albArn = aws elbv2 describe-load-balancers `
  --region eu-central-1 `
  --query "LoadBalancers[?contains(LoadBalancerName,'app-alb')].LoadBalancerArn" `
  --output text

# Отримати ARN Target Group
$tgArn = aws elbv2 describe-target-groups `
  --region eu-central-1 `
  --query "TargetGroups[0].TargetGroupArn" `
  --output text

# Додати HTTPS Listener
aws elbv2 create-listener `
  --load-balancer-arn $albArn `
  --protocol HTTPS `
  --port 443 `
  --certificates CertificateArn=YOUR_CERTIFICATE_ARN `
  --default-actions Type=forward,TargetGroupArn=$tgArn `
  --region eu-central-1
```

**Замініть `YOUR_CERTIFICATE_ARN`** на ARN вашого сертифікату!

---

## Крок 4: Перевірка

### 1. Перевірити, що HTTPS listener створено:

```powershell
aws elbv2 describe-listeners `
  --load-balancer-arn $albArn `
  --region eu-central-1 `
  --query "Listeners[?Protocol=='HTTPS']"
```

### 2. Відкрити в браузері:

```
https://secret-nick.duckdns.org
```

Ви повинні побачити:
- ✅ З замок в адресній строці (можливо з попередженням про самопідписаний сертифікат)
- ✅ Сайт працює через HTTPS
- ✅ Cloudflare проксує трафік

### 3. Змінити SSL режим в Cloudflare:

- Перейдіть в **SSL/TLS** → **Overview**
- Змініть з **"Flexible"** на **"Full"**
- Тепер Cloudflare буде підключатися до ALB через HTTPS

---

## Troubleshooting

### Помилка: "unable to load certificate private key"
- Перевірте, що private key в правильному форматі (PEM)
- Перевірте, що private key не зашифрований паролем
- Використайте OpenSSL для конвертації

### Помилка: "certificate and private key do not match"
- Переконайтеся, що сертифікат та ключ з одного PFX файлу
- Перегенеруйте сертифікат

### Cloudflare показує "ERR_SSL_VERSION_OR_CIPHER_MISMATCH"
- Перевірте, що HTTPS listener використовує TLS 1.2+
- Змініть Security Policy на `ELBSecurityPolicy-TLS13-1-2-2021-06`

### ALB Listener не створюється
- Перевірте, що Security Group дозволяє порт 443
- Перевірте, що сертифікат успішно імпортовано в ACM

---

## Чек-лист

- [ ] Створено самопідписаний сертифікат або Cloudflare Origin Certificate
- [ ] Конвертовано в PEM формат
- [ ] Імпортовано в AWS ACM
- [ ] Скопійовано Certificate ARN
- [ ] Додано HTTPS listener (443) до ALB з сертифікатом
- [ ] Налаштовано HTTP to HTTPS redirect (optional)
- [ ] Змінено SSL режим в Cloudflare на "Full"
- [ ] Перевірено HTTPS доступ через браузер
- [ ] Перевірено SSL рейтинг на ssllabs.com

---

## Наступні кроки

Після успішного налаштування HTTPS:

1. **Оновити Cloudflare SSL режим:** SSL/TLS → Overview → **"Full"** або **"Full (strict)"** (якщо використовуєте Cloudflare Origin Certificate)

2. **Перевірити HTTPS:** Відкрити `https://secret-nick.duckdns.org` → повинен працювати з замком

3. **Перевірити SSL rating:** https://www.ssllabs.com/ssltest/ → ввести ваш домен

4. **Закомітити зміни:** `git add . && git commit -m "feat: add HTTPS support with self-signed certificate" && git push`

🎉 **Вітаємо! Ваш сайт тепер працює з HTTPS!**
