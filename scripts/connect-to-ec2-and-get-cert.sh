#!/bin/bash

# Скрипт для получения сертификата на EC2
# ВЫПОЛНИТЕ ЭТОТ СКРИПТ ПОСЛЕ ПОДКЛЮЧЕНИЯ К EC2

set -e

DOMAIN="secret-nick.duckdns.org"
EMAIL="worlttanks87@gmail.com"

echo "=========================================="
echo "🔒 Получение SSL сертификата на EC2"
echo "=========================================="
echo ""

# Шаг 1: Обновление системы
echo "1️⃣ Обновление пакетов..."
sudo apt update

# Шаг 2: Установка Certbot
echo ""
echo "2️⃣ Установка Certbot..."
sudo apt install -y certbot

# Шаг 3: Остановка приложения на порту 80
echo ""
echo "3️⃣ Остановка приложения на порту 80..."
echo "   Проверяем, что запущено на порту 80..."

# Проверка Docker Compose
if [ -f "compose.yml" ] || [ -f "docker-compose.yml" ]; then
    echo "   Останавливаем Docker Compose..."
    sudo docker-compose down 2>/dev/null || true
fi

# Проверка nginx
if systemctl is-active --quiet nginx 2>/dev/null; then
    echo "   Останавливаем nginx..."
    sudo systemctl stop nginx
fi

# Проверка Apache
if systemctl is-active --quiet apache2 2>/dev/null; then
    echo "   Останавливаем apache2..."
    sudo systemctl stop apache2
fi

# Убиваем любой процесс на порту 80
if sudo lsof -ti:80 > /dev/null 2>&1; then
    echo "   Найден процесс на порту 80, останавливаем..."
    sudo lsof -ti:80 | xargs sudo kill -9 2>/dev/null || true
    sleep 2
fi

# Шаг 4: Получение сертификата
echo ""
echo "4️⃣ Получение сертификата Let's Encrypt..."
echo "   Домен: $DOMAIN"
echo "   Email: $EMAIL"
echo ""

sudo certbot certonly --standalone \
  --preferred-challenges http \
  -d "$DOMAIN" \
  --email "$EMAIL" \
  --agree-tos \
  --no-eff-email

# Шаг 5: Экспорт сертификатов
echo ""
echo "5️⃣ Экспорт сертификатов..."
CERT_DIR="/etc/letsencrypt/live/$DOMAIN"
EXPORT_DIR="/tmp/ssl-certs"

mkdir -p "$EXPORT_DIR"

sudo cp "$CERT_DIR/cert.pem" "$EXPORT_DIR/cert.pem"
sudo cp "$CERT_DIR/privkey.pem" "$EXPORT_DIR/privkey.pem"
sudo cp "$CERT_DIR/chain.pem" "$EXPORT_DIR/chain.pem"

sudo chmod 644 "$EXPORT_DIR/cert.pem" "$EXPORT_DIR/chain.pem"
sudo chmod 600 "$EXPORT_DIR/privkey.pem"

echo ""
echo "✅ Сертификаты экспортированы в: $EXPORT_DIR"
echo ""
echo "Файлы:"
ls -lh "$EXPORT_DIR"
echo ""

# Шаг 6: Информация для скачивания
echo "=========================================="
echo "📥 Следующий шаг: Скачать сертификаты"
echo "=========================================="
echo ""
echo "С вашего локального компьютера выполните:"
echo ""
echo "1. Создайте папку certs:"
echo "   mkdir certs"
echo ""
echo "2. Скачайте файлы (замените YOUR_EC2_IP на IP вашего EC2):"
echo "   scp -i your-key.pem ubuntu@YOUR_EC2_IP:/tmp/ssl-certs/* ./certs/"
echo ""
echo "Или через AWS Systems Manager Session Manager:"
echo "   aws ssm start-session --target i-YOUR_INSTANCE_ID"
echo "   # Затем скопируйте файлы через терминал"
echo ""
echo "3. После скачивания импортируйте в ACM:"
echo "   aws acm import-certificate \\"
echo "     --certificate fileb://certs/cert.pem \\"
echo "     --private-key fileb://certs/privkey.pem \\"
echo "     --certificate-chain fileb://certs/chain.pem \\"
echo "     --region eu-central-1"
echo ""


