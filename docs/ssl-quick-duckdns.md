# Quick SSL Setup for DuckDNS with AWS

Since DuckDNS doesn't support DNS validation (no CNAME records), we'll use Let's Encrypt.

## Option 1: Manual ACM Certificate Import (After Getting Let's Encrypt)

1. **Get Let's Encrypt certificate on EC2:**
```bash
sudo certbot certonly --standalone -d secret-nick.duckdns.org --email your@email.com
```

2. **Export certificate files:**
```bash
sudo cat /etc/letsencrypt/live/secret-nick.duckdns.org/cert.pem > cert.pem
sudo cat /etc/letsencrypt/live/secret-nick.duckdns.org/privkey.pem > privkey.pem
sudo cat /etc/letsencrypt/live/secret-nick.duckdns.org/chain.pem > chain.pem
```

3. **Import to ACM:**
```bash
aws acm import-certificate \
  --certificate fileb://cert.pem \
  --private-key fileb://privkey.pem \
  --certificate-chain fileb://chain.pem \
  --region eu-central-1 \
  --tags Key=Name,Value=secret-nick-ssl
```

4. **Get Certificate ARN:**
```bash
aws acm list-certificates --region eu-central-1
```

5. **Add HTTPS listener to ALB:**
```bash
ALB_ARN="arn:aws:elasticloadbalancing:eu-central-1:your-account:loadbalancer/app/app-alb/xxxxx"
CERT_ARN="arn:aws:acm:eu-central-1:your-account:certificate/xxxxx"
TARGET_GROUP_ARN="your-target-group-arn"

aws elbv2 create-listener \
  --load-balancer-arn $ALB_ARN \
  --protocol HTTPS \
  --port 443 \
  --certificates CertificateArn=$CERT_ARN \
  --default-actions Type=forward,TargetGroupArn=$TARGET_GROUP_ARN \
  --region eu-central-1
```

## Option 2: Simplest - Direct on EC2 with NGINX

**Connect to your EC2 instance and run:**

```bash
# Install certbot
sudo apt update && sudo apt install -y certbot python3-certbot-nginx

# Get certificate (automatically configures nginx)
sudo certbot --nginx -d secret-nick.duckdns.org --email your@email.com --agree-tos --no-eff-email

# Setup auto-renewal
sudo systemctl enable certbot.timer
```

**Done! Visit https://secret-nick.duckdns.org**

## What to do NOW:

1. Go to AWS Console → EC2 → Instances
2. Find your "react" instance (or any instance with public IP)
3. Note the Public IPv4 address
4. SSH into it:
   ```bash
   ssh -i your-key.pem ubuntu@YOUR_EC2_IP
   ```
5. Run the certbot command above

**Remember:** Your Security Group must allow inbound traffic on ports 80 and 443!
