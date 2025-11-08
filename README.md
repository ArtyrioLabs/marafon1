## Application Composition

# Architecture

Overall [architecture diagram](docs/assets/architecture.png)

# Backend

Backed represented as a [.NET application](backend/README.md)

# Frontend

Frontend developed by [React](frontend/react/README.md) and [Angular](frontend/angular/README.md) teams

# Cloud Infrastructure

Provision the cloud infrastructure in AWS using [Terraform](docs/terraform-readme.md)

# Local Development

Bring up an easily manageable infrastructure for local development with [Docker Compose](docs/compose-readme.md)

# Test Automation

Test Automation represented as a [.NET Reqnroll/NUnit API/UI testing framework](testautomation/SecretNick.TestAutomation/README.md)

# SSL/HTTPS Configuration

## 📋 Assignment: ACM SSL Setup

**For assignment requirements (ACME domain + ACM certificate + DNS routing):**
- **🎯 Assignment guide:** See [assignment-acm-ssl-setup.md](docs/assignment-acm-ssl-setup.md) - Complete step-by-step guide for ACM certificate setup

## 🔧 Troubleshooting "Не защищено" (Not secure)

If you're seeing "Не защищено" (Not secure) warning in your browser:

- **🚀 Start here:** See [YOUR-CASE-FIX.md](docs/YOUR-CASE-FIX.md) if you have CNAME record in Cloudflare
- **Quick fix:** See [QUICK-FIX-SSL.md](docs/QUICK-FIX-SSL.md) for 3-step solution
- **CNAME → A record:** See [fix-cname-to-a-record.md](docs/fix-cname-to-a-record.md) for replacing CNAME with A record
- **Detailed guide:** See [fix-ssl-not-secure.md](docs/fix-ssl-not-secure.md) for complete troubleshooting
- **Cloudflare setup:** See [cloudflare-ssl-setup.md](docs/cloudflare-ssl-setup.md) for Cloudflare configuration
- **Check configuration:** Run `scripts/check-ssl-config.ps1` (Windows) or `scripts/check-ssl-config.sh` (Linux/Mac)