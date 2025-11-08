# 📥 Импорт существующих ресурсов в Terraform

## Проблема

Terraform пытается создать ресурсы, которые уже существуют в AWS:
- ALB Listener на порту 80 (HTTP redirect)
- ALB Listener на порту 443 (HTTPS)
- Route53 A-запись для домена

## ✅ Решение: Импорт существующих ресурсов

### Шаг 1: Получить ARN существующих ресурсов

Запустите скрипт для получения ARN:

```powershell
.\scripts\import-existing-resources.ps1
```

Скрипт покажет команды для импорта.

---

### Шаг 2: Импортировать ресурсы в Terraform state

Выполните команды, которые показал скрипт:

#### Импорт HTTP redirect listener (порт 80):

```bash
cd terraform
terraform import 'module.alb.aws_lb_listener.http_redirect[0]' 'arn:aws:elasticloadbalancing:eu-central-1:681052412865:listener/app/app-alb/3745113c02cce955/XXXXX'
```

#### Импорт HTTPS listener (порт 443):

```bash
terraform import 'module.alb.aws_lb_listener.https[0]' 'arn:aws:elasticloadbalancing:eu-central-1:681052412865:listener/app/app-alb/3745113c02cce955/XXXXX'
```

#### Импорт Route53 A-записи:

```bash
terraform import 'module.dns_ssl[0].aws_route53_record.app' 'Z01968361BKBZ0AI1EB7Y_sekret-nick.pp.ua_A'
```

**Формат Route53 record ID:** `{ZONE_ID}_{RECORD_NAME}_{RECORD_TYPE}`

---

### Шаг 3: Проверить импорт

После импорта выполните:

```bash
terraform plan
```

Terraform должен показать, что ресурсы уже существуют и не требуют изменений (или покажет только необходимые изменения).

---

### Шаг 4: Запустить terraform apply

Если `terraform plan` показывает, что все в порядке:

```bash
terraform apply
```

---

## 🔍 Альтернатива: Получить ARN вручную

Если скрипт не работает, получите ARN вручную:

### Получить ARN listeners:

```bash
aws elbv2 describe-listeners \
  --load-balancer-arn "arn:aws:elasticloadbalancing:eu-central-1:681052412865:loadbalancer/app/app-alb/3745113c02cce955" \
  --region eu-central-1 \
  --query "Listeners[*].[Port,Protocol,ListenerArn]" \
  --output table
```

### Получить Route53 record ID:

```bash
aws route53 list-resource-record-sets \
  --hosted-zone-id Z01968361BKBZ0AI1EB7Y \
  --query "ResourceRecordSets[?Name=='sekret-nick.pp.ua.' && Type=='A']" \
  --output json
```

Route53 record ID для импорта: `Z01968361BKBZ0AI1EB7Y_sekret-nick.pp.ua_A`

---

## ⚠️ Важно

1. **Импортируйте ресурсы в правильном порядке:**
   - Сначала listeners
   - Затем Route53 запись

2. **Проверьте ARN перед импортом:**
   - Убедитесь, что ARN правильные
   - Убедитесь, что ресурсы действительно существуют

3. **После импорта:**
   - Выполните `terraform plan` для проверки
   - Если все в порядке, выполните `terraform apply`

---

## 🐛 Если импорт не работает

Если импорт не работает, можно удалить существующие ресурсы вручную через AWS Console и позволить Terraform создать их заново:

1. **Удалить listeners через AWS Console:**
   - EC2 → Load Balancers → app-alb → Listeners
   - Удалить listener на порту 80
   - Удалить listener на порту 443

2. **Удалить Route53 A-запись:**
   - Route53 → Hosted Zones → sekret-nick.pp.ua
   - Удалить A-запись для sekret-nick.pp.ua

3. **Запустить terraform apply:**
   - Terraform создаст ресурсы заново

---

## 📚 Дополнительная информация

- [Terraform Import Documentation](https://developer.hashicorp.com/terraform/cli/commands/import)
- [AWS ELBv2 Listener Import](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/lb_listener#import)
- [AWS Route53 Record Import](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route53_record#import)

