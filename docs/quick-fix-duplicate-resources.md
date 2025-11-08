# ⚡ Быстрое решение: DuplicateListener и DuplicateRecord

## Проблема

Terraform выдает ошибки:
- `DuplicateListener: A listener already exists on this port`
- `InvalidChangeBatch: [Tried to create resource record set but it already exists]`

## ✅ Решение: Импорт существующих ресурсов

### Вариант 1: Автоматический (рекомендуется)

1. **Запустите скрипт для получения ARN:**

```powershell
.\scripts\import-existing-resources.ps1
```

2. **Скопируйте и выполните команды импорта**, которые покажет скрипт.

---

### Вариант 2: Вручную через AWS CLI

#### Шаг 1: Получить ARN listeners

```powershell
# Получить все listeners
aws elbv2 describe-listeners `
  --load-balancer-arn "arn:aws:elasticloadbalancing:eu-central-1:681052412865:loadbalancer/app/app-alb/3745113c02cce955" `
  --region eu-central-1 `
  --query "Listeners[*].[Port,ListenerArn]" `
  --output table
```

#### Шаг 2: Импортировать listeners

```bash
cd terraform

# Импорт HTTP redirect listener (порт 80)
terraform import 'module.alb.aws_lb_listener.http_redirect[0]' 'ARN_ОТ_ПОРТА_80'

# Импорт HTTPS listener (порт 443)
terraform import 'module.alb.aws_lb_listener.https[0]' 'ARN_ОТ_ПОРТА_443'
```

#### Шаг 3: Импортировать Route53 A-запись

```bash
terraform import 'module.dns_ssl[0].aws_route53_record.app' 'Z01968361BKBZ0AI1EB7Y_sekret-nick.pp.ua_A'
```

#### Шаг 4: Проверить

```bash
terraform plan
```

Должно показать, что ресурсы уже существуют.

---

## 🔄 Альтернатива: Удалить и пересоздать

Если импорт не работает, можно удалить ресурсы вручную:

### 1. Удалить listeners через AWS Console

1. EC2 → Load Balancers → `app-alb`
2. Вкладка "Listeners"
3. Выберите listener на порту 80 → Actions → Delete
4. Выберите listener на порту 443 → Actions → Delete

### 2. Удалить Route53 A-запись

1. Route53 → Hosted Zones → `sekret-nick.pp.ua`
2. Найдите A-запись для `sekret-nick.pp.ua`
3. Delete → Confirm

### 3. Запустить terraform apply

```bash
terraform apply
```

Terraform создаст ресурсы заново.

---

## 📝 После импорта/удаления

1. **Проверьте план:**
   ```bash
   terraform plan
   ```

2. **Примените изменения:**
   ```bash
   terraform apply
   ```

3. **Проверьте сайт:**
   - Откройте `https://sekret-nick.pp.ua`
   - Должен появиться замок 🔒

---

## ⚠️ Важно

- **Импорт предпочтительнее удаления**, так как сохраняет существующие ресурсы
- **После импорта** выполните `terraform plan` для проверки
- **Если импорт не работает**, используйте удаление и пересоздание

