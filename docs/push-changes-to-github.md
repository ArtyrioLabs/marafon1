# 📤 Запушить изменения в GitHub

## Проблема

Изменения в `.github/workflows/infra-deploy.yml` не были запушены в репозиторий, поэтому старый код все еще выполняется в GitHub Actions.

## ✅ Решение: Запушить изменения

### Шаг 1: Проверить статус

```bash
git status
```

Должны увидеть измененные файлы, включая `.github/workflows/infra-deploy.yml`

### Шаг 2: Добавить изменения

```bash
git add .github/workflows/infra-deploy.yml
```

Или добавить все изменения:

```bash
git add .
```

### Шаг 3: Закоммитить

```bash
git commit -m "Fix: Remove old HTTP listener from state when enable_https=true"
```

### Шаг 4: Запушить

```bash
git push
```

---

## 🎯 После пуша

1. **Проверьте GitHub Actions:**
   - Перейдите в ваш репозиторий на GitHub
   - **Actions** → выберите последний workflow run
   - Убедитесь, что шаг "Remove old HTTP listener from state" выполняется

2. **Запустите workflow снова:**
   - **Actions** → **Infrastructure Deployment**
   - **Run workflow** → **Apply**
   - Дождитесь завершения

---

## 📝 Что было исправлено

В `.github/workflows/infra-deploy.yml` добавлен шаг, который:
- Проверяет, что `enable_https = true` в `terraform.tfvars`
- Удаляет старый HTTP listener из Terraform state
- Удаляет старый HTTP listener rule из Terraform state

Это предотвращает конфликт при создании redirect listener на порту 80.

---

## ⚡ Быстрая команда (все сразу)

```bash
git add .github/workflows/infra-deploy.yml
git commit -m "Fix: Remove old HTTP listener from state when enable_https=true"
git push
```

После этого запустите GitHub Actions workflow снова!

