#!/bin/bash

# Скрипт для определения ОС и установки Certbot
# Выполните на EC2: bash scripts/check-os-and-install-certbot.sh

echo "=========================================="
echo "🔍 Определение операционной системы"
echo "=========================================="
echo ""

# Определение ОС
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$ID
    VERSION=$VERSION_ID
    echo "ОС: $OS"
    echo "Версия: $VERSION"
elif [ -f /etc/redhat-release ]; then
    OS="rhel"
    echo "ОС: Red Hat / CentOS / Amazon Linux"
else
    OS="unknown"
    echo "ОС: Неизвестна"
fi

echo ""
echo "=========================================="
echo "📦 Установка Certbot"
echo "=========================================="
echo ""

# Установка в зависимости от ОС
case $OS in
    ubuntu|debian)
        echo "Установка для Ubuntu/Debian..."
        sudo apt update
        sudo apt install -y certbot
        ;;
    rhel|centos|amzn|fedora)
        echo "Установка для RHEL/CentOS/Amazon Linux..."
        if command -v dnf &> /dev/null; then
            sudo dnf install -y certbot
        elif command -v yum &> /dev/null; then
            sudo yum install -y certbot
        else
            echo "❌ Не найден менеджер пакетов (yum/dnf)"
            exit 1
        fi
        ;;
    *)
        echo "❌ Неподдерживаемая ОС: $OS"
        echo "Попробуйте установить certbot вручную"
        exit 1
        ;;
esac

echo ""
echo "✅ Certbot установлен!"
echo ""
certbot --version


