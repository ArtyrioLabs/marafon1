#!/bin/bash
# Скрипт для удаления старого ALB listener на порту 80
# Используйте этот скрипт, если получаете ошибку "DuplicateListener"

set -e

# Получите ARN ALB из Terraform output или укажите вручную
ALB_ARN="${ALB_ARN:-$(cd terraform && terraform output -raw alb_arn 2>/dev/null || echo "")}"

if [ -z "$ALB_ARN" ]; then
  echo "❌ ALB ARN не найден. Укажите его вручную:"
  echo "   export ALB_ARN='arn:aws:elasticloadbalancing:REGION:ACCOUNT:loadbalancer/app/NAME/ID'"
  echo "   или получите из AWS Console: EC2 > Load Balancers"
  exit 1
fi

echo "🔍 Ищем listeners на ALB: $ALB_ARN"
echo ""

# Получаем список всех listeners
LISTENERS=$(aws elbv2 describe-listeners \
  --load-balancer-arn "$ALB_ARN" \
  --query 'Listeners[*].[ListenerArn,Port,Protocol]' \
  --output text)

if [ -z "$LISTENERS" ]; then
  echo "✅ Listeners не найдены"
  exit 0
fi

echo "Найденные listeners:"
echo "$LISTENERS" | while read -r arn port protocol; do
  echo "  - Port: $port, Protocol: $protocol, ARN: $arn"
done

echo ""
echo "🔍 Ищем listener на порту 80..."

# Находим listener на порту 80
LISTENER_80_ARN=$(aws elbv2 describe-listeners \
  --load-balancer-arn "$ALB_ARN" \
  --query 'Listeners[?Port==`80`].ListenerArn' \
  --output text)

if [ -z "$LISTENER_80_ARN" ]; then
  echo "✅ Listener на порту 80 не найден. Все в порядке!"
  exit 0
fi

echo "⚠️  Найден listener на порту 80: $LISTENER_80_ARN"
echo ""
read -p "Удалить этот listener? (yes/no): " -r
echo

if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
  echo "❌ Отменено"
  exit 1
fi

echo "🗑️  Удаляем listener..."
aws elbv2 delete-listener --listener-arn "$LISTENER_80_ARN"

echo "✅ Listener удален! Теперь можно запустить terraform apply"

