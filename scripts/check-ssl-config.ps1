# Script to check SSL configuration for DuckDNS + Cloudflare
# Usage: .\check-ssl-config.ps1 secret-nick.duckdns.org

param(
    [string]$Domain = "secret-nick.duckdns.org"
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🔍 Проверка SSL конфигурации для $Domain" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check 1: DNS Resolution
Write-Host "1️⃣ Проверка DNS разрешения..." -ForegroundColor Yellow
try {
    $DNSResult = Resolve-DnsName -Name $Domain -ErrorAction Stop | Where-Object { $_.Type -eq "A" } | Select-Object -First 1
    $IP = $DNSResult.IPAddress
    Write-Host "   ✅ DNS разрешен: $IP" -ForegroundColor Green
    
    # Check if it's Cloudflare IP
    if ($IP -match "^104\." -or $IP -match "^172\." -or $IP -match "^198\." -or $IP -match "^141\.") {
        Write-Host "   ✅ IP принадлежит Cloudflare (проксирование работает)" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  IP не принадлежит Cloudflare - возможно, проксирование не включено" -ForegroundColor Yellow
        Write-Host "   💡 Решение: В Cloudflare DNS включите 'Proxied' (оранжевая хмарка)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Не удалось разрешить DNS: $_" -ForegroundColor Red
}
Write-Host ""

# Check 2: HTTPS Connection
Write-Host "2️⃣ Проверка HTTPS соединения..." -ForegroundColor Yellow
try {
    $HTTPSResponse = Invoke-WebRequest -Uri "https://$Domain" -Method Head -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
    $HTTPSCode = $HTTPSResponse.StatusCode
    Write-Host "   ✅ HTTPS работает (код: $HTTPSCode)" -ForegroundColor Green
} catch {
    $HTTPSCode = $_.Exception.Response.StatusCode.value__
    if ($HTTPSCode -eq 200 -or $HTTPSCode -eq 301 -or $HTTPSCode -eq 302) {
        Write-Host "   ✅ HTTPS работает (код: $HTTPSCode)" -ForegroundColor Green
    } else {
        Write-Host "   ❌ HTTPS не работает (код: $HTTPSCode)" -ForegroundColor Red
        Write-Host "   💡 Решение: Проверьте настройки SSL в Cloudflare" -ForegroundColor Yellow
    }
}
Write-Host ""

# Check 3: HTTP to HTTPS Redirect
Write-Host "3️⃣ Проверка редиректа HTTP → HTTPS..." -ForegroundColor Yellow
try {
    $HTTPResponse = Invoke-WebRequest -Uri "http://$Domain" -Method Head -TimeoutSec 10 -MaximumRedirection 0 -UseBasicParsing -ErrorAction Stop
    Write-Host "   ⚠️  HTTP не редиректит на HTTPS (код: $($HTTPResponse.StatusCode))" -ForegroundColor Yellow
    Write-Host "   💡 Решение: Включите 'Always Use HTTPS' в Cloudflare" -ForegroundColor Yellow
} catch {
    $StatusCode = $_.Exception.Response.StatusCode.value__
    if ($StatusCode -eq 301 -or $StatusCode -eq 302) {
        Write-Host "   ✅ Редирект HTTP → HTTPS работает (код: $StatusCode)" -ForegroundColor Green
    } else {
        Write-Host "   ❌ HTTP не отвечает (код: $StatusCode)" -ForegroundColor Red
    }
}
Write-Host ""

# Summary
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "📋 Резюме:" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Если видите проблемы:" -ForegroundColor Yellow
Write-Host "1. Проверьте, что в Cloudflare DNS записи имеют статус 'Proxied' (оранжевая хмарка)" -ForegroundColor White
Write-Host "2. Убедитесь, что SSL режим установлен на 'Full'" -ForegroundColor White
Write-Host "3. Включите 'Always Use HTTPS' в Cloudflare" -ForegroundColor White
Write-Host ""
Write-Host "Подробная инструкция: docs/QUICK-FIX-SSL.md" -ForegroundColor Cyan
Write-Host ""

