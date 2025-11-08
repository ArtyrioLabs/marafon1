resource "aws_lb" "this" {
  name                       = var.name
  internal                   = false
  load_balancer_type         = "application"
  security_groups            = [var.security_group_id]
  subnets                    = var.subnet_ids
  enable_deletion_protection = false

  tags = merge(
    var.tags,
    { "Name" = var.name }
  )
}

resource "aws_lb_target_group" "backend" {
  name        = "${var.name}-backend"
  port        = var.web_backend_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/api/system/info"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }

  tags = merge(
    var.tags,
    { "Name" = "${var.name}-backend" }
  )
}

resource "aws_lb_target_group" "web_ui_react" {
  name        = "${var.name}-ui-react"
  port        = var.web_ui_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/index.html"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }
 
  tags = merge(
    var.tags,
    { "Name" = "${var.name}-ui_react" }
  )
}

resource "aws_lb_target_group" "web_ui_angular" {
  name        = "${var.name}-ui-angular"
  port        = var.web_ui_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/index.html"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }
 
  tags = merge(
    var.tags,
    { "Name" = "${var.name}-angular" }
  )
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.this.arn
  port              = var.web_ui_port
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    
    forward {
      target_group {
        arn  = aws_lb_target_group.web_ui_react.arn
        weight = 50 # 50% to React
      }
      target_group {
        arn  = aws_lb_target_group.web_ui_angular.arn
        weight = 50 # 50% to Angular
      }
      stickiness {
        enabled  = true
        duration = 1800
      }
    }
  }
}

resource "aws_lb_listener_rule" "http_api_forward" {
  listener_arn = aws_lb_listener.http.arn
  priority     = 1 # Set the priority to ensure this rule is processed first

  condition {
    path_pattern {
      values = ["/api/*"]
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }
}

################################################################################
# HTTPS Listener (optional, enabled when certificate_arn is provided)
################################################################################

resource "aws_lb_listener" "https" {
  count = var.enable_https ? 1 : 0

  load_balancer_arn = aws_lb.this.arn
  port              = 443
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = var.certificate_arn

  default_action {
    type = "forward"
    
    forward {
      target_group {
        arn    = aws_lb_target_group.web_ui_react.arn
        weight = 50 # 50% to React
      }
      target_group {
        arn    = aws_lb_target_group.web_ui_angular.arn
        weight = 50 # 50% to Angular
      }
      stickiness {
        enabled  = true
        duration = 1800
      }
    }
  }
}

resource "aws_lb_listener_rule" "https_api_forward" {
  count = var.enable_https ? 1 : 0

  listener_arn = aws_lb_listener.https[0].arn
  priority     = 1

  condition {
    path_pattern {
      values = ["/api/*"]
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }
}

# HTTP to HTTPS redirect (when HTTPS is enabled)
resource "aws_lb_listener" "http_redirect" {
  count = var.enable_https ? 1 : 0

  load_balancer_arn = aws_lb.this.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = "redirect"

    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }
}
