#!/bin/bash

# Script to setup Let's Encrypt SSL certificate for DuckDNS domain
# Run this on your EC2 instance that handles the web traffic

set -e

DOMAIN="secret-nick.duckdns.org"
EMAIL="your-email@example.com"  # Replace with your email

echo "=== Installing Certbot ==="
sudo apt-get update
sudo apt-get install -y certbot python3-certbot-nginx

echo "=== Stopping application temporarily ==="
# Stop your Docker containers or application
sudo docker-compose down || true

echo "=== Requesting SSL certificate ==="
sudo certbot certonly --standalone \
  --preferred-challenges http \
  --email "$EMAIL" \
  --agree-tos \
  --no-eff-email \
  -d "$DOMAIN"

echo "=== Certificate obtained ==="
echo "Certificate location: /etc/letsencrypt/live/$DOMAIN/fullchain.pem"
echo "Private key location: /etc/letsencrypt/live/$DOMAIN/privkey.pem"

echo "=== Setting up auto-renewal ==="
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

echo "=== Restarting application ==="
# Restart your application with SSL enabled
# sudo docker-compose up -d

echo ""
echo "✅ SSL certificate successfully installed!"
echo ""
echo "Next steps:"
echo "1. Update your nginx/docker-compose configuration to use SSL"
echo "2. Configure redirect from HTTP (80) to HTTPS (443)"
echo "3. Test with: curl -I https://$DOMAIN"
