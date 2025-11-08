# Скрипт для получения ARN существующих ресурсов для импорта в Terraform

param(
    [string]$ALBArn = "arn:aws:elasticloadbalancing:eu-central-1:681052412865:loadbalancer/app/app-alb/3745113c02cce955",
    [string]$Route53ZoneId = "Z01968361BKBZ0AI1EB7Y",
    [string]$DomainName = "sekret-nick.pp.ua",
    [string]$Region = "eu-central-1"
)

Write-Host "🔍 Получаем информацию о существующих ресурсах..." -ForegroundColor Cyan
Write-Host ""

# 1. Получаем listeners
Write-Host "📋 Listeners на ALB:" -ForegroundColor Yellow
$listeners = aws elbv2 describe-listeners `
    --load-balancer-arn $ALBArn `
    --region $Region `
    --query "Listeners[*].[Port,Protocol,ListenerArn]" `
    --output json | ConvertFrom-Json

$listener80Arn = $null
$listener443Arn = $null

foreach ($listener in $listeners) {
    $port = $listener[0]
    $protocol = $listener[1]
    $arn = $listener[2]
    
    Write-Host "  Port: $port, Protocol: $protocol" -ForegroundColor Gray
    Write-Host "    ARN: $arn" -ForegroundColor Gray
    
    if ($port -eq 80) {
        $listener80Arn = $arn
    }
    if ($port -eq 443) {
        $listener443Arn = $arn
    }
}

Write-Host ""

# 2. Получаем Route53 A-запись
Write-Host "📋 Route53 A-запись:" -ForegroundColor Yellow
$route53Record = aws route53 list-resource-record-sets `
    --hosted-zone-id $Route53ZoneId `
    --query "ResourceRecordSets[?Name=='${DomainName}.' && Type=='A']" `
    --output json | ConvertFrom-Json

if ($route53Record) {
    Write-Host "  Найдена A-запись для $DomainName" -ForegroundColor Gray
    $route53RecordId = "${Route53ZoneId}_${DomainName}_A"
} else {
    Write-Host "  A-запись не найдена" -ForegroundColor Gray
    $route53RecordId = $null
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 Команды для импорта в Terraform:" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($listener80Arn) {
    Write-Host "# Импорт HTTP redirect listener (порт 80):" -ForegroundColor Yellow
    Write-Host "cd terraform" -ForegroundColor White
    Write-Host "terraform import 'module.alb.aws_lb_listener.http_redirect[0]' '$listener80Arn'" -ForegroundColor White
    Write-Host ""
}

if ($listener443Arn) {
    Write-Host "# Импорт HTTPS listener (порт 443):" -ForegroundColor Yellow
    Write-Host "cd terraform" -ForegroundColor White
    Write-Host "terraform import 'module.alb.aws_lb_listener.https[0]' '$listener443Arn'" -ForegroundColor White
    Write-Host ""
}

if ($route53RecordId) {
    Write-Host "# Импорт Route53 A-записи:" -ForegroundColor Yellow
    Write-Host "cd terraform" -ForegroundColor White
    Write-Host "terraform import 'module.dns_ssl[0].aws_route53_record.app' '$route53RecordId'" -ForegroundColor White
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 После импорта выполните: terraform plan" -ForegroundColor Green
Write-Host "   Terraform покажет, что ресурсы уже существуют и не требуют изменений." -ForegroundColor Gray

