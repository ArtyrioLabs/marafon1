#!/bin/bash

# Script to check SSL configuration for DuckDNS + Cloudflare
# Usage: ./check-ssl-config.sh secret-nick.duckdns.org

set -e

DOMAIN="${1:-secret-nick.duckdns.org}"

echo "=========================================="
echo "🔍 Проверка SSL конфигурации для $DOMAIN"
echo "=========================================="
echo ""

# Check 1: DNS Resolution
echo "1️⃣ Проверка DNS разрешения..."
DNS_RESULT=$(nslookup $DOMAIN 2>/dev/null | grep -A 1 "Name:" | tail -1 | awk '{print $2}' || echo "ERROR")
if [[ "$DNS_RESULT" == "ERROR" ]]; then
    echo "   ❌ Не удалось разрешить DNS"
else
    echo "   ✅ DNS разрешен: $DNS_RESULT"
    
    # Check if it's Cloudflare IP
    if [[ "$DNS_RESULT" =~ ^104\.|^172\.|^198\.|^141\. ]]; then
        echo "   ✅ IP принадлежит Cloudflare (проксирование работает)"
    else
        echo "   ⚠️  IP не принадлежит Cloudflare - возможно, проксирование не включено"
        echo "   💡 Решение: В Cloudflare DNS включите 'Proxied' (оранжевая хмарка)"
    fi
fi
echo ""

# Check 2: HTTPS Connection
echo "2️⃣ Проверка HTTPS соединения..."
HTTPS_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 https://$DOMAIN 2>/dev/null || echo "000")
if [[ "$HTTPS_CODE" == "200" ]] || [[ "$HTTPS_CODE" == "301" ]] || [[ "$HTTPS_CODE" == "302" ]]; then
    echo "   ✅ HTTPS работает (код: $HTTPS_CODE)"
else
    echo "   ❌ HTTPS не работает (код: $HTTPS_CODE)"
    echo "   💡 Решение: Проверьте настройки SSL в Cloudflare"
fi
echo ""

# Check 3: SSL Certificate
echo "3️⃣ Проверка SSL сертификата..."
CERT_INFO=$(echo | openssl s_client -servername $DOMAIN -connect $DOMAIN:443 2>/dev/null | openssl x509 -noout -subject -issuer 2>/dev/null || echo "ERROR")
if [[ "$CERT_INFO" == "ERROR" ]]; then
    echo "   ❌ Не удалось получить информацию о сертификате"
else
    echo "   ✅ Сертификат получен:"
    echo "$CERT_INFO" | sed 's/^/      /'
    
    # Check if certificate is from Cloudflare
    if echo "$CERT_INFO" | grep -q "Cloudflare"; then
        echo "   ✅ Сертификат выдан Cloudflare"
    else
        echo "   ⚠️  Сертификат не от Cloudflare"
    fi
fi
echo ""

# Check 4: HTTP to HTTPS Redirect
echo "4️⃣ Проверка редиректа HTTP → HTTPS..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 http://$DOMAIN 2>/dev/null || echo "000")
if [[ "$HTTP_CODE" == "301" ]] || [[ "$HTTP_CODE" == "302" ]]; then
    echo "   ✅ Редирект HTTP → HTTPS работает (код: $HTTP_CODE)"
elif [[ "$HTTP_CODE" == "200" ]]; then
    echo "   ⚠️  HTTP не редиректит на HTTPS"
    echo "   💡 Решение: Включите 'Always Use HTTPS' в Cloudflare"
else
    echo "   ❌ HTTP не отвечает (код: $HTTP_CODE)"
fi
echo ""

# Summary
echo "=========================================="
echo "📋 Резюме:"
echo "=========================================="
echo ""
echo "Если видите проблемы:"
echo "1. Проверьте, что в Cloudflare DNS записи имеют статус 'Proxied' (оранжевая хмарка)"
echo "2. Убедитесь, что SSL режим установлен на 'Full'"
echo "3. Включите 'Always Use HTTPS' в Cloudflare"
echo ""
echo "Подробная инструкция: docs/QUICK-FIX-SSL.md"
echo ""

