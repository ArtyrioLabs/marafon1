#!/bin/bash

# Скрипт для освобождения порта 80 и получения сертификата
# Выполните в WSL: bash scripts/fix-port-80-and-get-cert.sh

set -e

DOMAIN="secret-nick.duckdns.org"
EMAIL="worlttanks87@gmail.com"

echo "=========================================="
echo "🔧 Освобождение порта 80 и получение сертификата"
echo "=========================================="
echo ""

# Шаг 1: Найти и убить процесс на порту 80
echo "1️⃣ Поиск процесса на порту 80..."

# Проверка через разные методы
PORT_IN_USE=false

# Метод 1: lsof
if command -v lsof &> /dev/null; then
    if sudo lsof -ti:80 > /dev/null 2>&1; then
        echo "   Найден процесс через lsof, останавливаем..."
        sudo lsof -ti:80 | xargs sudo kill -9 2>/dev/null || true
        sleep 2
        PORT_IN_USE=true
    fi
fi

# Метод 2: fuser
if command -v fuser &> /dev/null; then
    if sudo fuser 80/tcp 2>/dev/null | grep -q "80/tcp"; then
        echo "   Найден процесс через fuser, останавливаем..."
        sudo fuser -k 80/tcp 2>/dev/null || true
        sleep 2
        PORT_IN_USE=true
    fi
fi

# Метод 3: netstat
if command -v netstat &> /dev/null; then
    if sudo netstat -tulpn 2>/dev/null | grep -q ":80 "; then
        echo "   Найден процесс через netstat"
        PORT_IN_USE=true
    fi
fi

# Метод 4: ss
if command -v ss &> /dev/null; then
    if sudo ss -tulpn 2>/dev/null | grep -q ":80 "; then
        echo "   Найден процесс через ss"
        PORT_IN_USE=true
    fi
fi

if [ "$PORT_IN_USE" = true ]; then
    echo "   ⚠️  Порт 80 может быть занят процессом Windows"
    echo "   Попробуйте остановить через Windows:"
    echo "   net stop http (в PowerShell от администратора)"
    echo "   или"
    echo "   Отключите службу 'World Wide Web Publishing Service' в Windows"
else
    echo "   ✅ Порт 80 свободен (по проверке в WSL)"
fi

# Шаг 2: Проверка порта
echo ""
echo "2️⃣ Проверка порта 80..."
if sudo lsof -ti:80 > /dev/null 2>&1; then
    echo "   ⚠️  Порт 80 все еще занят"
    echo "   Попробуйте вручную:"
    echo "   sudo lsof -i :80"
    echo "   sudo kill -9 <PID>"
    exit 1
else
    echo "   ✅ Порт 80 свободен"
fi

# Шаг 3: Получение сертификата
echo ""
echo "3️⃣ Получение сертификата Let's Encrypt..."
echo "   Домен: $DOMAIN"
echo "   Email: $EMAIL"
echo ""

sudo certbot certonly --standalone \
  --preferred-challenges http \
  -d "$DOMAIN" \
  --email "$EMAIL" \
  --agree-tos \
  --no-eff-email

# Шаг 4: Экспорт сертификатов
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
ls -lh "$EXPORT_DIR"
echo ""
echo "=========================================="
echo "📥 Следующий шаг: Импорт в ACM"
echo "=========================================="
echo ""
echo "1. Скачайте файлы на локальный компьютер:"
echo "   scp -i your-key.pem ubuntu@EC2_IP:/tmp/ssl-certs/* ./certs/"
echo ""
echo "2. Импортируйте в ACM:"
echo "   aws acm import-certificate \\"
echo "     --certificate fileb://certs/cert.pem \\"
echo "     --private-key fileb://certs/privkey.pem \\"
echo "     --certificate-chain fileb://certs/chain.pem \\"
echo "     --region eu-central-1"
echo ""

