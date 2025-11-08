#!/bin/bash

# Скрипт установки инструментов для работы с SSL сертификатами
# Выполните в WSL: bash scripts/install-ssl-tools.sh

set -e

echo "=========================================="
echo "🔧 Установка инструментов для SSL"
echo "=========================================="
echo ""

# Обновление пакетов
echo "1️⃣ Обновление списка пакетов..."
sudo apt update

# Установка Certbot (Let's Encrypt)
echo ""
echo "2️⃣ Установка Certbot..."
sudo apt install -y certbot

# Установка AWS CLI (если не установлен)
echo ""
echo "3️⃣ Проверка AWS CLI..."
if ! command -v aws &> /dev/null; then
    echo "   Установка AWS CLI..."
    curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
    unzip awscliv2.zip
    sudo ./aws/install
    rm -rf aws awscliv2.zip
else
    echo "   ✅ AWS CLI уже установлен"
    aws --version
fi

# Установка дополнительных инструментов
echo ""
echo "4️⃣ Установка дополнительных инструментов..."
sudo apt install -y curl unzip openssl

# Проверка установки
echo ""
echo "=========================================="
echo "✅ Проверка установки"
echo "=========================================="
echo ""

echo "Certbot:"
certbot --version || echo "❌ Certbot не установлен"

echo ""
echo "AWS CLI:"
aws --version || echo "❌ AWS CLI не установлен"

echo ""
echo "OpenSSL:"
openssl version || echo "❌ OpenSSL не установлен"

echo ""
echo "=========================================="
echo "✅ Установка завершена!"
echo "=========================================="


