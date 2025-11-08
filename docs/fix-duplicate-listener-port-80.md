# 🔧 Исправление: DuplicateListener на порту 80

## Проблема

Terraform пытается создать два listener на порту 80:
1. `aws_lb_listener.http` (порт 80) - старый listener, который был удален вручную, но остался в state
2. `aws_lb_listener.http_redirect[0]` (порт 80) - новый redirect listener

## ✅ Решение: Удалить старый listener из Terraform state

### Вариант 1: Через GitHub Actions (добавить шаг)

Добавьте шаг в `.github/workflows/infra-deploy.yml` перед `terraform plan`:

```yaml
- name: Remove old HTTP listener from state (if exists)
  run: |
    terraform state rm 'module.alb.aws_lb_listener.http' 2>/dev/null || echo "Listener not in state"
  continue-on-error: true
```

### Вариант 2: Локально

```bash
cd terraform
terraform state rm 'module.alb.aws_lb_listener.http'
terraform plan  # Проверить
terraform apply  # Применить
```

### Вариант 3: Через AWS Console (удалить созданный listener)

Если listener уже создан на порту 80:

1. EC2 → Load Balancers → `app-alb` → Listeners
2. Найдите listener на порту 80 с типом "forward" (не "redirect")
3. Удалите его
4. Запустите `terraform apply` снова

---

## 📝 После исправления

После удаления старого listener из state или AWS:

1. **Проверьте план:**
   ```bash
   terraform plan
   ```

2. **Должно быть создано:**
   - ✅ HTTP redirect listener на порту 80 (HTTP → HTTPS)
   - ✅ HTTPS listener на порту 443
   - ✅ Route53 A-запись

3. **Запустите apply:**
   ```bash
   terraform apply
   ```

---

## 🔍 Почему это произошло?

1. Старый `aws_lb_listener.http` был создан на порту 80 (когда `enable_https = false`)
2. Вы удалили его вручную через AWS Console
3. Terraform видит, что listener есть в state, но отсутствует в AWS
4. Terraform пытается его пересоздать
5. Но также пытается создать `http_redirect[0]` на том же порту 80
6. Конфликт!

---

## ✅ Правильная конфигурация

При `enable_https = true`:
- ❌ `aws_lb_listener.http` НЕ создается (count = 0)
- ✅ `aws_lb_listener.http_redirect[0]` создается на порту 80 (redirect)
- ✅ `aws_lb_listener.https[0]` создается на порту 443 (HTTPS)

При `enable_https = false`:
- ✅ `aws_lb_listener.http` создается на порту 80 (forward)
- ❌ `aws_lb_listener.http_redirect[0]` НЕ создается (count = 0)
- ❌ `aws_lb_listener.https[0]` НЕ создается (count = 0)

