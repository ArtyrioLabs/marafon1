# Скрипт для удаления старого ALB listener на порту 80
# Используйте этот скрипт, если получаете ошибку "DuplicateListener"

param(
    [string]$ALBArn = ""
)

# Получите ARN ALB из Terraform output или укажите вручную
if ([string]::IsNullOrEmpty($ALBArn)) {
    Push-Location terraform
    try {
        $ALBArn = terraform output -raw alb_arn 2>$null
    } finally {
        Pop-Location
    }
}

if ([string]::IsNullOrEmpty($ALBArn)) {
    Write-Host "❌ ALB ARN не найден. Укажите его вручную:" -ForegroundColor Red
    Write-Host "   .\scripts\remove-old-alb-listener.ps1 -ALBArn 'arn:aws:elasticloadbalancing:REGION:ACCOUNT:loadbalancer/app/NAME/ID'"
    Write-Host "   или получите из AWS Console: EC2 > Load Balancers"
    exit 1
}

Write-Host "🔍 Ищем listeners на ALB: $ALBArn" -ForegroundColor Cyan
Write-Host ""

# Получаем список всех listeners
$listeners = aws elbv2 describe-listeners `
    --load-balancer-arn $ALBArn `
    --query 'Listeners[*].[ListenerArn,Port,Protocol]' `
    --output text

if ([string]::IsNullOrEmpty($listeners)) {
    Write-Host "✅ Listeners не найдены" -ForegroundColor Green
    exit 0
}

Write-Host "Найденные listeners:"
$listeners | ForEach-Object {
    $parts = $_ -split "`t"
    if ($parts.Length -eq 3) {
        Write-Host "  - Port: $($parts[1]), Protocol: $($parts[2]), ARN: $($parts[0])"
    }
}

Write-Host ""
Write-Host "🔍 Ищем listener на порту 80..." -ForegroundColor Cyan

# Находим listener на порту 80
$listener80Arn = aws elbv2 describe-listeners `
    --load-balancer-arn $ALBArn `
    --query 'Listeners[?Port==`80`].ListenerArn' `
    --output text

if ([string]::IsNullOrEmpty($listener80Arn)) {
    Write-Host "✅ Listener на порту 80 не найден. Все в порядке!" -ForegroundColor Green
    exit 0
}

Write-Host "⚠️  Найден listener на порту 80: $listener80Arn" -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "Удалить этот listener? (yes/no)"

if ($confirm -notmatch "^[Yy][Ee][Ss]$") {
    Write-Host "❌ Отменено" -ForegroundColor Red
    exit 1
}

Write-Host "🗑️  Удаляем listener..." -ForegroundColor Yellow
aws elbv2 delete-listener --listener-arn $listener80Arn

Write-Host "✅ Listener удален! Теперь можно запустить terraform apply" -ForegroundColor Green

