output "route53_zone_id" {
  description = "The Route53 zone ID"
  value       = var.create_route53_zone ? aws_route53_zone.main[0].zone_id : var.route53_zone_id
}

output "route53_name_servers" {
  description = "Name servers for the Route53 zone (use these in your domain registrar)"
  value       = var.create_route53_zone ? aws_route53_zone.main[0].name_servers : []
}

output "certificate_arn" {
  description = "ARN of the ACM certificate"
  value       = aws_acm_certificate.main.arn
}

output "certificate_validation_arn" {
  description = "ARN of the validated ACM certificate (use this for ALB listeners)"
  value       = aws_acm_certificate_validation.main.certificate_arn
}

output "certificate_domain_name" {
  description = "Domain name for which the certificate is issued"
  value       = aws_acm_certificate.main.domain_name
}

output "certificate_status" {
  description = "Status of the certificate"
  value       = aws_acm_certificate.main.status
}

output "domain_validation_options" {
  description = "Domain validation options (for manual validation if needed)"
  value       = aws_acm_certificate.main.domain_validation_options
}

output "application_url" {
  description = "URL to access the application"
  value       = "https://${var.domain_name}"
}
