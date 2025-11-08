# 🔒 Полная инструкция: Получение SSL сертификата для DuckDNS и импорт в ACM

## ⚠️ ВАЖНО: Сертификат нужно получать на EC2, а не на локальной машине!

Let's Encrypt требует, чтобы домен был доступен на порту 80 для HTTP validation. Это должно быть на вашем EC2 инстансе.

---

## 📋 Шаг 1: Подключитесь к EC2 инстансу

### Вариант A: Через SSH

```powershell
# Найдите IP вашего EC2 инстанса в AWS Console
# EC2 → Instances → выберите инстанс (react, angular или dotnet)

ssh -i your-key.pem ubuntu@YOUR_EC2_IP
```

### Вариант B: Через AWS Systems Manager Session Manager

1. AWS Console → EC2 → Instances
2. Выберите ваш инстанс
3. Нажмите "Connect" → "Session Manager"
4. Нажмите "Connect"

---

## 📋 Шаг 2: На EC2 - Установите Certbot

После подключения к EC2 выполните:

```bash
# Обновление пакетов
sudo apt update

# Установка Certbot
sudo apt install -y certbot
```

---

## 📋 Шаг 3: На EC2 - Остановите приложение на порту 80

**ВАЖНО:** Certbot должен временно использовать порт 80 для валидации.

```bash
# Если используете Docker Compose
sudo docker-compose down

# Или если используете nginx
sudo systemctl stop nginx

# Или остановите ваше приложение другим способом
```

---

## 📋 Шаг 4: На EC2 - Получите сертификат

```bash
# Замените email на ваш
sudo certbot certonly --standalone \
  --preferred-challenges http \
  -d secret-nick.duckdns.org \
  --email your-email@example.com \
  --agree-tos \
  --no-eff-email
```

**Или используйте готовый скрипт:**

```bash
# Скачайте скрипт на EC2
# Затем выполните:
chmod +x get-cert-on-ec2.sh
./get-cert-on-ec2.sh
```

---

## 📋 Шаг 5: На EC2 - Экспортируйте сертификаты

```bash
# Создайте директорию для экспорта
mkdir -p /tmp/ssl-certs

# Скопируйте сертификаты
sudo cp /etc/letsencrypt/live/secret-nick.duckdns.org/cert.pem /tmp/ssl-certs/
sudo cp /etc/letsencrypt/live/secret-nick.duckdns.org/privkey.pem /tmp/ssl-certs/
sudo cp /etc/letsencrypt/live/secret-nick.duckdns.org/chain.pem /tmp/ssl-certs/

# Установите правильные права
sudo chmod 644 /tmp/ssl-certs/cert.pem /tmp/ssl-certs/chain.pem
sudo chmod 600 /tmp/ssl-certs/privkey.pem
```

---

## 📋 Шаг 6: Скачайте сертификаты на локальный компьютер

### Вариант A: Через SCP

```powershell
# Создайте папку certs в проекте
mkdir certs

# Скачайте файлы
scp -i your-key.pem ubuntu@YOUR_EC2_IP:/tmp/ssl-certs/* ./certs/
```

### Вариант B: Через AWS Systems Manager

1. AWS Console → Systems Manager → Session Manager
2. Подключитесь к инстансу
3. Скопируйте файлы через терминал

---

## 📋 Шаг 7: На локальном компьютере - Импортируйте в ACM

### Создайте папку certs (если еще не создана):

```powershell
mkdir certs
# Поместите туда файлы: cert.pem, privkey.pem, chain.pem
```

### Импортируйте в ACM:

```powershell
aws acm import-certificate `
  --certificate fileb://certs/cert.pem `
  --private-key fileb://certs/privkey.pem `
  --certificate-chain fileb://certs/chain.pem `
  --region eu-central-1 `
  --tags Key=Name,Value=secret-nick-ssl
```

**Или используйте готовый скрипт:**

```powershell
.\scripts\import-to-acm.ps1
```

**Скопируйте Certificate ARN из вывода!**

---

## 📋 Шаг 8: Добавьте HTTPS listener на ALB

### Через AWS Console:

1. AWS Console → EC2 → Load Balancers
2. Выберите ваш ALB
3. Вкладка "Listeners" → "Add listener"
4. Настройки:
   - **Protocol:** HTTPS
   - **Port:** 443
   - **Default action:** Forward to (выберите ваши target groups)
   - **Certificate:** выберите импортированный сертификат из ACM
5. Нажмите "Save"

### Через AWS CLI:

```powershell
# Получите ARN вашего ALB
$ALB_ARN = "arn:aws:elasticloadbalancing:eu-central-1:ACCOUNT:loadbalancer/app/app-alb/XXXXX"
$CERT_ARN = "arn:aws:acm:eu-central-1:ACCOUNT:certificate/XXXXX"  # Из шага 7
$TARGET_GROUP_ARN = "arn:aws:elasticloadbalancing:eu-central-1:ACCOUNT:targetgroup/XXXXX"

aws elbv2 create-listener `
  --load-balancer-arn $ALB_ARN `
  --protocol HTTPS `
  --port 443 `
  --certificates CertificateArn=$CERT_ARN `
  --default-actions Type=forward,TargetGroupArn=$TARGET_GROUP_ARN `
  --region eu-central-1
```

---

## 📋 Шаг 9: Перезапустите приложение на EC2

```bash
# Если используете Docker Compose
sudo docker-compose up -d

# Или если используете nginx
sudo systemctl start nginx
```

---

## ✅ Проверка

1. Откройте в браузере: `https://secret-nick.duckdns.org`
2. Должен появиться замок 🔒
3. Предупреждение "Не защищено" должно исчезнуть

---

## 🔄 Автоматическое обновление сертификата

Certbot автоматически обновляет сертификаты. Но после обновления нужно будет:
1. Экспортировать новые сертификаты с EC2
2. Импортировать их в ACM
3. Обновить listener на ALB

---

## 📚 Файлы скриптов

- `scripts/get-cert-on-ec2.sh` - получение сертификата на EC2
- `scripts/import-to-acm.ps1` - импорт в ACM на локальном компьютере


