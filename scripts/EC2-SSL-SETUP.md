# 🔒 Получение SSL сертификата на EC2 - Пошаговая инструкция

## Шаг 1: Найдите ваш EC2 инстанс

### Вариант A: Через AWS Console
1. Откройте AWS Console → EC2 → Instances
2. Найдите один из ваших инстансов (react, angular или dotnet)
3. Скопируйте **Public IPv4 address** или **Public IPv4 DNS**

### Вариант B: Через AWS CLI
```powershell
aws ec2 describe-instances --region eu-central-1 --query "Reservations[*].Instances[*].[InstanceId,PublicIpAddress,State.Name,Tags[?Key=='Name'].Value|[0]]" --output table
```

---

## Шаг 2: Подключитесь к EC2

### Вариант A: Через SSH (если есть ключ)

```bash
# В WSL терминале
ssh -i /path/to/your-key.pem ubuntu@YOUR_EC2_IP
```

### Вариант B: Через AWS Systems Manager Session Manager (рекомендуется)

1. AWS Console → EC2 → Instances
2. Выберите ваш инстанс
3. Нажмите **"Connect"** → **"Session Manager"**
4. Нажмите **"Connect"**

**Преимущества:**
- Не нужен SSH ключ
- Работает через браузер
- Безопаснее

---

## Шаг 3: На EC2 - Выполните скрипт

После подключения к EC2 выполните:

```bash
# Скачайте скрипт на EC2 (если его там нет)
# Или скопируйте команды из скрипта

# Выполните скрипт
bash scripts/connect-to-ec2-and-get-cert.sh
```

**Или выполните команды вручную:**

```bash
# 1. Обновление и установка Certbot
sudo apt update
sudo apt install -y certbot

# 2. Остановите приложение на порту 80
sudo docker-compose down
# или
sudo systemctl stop nginx

# 3. Получите сертификат
sudo certbot certonly --standalone \
  --preferred-challenges http \
  -d secret-nick.duckdns.org \
  --email worlttanks87@gmail.com \
  --agree-tos \
  --no-eff-email

# 4. Экспортируйте сертификаты
mkdir -p /tmp/ssl-certs
sudo cp /etc/letsencrypt/live/secret-nick.duckdns.org/cert.pem /tmp/ssl-certs/
sudo cp /etc/letsencrypt/live/secret-nick.duckdns.org/privkey.pem /tmp/ssl-certs/
sudo cp /etc/letsencrypt/live/secret-nick.duckdns.org/chain.pem /tmp/ssl-certs/
sudo chmod 644 /tmp/ssl-certs/*.pem
sudo chmod 600 /tmp/ssl-certs/privkey.pem
```

---

## Шаг 4: Скачайте сертификаты на локальный компьютер

### Вариант A: Через SCP (если используете SSH)

```powershell
# В PowerShell на локальном компьютере
mkdir certs
scp -i your-key.pem ubuntu@YOUR_EC2_IP:/tmp/ssl-certs/* ./certs/
```

### Вариант B: Через AWS Systems Manager

1. В Session Manager терминале на EC2:
```bash
# Создайте архив
cd /tmp
tar -czf ssl-certs.tar.gz ssl-certs/

# Скопируйте содержимое файлов
cat /tmp/ssl-certs/cert.pem
cat /tmp/ssl-certs/privkey.pem
cat /tmp/ssl-certs/chain.pem
```

2. Скопируйте содержимое и сохраните в файлы на локальном компьютере:
   - `certs/cert.pem`
   - `certs/privkey.pem`
   - `certs/chain.pem`

---

## Шаг 5: Импортируйте сертификат в ACM

На локальном компьютере выполните:

```powershell
# Убедитесь, что файлы в папке certs/
aws acm import-certificate `
  --certificate fileb://certs/cert.pem `
  --private-key fileb://certs/privkey.pem `
  --certificate-chain fileb://certs/chain.pem `
  --region eu-central-1 `
  --tags Key=Name,Value=secret-nick-ssl
```

**Скопируйте Certificate ARN из вывода!**

---

## Шаг 6: Добавьте HTTPS listener на ALB

### Через AWS Console:

1. AWS Console → EC2 → Load Balancers
2. Выберите ваш ALB
3. Вкладка **"Listeners"** → **"Add listener"**
4. Настройки:
   - **Protocol:** HTTPS
   - **Port:** 443
   - **Default action:** Forward to (выберите ваши target groups)
   - **Certificate:** выберите импортированный сертификат из ACM
5. Нажмите **"Save"**

### Через AWS CLI:

```powershell
# Получите ARN вашего ALB
$ALB_ARN = "arn:aws:elasticloadbalancing:eu-central-1:ACCOUNT:loadbalancer/app/app-alb/XXXXX"
$CERT_ARN = "arn:aws:acm:eu-central-1:ACCOUNT:certificate/XXXXX"  # Из шага 5
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

## Шаг 7: Перезапустите приложение на EC2

После получения сертификата:

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

## 📋 Чеклист

- [ ] Найден EC2 инстанс
- [ ] Подключились к EC2 (SSH или Session Manager)
- [ ] Установлен Certbot на EC2
- [ ] Остановлено приложение на порту 80
- [ ] Получен сертификат Let's Encrypt
- [ ] Сертификаты экспортированы в /tmp/ssl-certs
- [ ] Сертификаты скачаны на локальный компьютер
- [ ] Сертификат импортирован в ACM
- [ ] Добавлен HTTPS listener на ALB
- [ ] Приложение перезапущено на EC2
- [ ] HTTPS работает: https://secret-nick.duckdns.org


