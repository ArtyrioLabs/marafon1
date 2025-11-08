# DNS and SSL/TLS Certificate Module

This module manages:
- Route53 Hosted Zone configuration
- ACM SSL/TLS certificate generation with DNS validation
- DNS records for certificate validation
- DNS records pointing to Application Load Balancer

## Requirements

### Free Domain Options with ACME Support:
1. **Freenom** (freenom.com) - Free domains: .tk, .ml, .ga, .cf, .gq
2. **DuckDNS** (duckdns.org) - Free subdomain with Let's Encrypt support
3. **eu.org** - Free .eu.org subdomains
4. **afraid.org** - Free DNS service with various free domains

## Usage

### Step 1: Register Free Domain
Choose one of the options above and register your domain.

### Step 2: Configure Terraform Variables
Add to your `terraform.tfvars`:
```hcl
domain_name = "yourapp.tk"  # Your registered domain
create_route53_zone = true   # Set to false if using external DNS
```

### Step 3: After applying, configure nameservers
If using external DNS provider (like Freenom), you'll need to:
1. Get Route53 nameservers from Terraform output
2. Update nameservers in your domain registrar panel

## Resources Created
- Route53 Hosted Zone (if enabled)
- ACM Certificate with DNS validation
- Route53 validation records
- Route53 A record pointing to ALB
