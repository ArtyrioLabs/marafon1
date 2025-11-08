#!/bin/bash

# Скрипт для получения сертификата с временным изменением DNS
# ВЫПОЛНИТЕ НА EC2

set -e

DOMAIN="secret-nick.duckdns.org"
EMAIL="worlttanks87@gmail.com"

echo "=========================================="
echo "🔒 Получение SSL сертификата (с DNS fix)"
echo "=========================================="
echo ""

# Шаг 1: Получить IP EC2 инстанса
echo "1️⃣ Определение IP EC2 инстанса..."
EC2_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)
echo "   Public IP: $EC2_IP"

# Шаг 2: Инструкция по изменению DNS
echo ""
echo "=========================================="
echo "⚠️  ВАЖНО: Измените DNS перед продолжением!"
echo "=========================================="
echo ""
echo "1. Откройте DuckDNS: https://www.duckdns.org"
echo "2. Временно измените IP для домена secret-nick на: $EC2_IP"
echo "3. Нажмите 'update ip'"
echo "4. Подождите 1-2 минуты для DNS propagation"
echo ""
read -p "Нажмите Enter когда измените DNS и подождете 1-2 минуты..."

# Шаг 3: Проверка DNS
echo ""
echo "2️⃣ Проверка DNS..."
echo "   Проверяем, что домен указывает на EC2..."
DNS_IP=$(dig +short $DOMAIN | tail -1)
echo "   DNS показывает IP: $DNS_IP"
echo "   EC2 IP: $EC2_IP"

if [ "$DNS_IP" != "$EC2_IP" ]; then
    echo "   ⚠️  DNS еще не обновился!"
    echo "   Подождите еще 1-2 минуты и попробуйте снова"
    echo "   Или проверьте вручную: dig +short $DOMAIN"
    exit 1
fi

echo "   ✅ DNS указывает на EC2"

# Шаг 4: Остановка приложения
echo ""
echo "3️⃣ Остановка приложения на порту 80..."
if command -v docker-compose &> /dev/null; then
    sudo docker-compose down 2>/dev/null || true
fi
if systemctl is-active --quiet nginx 2>/dev/null; then
    sudo systemctl stop nginx
fi
if systemctl is-active --quiet httpd 2>/dev/null; then
    sudo systemctl stop httpd
fi

# Убиваем процессы на порту 80
if sudo lsof -ti:80 > /dev/null 2>&1; then
    sudo lsof -ti:80 | xargs sudo kill -9 2>/dev/null || true
    sleep 2
fi

# Шаг 5: Получение сертификата
echo ""
echo "4️⃣ Получение сертификата Let's Encrypt..."
sudo certbot certonly --standalone \
  --preferred-challenges http \
  -d "$DOMAIN" \
  --email "$EMAIL" \
  --agree-tos \
  --no-eff-email

# Шаг 6: Экспорт сертификатов
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
ls -lh "$EXPORT_DIR"

# Шаг 7: Инструкция по возврату DNS
echo ""
echo "=========================================="
echo "⚠️  ВАЖНО: Верните DNS обратно!"
echo "=========================================="
echo ""
echo "1. Откройте DuckDNS: https://www.duckdns.org"
echo "2. Верните IP для домена secret-nick на: 3.124.207.23 (ALB IP)"
echo "3. Нажмите 'update ip'"
echo ""
echo "Сертификаты готовы для импорта в ACM!"
echo ""


