# Generate self-signed SSL certificate for testing
# This script creates a self-signed certificate for secret-nick.duckdns.org

$DomainName = "secret-nick.duckdns.org"
$CertDir = ".\certs"
$CertFile = "$CertDir\certificate.crt"
$KeyFile = "$CertDir\private.key"

# Create certs directory if it doesn't exist
if (!(Test-Path $CertDir)) {
    New-Item -ItemType Directory -Path $CertDir | Out-Null
    Write-Host "Created directory: $CertDir" -ForegroundColor Green
}

Write-Host "`n=== Generating Self-Signed SSL Certificate ===" -ForegroundColor Cyan
Write-Host "Domain: $DomainName" -ForegroundColor Yellow
Write-Host "This certificate is for TESTING purposes only!" -ForegroundColor Red

# Generate certificate using OpenSSL (requires OpenSSL to be installed)
# If OpenSSL is not available, use PowerShell's New-SelfSignedCertificate

try {
    # Try using OpenSSL first (more compatible with AWS ACM)
    $opensslPath = Get-Command openssl -ErrorAction Stop
    
    Write-Host "`nUsing OpenSSL to generate certificate..." -ForegroundColor Green
    
    # Generate private key
    & openssl genrsa -out $KeyFile 2048
    
    # Generate certificate
    & openssl req -new -x509 -key $KeyFile -out $CertFile -days 365 `
        -subj "/C=UA/ST=Kyiv/L=Kyiv/O=Test/CN=$DomainName"
    
    Write-Host "`nCertificate generated successfully!" -ForegroundColor Green
    Write-Host "Certificate: $CertFile" -ForegroundColor Yellow
    Write-Host "Private Key: $KeyFile" -ForegroundColor Yellow
    
} catch {
    # Fallback to PowerShell's New-SelfSignedCertificate
    Write-Host "`nOpenSSL not found. Using PowerShell to generate certificate..." -ForegroundColor Yellow
    
    $cert = New-SelfSignedCertificate `
        -DnsName $DomainName `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -KeyExportPolicy Exportable `
        -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
        -NotAfter (Get-Date).AddYears(1)
    
    # Export certificate
    $certPassword = ConvertTo-SecureString -String "temp" -Force -AsPlainText
    $pfxPath = "$CertDir\certificate.pfx"
    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $certPassword | Out-Null
    
    # Convert PFX to PEM format for AWS ACM
    # Note: This requires OpenSSL, so we'll just export the cert directly
    $certContent = [System.Convert]::ToBase64String($cert.Export('Cert'))
    $pemCert = "-----BEGIN CERTIFICATE-----`n"
    for ($i = 0; $i -lt $certContent.Length; $i += 64) {
        $len = [Math]::Min(64, $certContent.Length - $i)
        $pemCert += $certContent.Substring($i, $len) + "`n"
    }
    $pemCert += "-----END CERTIFICATE-----`n"
    
    Set-Content -Path $CertFile -Value $pemCert
    
    Write-Host "`nCertificate generated successfully!" -ForegroundColor Green
    Write-Host "Certificate: $CertFile" -ForegroundColor Yellow
    Write-Host "PFX: $pfxPath" -ForegroundColor Yellow
    Write-Host "PFX Password: temp" -ForegroundColor Yellow
    
    # Clean up from cert store
    Remove-Item -Path "Cert:\LocalMachine\My\$($cert.Thumbprint)" -Force
}

Write-Host "`n=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Import certificate to AWS ACM:" -ForegroundColor White
Write-Host "   aws acm import-certificate ``" -ForegroundColor Gray
Write-Host "     --certificate fileb://$CertFile ``" -ForegroundColor Gray
Write-Host "     --private-key fileb://$KeyFile ``" -ForegroundColor Gray
Write-Host "     --region eu-central-1" -ForegroundColor Gray
Write-Host "`n2. Or use the AWS Console:" -ForegroundColor White
Write-Host "   - Go to ACM -> Import certificate" -ForegroundColor Gray
Write-Host "   - Copy contents of $CertFile" -ForegroundColor Gray
Write-Host "   - Copy contents of $KeyFile" -ForegroundColor Gray
Write-Host "`n3. Note the Certificate ARN" -ForegroundColor White
Write-Host "`n4. Update ALB with HTTPS listener using the Certificate ARN" -ForegroundColor White
