#!/bin/bash

# Скрипт для получения Let's Encrypt сертификата на EC2
# ВЫПОЛНИТЕ ЭТОТ СКРИПТ НА EC2 ИНСТАНСЕ, ГДЕ РАБОТАЕТ ВАШЕ ПРИЛОЖЕНИЕ

set -e

DOMAIN="secret-nick.duckdns.org"
EMAIL="your-email@example.com"  # ЗАМЕНИТЕ НА ВАШ EMAIL

echo "=========================================="
echo "🔒 Получение SSL сертификата для $DOMAIN"
echo "=========================================="
echo ""

# Обновление системы
echo "1️⃣ Обновление пакетов..."
sudo apt update

# Установка Certbot
echo ""
echo "2️⃣ Установка Certbot..."
sudo apt install -y certbot

# ВАЖНО: Остановите приложение на порту 80 временно
echo ""
echo "⚠️  ВАЖНО: Убедитесь, что порт 80 свободен!"
echo "   Если у вас запущено приложение на порту 80, остановите его:"
echo "   sudo docker-compose down"
echo "   или"
echo "   sudo systemctl stop nginx"
echo ""
read -p "Нажмите Enter когда порт 80 будет свободен..."

# Получение сертификата
echo ""
echo "3️⃣ Получение сертификата Let's Encrypt..."
sudo certbot certonly --standalone \
  --preferred-challenges http \
  -d "$DOMAIN" \
  --email "$EMAIL" \
  --agree-tos \
  --no-eff-email

# Экспорт сертификатов
echo ""
echo "4️⃣ Экспорт сертификатов..."
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
echo "  - cert.pem (сертификат)"
echo "  - privkey.pem (приватный ключ)"
echo "  - chain.pem (цепочка сертификатов)"
echo ""
echo "=========================================="
echo "📥 Следующий шаг: Скачайте эти файлы"
echo "=========================================="
echo ""
echo "Скачайте файлы на локальный компьютер:"
echo "  scp -i your-key.pem ubuntu@EC2_IP:/tmp/ssl-certs/* ./certs/"
echo ""
echo "Или используйте AWS Systems Manager Session Manager"

