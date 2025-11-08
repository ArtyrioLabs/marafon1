# Налаштування SSL сертифікату для DuckDNS домену

Оскільки DuckDNS не підтримує DNS validation (CNAME записи), ми використаємо **Let's Encrypt** з **HTTP validation**.

## Варіант 1: Let's Encrypt через Certbot (Рекомендовано для DuckDNS)

### Крок 1: Підключіться до EC2 інстансу

```bash
# Знайдіть публічний IP одного з ваших інстансів
# В AWS Console → EC2 → Instances → виберіть інстанс "react" або "dotnet"

ssh -i your-key.pem ubuntu@YOUR_EC2_IP
```

### Крок 2: Встановіть Certbot

```bash
sudo apt-get update
sudo apt-get install -y certbot
```

### Крок 3: Тимчасово зупиніть застосунок на порту 80

```bash
# Якщо використовуєте Docker
sudo docker ps
sudo docker stop <container_id_on_port_80>

# Або зупиніть nginx/інший web server
sudo systemctl stop nginx
```

### Крок 4: Отримайте SSL сертифікат

```bash
sudo certbot certonly --standalone \
  --preferred-challenges http \
  --email your-email@example.com \
  --agree-tos \
  --no-eff-email \
  -d secret-nick.duckdns.org
```

**Що відбувається:**
- Certbot запустить тимчасовий веб-сервер на порту 80
- Let's Encrypt перевірить, що ви контролюєте домен
- Сертифікат буде збережено в `/etc/letsencrypt/live/secret-nick.duckdns.org/`

### Крок 5: Налаштуйте NGINX з SSL

Створіть конфігурацію NGINX:

```bash
sudo nano /etc/nginx/sites-available/secret-nick
```

Додайте:

```nginx
server {
    listen 80;
    server_name secret-nick.duckdns.org;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name secret-nick.duckdns.org;

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/secret-nick.duckdns.org/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/secret-nick.duckdns.org/privkey.pem;
    
    # Modern SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256';
    ssl_prefer_server_ciphers off;

    # Proxy to your application
    location / {
        proxy_pass http://localhost:8080;  # Adjust to your app port
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Активуйте конфігурацію:

```bash
sudo ln -s /etc/nginx/sites-available/secret-nick /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### Крок 6: Налаштуйте авто-оновлення

```bash
# Перевірте, що таймер працює
sudo systemctl status certbot.timer

# Тестовий запуск оновлення
sudo certbot renew --dry-run
```

### Крок 7: Відкрийте порт 443 в Security Group

1. AWS Console → EC2 → Security Groups
2. Знайдіть security group для вашого інстансу
3. Додайте інbound rule:
   - Type: HTTPS
   - Protocol: TCP
   - Port: 443
   - Source: 0.0.0.0/0

### Крок 8: Перевірте HTTPS

Відкрийте в браузері:
```
https://secret-nick.duckdns.org
```

---

## Варіант 2: AWS Certificate Manager з ALB (Складніший для DuckDNS)

Цей варіант **не працює з DuckDNS**, оскільки:
- DuckDNS не дозволяє додавати CNAME записи для DNS validation
- Email validation не підтримується ACM для безкоштовних доменів

**Рекомендація:** Використовуйте Варіант 1 (Let's Encrypt)

---

## Автоматизація через Docker Compose

Якщо ви хочете автоматизувати SSL в Docker:

```yaml
version: '3.8'

services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - /etc/letsencrypt:/etc/letsencrypt:ro
    depends_on:
      - app

  certbot:
    image: certbot/certbot
    volumes:
      - /etc/letsencrypt:/etc/letsencrypt
      - /var/www/certbot:/var/www/certbot
    entrypoint: "/bin/sh -c 'trap exit TERM; while :; do certbot renew; sleep 12h & wait $${!}; done;'"

  app:
    image: your-app-image
    ports:
      - "8080:8080"
```

---

## Troubleshooting

### Помилка: "Port 80 already in use"
```bash
# Знайдіть процес на порту 80
sudo lsof -i :80
# Зупиніть його
sudo systemctl stop nginx  # або docker stop <container>
```

### Помилка: "Connection refused"
Перевірте Security Group:
```bash
# Переконайтесь, що порти 80 і 443 відкриті
aws ec2 describe-security-groups --group-ids sg-xxxxxx
```

### Сертифікат не оновлюється автоматично
```bash
# Перевірте логи
sudo journalctl -u certbot.timer
# Запустіть вручну
sudo certbot renew --force-renewal
```

---

## Додаткові ресурси

- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [Certbot Instructions](https://certbot.eff.org/)
- [NGINX SSL Configuration](https://nginx.org/en/docs/http/configuring_https_servers.html)
