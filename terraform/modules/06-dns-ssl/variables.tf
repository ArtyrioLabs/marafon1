variable "domain_name" {
  description = "The domain name for SSL certificate and Route53 zone"
  type        = string
}

variable "create_route53_zone" {
  description = "Whether to create Route53 hosted zone or use existing/external DNS"
  type        = bool
  default     = true
}

variable "route53_zone_id" {
  description = "Existing Route53 zone ID (if create_route53_zone is false)"
  type        = string
  default     = ""
}

variable "alb_dns_name" {
  description = "DNS name of the Application Load Balancer"
  type        = string
}

variable "alb_zone_id" {
  description = "Zone ID of the Application Load Balancer"
  type        = string
}

variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default     = {}
}

variable "subject_alternative_names" {
  description = "Additional domain names to include in certificate (e.g., www.example.com)"
  type        = list(string)
  default     = []
}
