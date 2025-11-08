# PowerShell скрипт для импорта сертификата в AWS ACM
# ВЫПОЛНИТЕ НА ЛОКАЛЬНОМ КОМПЬЮТЕРЕ ПОСЛЕ СКАЧИВАНИЯ СЕРТИФИКАТОВ С EC2

$Domain = "secret-nick.duckdns.org"
$Region = "eu-central-1"
$CertDir = "certs"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "📤 Импорт сертификата в AWS ACM" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Проверка наличия файлов
$certFile = "$CertDir\cert.pem"
$keyFile = "$CertDir\privkey.pem"
$chainFile = "$CertDir\chain.pem"

if (-not (Test-Path $certFile)) {
    Write-Host "❌ Файл не найден: $certFile" -ForegroundColor Red
    Write-Host "   Убедитесь, что сертификаты скачаны с EC2 в папку $CertDir" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $keyFile)) {
    Write-Host "❌ Файл не найден: $keyFile" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $chainFile)) {
    Write-Host "❌ Файл не найден: $chainFile" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Все файлы сертификатов найдены" -ForegroundColor Green
Write-Host ""

# Импорт в ACM
Write-Host "📤 Импорт сертификата в ACM..." -ForegroundColor Yellow

try {
    $result = aws acm import-certificate `
        --certificate "fileb://$certFile" `
        --private-key "fileb://$keyFile" `
        --certificate-chain "fileb://$chainFile" `
        --region $Region `
        --tags "Key=Name,Value=secret-nick-ssl" `
        2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Сертификат успешно импортирован!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Certificate ARN:" -ForegroundColor Cyan
        $result | ConvertFrom-Json | Select-Object -ExpandProperty CertificateArn | Write-Host -ForegroundColor Yellow
        Write-Host ""
        Write-Host "==========================================" -ForegroundColor Cyan
        Write-Host "📋 Следующий шаг:" -ForegroundColor Cyan
        Write-Host "==========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "1. Скопируйте Certificate ARN выше" -ForegroundColor White
        Write-Host "2. Откройте AWS Console → EC2 → Load Balancers" -ForegroundColor White
        Write-Host "3. Выберите ваш ALB → Listeners → Add listener" -ForegroundColor White
        Write-Host "4. Настройте HTTPS listener с этим сертификатом" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host "❌ Ошибка при импорте:" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Ошибка: $_" -ForegroundColor Red
}


