################################################################################
# General
################################################################################

aws_region = "eu-central-1"
az_letter  = "a"
tags = {
  owner = "terraform"
}

################################################################################
# VPC variables
################################################################################

vpc_cidr = "10.0.0.0/16"
vpc_name = "test"
subnets = {
  subnet0 = {
    cidr_block = "10.0.1.0/24"
    public     = true
    az_index   = 0
  },
  subnet1 = {
    cidr_block = "10.0.2.0/24"
    public     = true
    az_index   = 1
  },
  subnet2 = {
    cidr_block = "10.0.3.0/24"
    public     = false
    az_index   = 0
  }
  subnet3 = {
    cidr_block = "10.0.4.0/24"
    public     = false
    az_index   = 1
  }

}

###############################################
# Security Groups variables and ALB variables
###############################################

name_prefix       = "app"
project_name      = "app"
alb_ingress_cidr  = ["0.0.0.0/0"]
alb_ingress_ports = [80, 443, 8080]  # Added 443 for HTTPS
web_backend_port  = 8080
web_ui_port       = 80

################################################################################
# DNS and SSL/TLS configuration
################################################################################

# Enable HTTPS with SSL/TLS certificate
# Set to true after registering a domain
enable_https = true

# Your registered domain name (e.g., yourapp.tk, yourapp.duckdns.org)
# IMPORTANT: Register a free domain first at:
# - https://www.duckdns.org/ (*.duckdns.org - easiest, but no DNS validation support)
# - https://freedns.afraid.org/ (various free domains)
# - https://nic.eu.org/ (*.eu.org - takes 1-2 days, RECOMMENDED for Route53)
domain_name = "sekret-nick.pp.ua"  # Домен с nic.ua

# Create Route53 hosted zone (set to false if using external DNS provider like DuckDNS)
# For DuckDNS: set to false (DuckDNS doesn't support nameserver delegation)
# For nic.ua, eu.org or other registrars: set to true to create Route53 zone
create_route53_zone = true  # ВАЖНО: true для nic.ua с Route53

# Additional domain names for certificate (optional)
# subject_alternative_names = ["www.marafon-app.duckdns.org"]

# If using existing Route53 zone, provide zone ID:
# route53_zone_id = "Z0123456789ABCDEFGHIJ"

################################################################################
# EC2 configuration
################################################################################
#Amazon Linux 2023 AMI 2023.8.20250808.1 x86_64 HVM kernel-6.1 in eu-central-1

ami           = "ami-015cbce10f839bd0c"
ec2_name_set  = ["react", "angular", "dotnet"]
subnet        = ""
sgs           = []
instance_type = "t3.micro"
iam_role_policies = {
  AdministratorAccess = "arn:aws:iam::aws:policy/AdministratorAccess",
  SSM                 = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

################################################################################
# RDS variables
################################################################################

rds_port             = 5432
db_id                = "postgres-db"
db_username          = "postgres"
db_engine            = "postgres"
db_engine_version    = "17.5"
db_instance_class    = "db.t3.micro"
db_subnet_group_name = "rds-private-subnet-group"
