output "vpc_id" {
  description = "The ID of the VPC"
  value       = try(module.vpc.vpc_id, null)
}

output "vpc_cidr_block" {
  description = "The CIDR block of the VPC"
  value       = try(module.vpc.vpc_cidr_block, null)
}

output "public_subnet_ids" {
  description = "IDs of created public subnets"
  value       = try(module.vpc.public_subnet_ids, null)
}

output "private_subnet_ids" {
  description = "IDs of created private subnets"
  value       = try(module.vpc.private_subnet_ids, null)
}

output "ec2_id" {
  description = "EC2 module output"
  value       = try(module.ec2, null)
}

output "alb_dns_name" {
  description = "The DNS name of the load balancer"
  value       = try(module.alb.lb_dns_name, null)
}

################################################################################
# DNS and SSL outputs
################################################################################

output "route53_zone_id" {
  description = "The Route53 zone ID"
  value       = var.enable_https ? module.dns_ssl[0].route53_zone_id : null
}

output "route53_name_servers" {
  description = "Name servers for Route53 zone - configure these in your domain registrar"
  value       = var.enable_https ? module.dns_ssl[0].route53_name_servers : []
}

output "certificate_arn" {
  description = "ARN of the ACM certificate"
  value       = var.enable_https ? module.dns_ssl[0].certificate_arn : null
}

output "application_url" {
  description = "URL to access your application"
  value       = var.enable_https ? module.dns_ssl[0].application_url : "http://${module.alb.lb_dns_name}"
}