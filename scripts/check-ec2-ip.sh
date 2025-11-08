#!/bin/bash

# Скрипт для проверки IP адресов EC2
# Выполните на EC2

echo "=========================================="
echo "🔍 Проверка IP адресов EC2"
echo "=========================================="
echo ""

echo "1️⃣ Public IPv4:"
PUBLIC_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4 2>/dev/null || echo "NOT_AVAILABLE")
if [ "$PUBLIC_IP" = "NOT_AVAILABLE" ] || [ -z "$PUBLIC_IP" ]; then
    echo "   ❌ Public IP недоступен (инстанс в приватной подсети)"
else
    echo "   ✅ $PUBLIC_IP"
fi

echo ""
echo "2️⃣ Private IPv4:"
PRIVATE_IP=$(curl -s http://169.254.169.254/latest/meta-data/local-ipv4)
echo "   $PRIVATE_IP"

echo ""
echo "3️⃣ Instance ID:"
INSTANCE_ID=$(curl -s http://169.254.169.254/latest/meta-data/instance-id)
echo "   $INSTANCE_ID"

echo ""
echo "=========================================="
echo "📋 Решение:"
echo "=========================================="
echo ""

if [ "$PUBLIC_IP" = "NOT_AVAILABLE" ] || [ -z "$PUBLIC_IP" ]; then
    echo "⚠️  У инстанса нет Public IP!"
    echo ""
    echo "Варианты решения:"
    echo ""
    echo "1. Используйте Cloudflare Origin Certificate (рекомендуется)"
    echo "   - Создайте сертификат в Cloudflare"
    echo "   - Импортируйте в ACM"
    echo ""
    echo "2. Используйте домен Freenom с Route53 (для задания)"
    echo "   - Зарегистрируйте домен на Freenom"
    echo "   - Настройте через Terraform"
    echo "   - ACM автоматически получит сертификат"
    echo ""
    echo "3. Добавьте Public IP инстансу:"
    echo "   - AWS Console → EC2 → Instances"
    echo "   - Выберите инстанс → Actions → Networking → Manage IP addresses"
    echo "   - Allocate Elastic IP и привяжите к инстансу"
else
    echo "✅ Public IP найден: $PUBLIC_IP"
    echo ""
    echo "Используйте этот IP для временного изменения DNS в DuckDNS"
fi


