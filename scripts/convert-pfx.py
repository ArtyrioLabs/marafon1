from cryptography.hazmat.primitives.serialization import pkcs12
from cryptography.hazmat.primitives import serialization
import sys

# Загружаємо PFX
pfx_file = 'certs/secret-nick.pfx'
pfx_password = b'SecretNick2025!'

try:
    with open(pfx_file, 'rb') as f:
        pfx_data = f.read()
    
    # Парсимо PFX
    private_key, certificate, additional_certificates = pkcs12.load_key_and_certificates(
        pfx_data, 
        pfx_password
    )
    
    # Экспортируємо сертифікат в PEM
    cert_pem = certificate.public_bytes(serialization.Encoding.PEM)
    with open('certs/certificate-export.pem', 'wb') as f:
        f.write(cert_pem)
    
    # Экспортируємо приватный ключ в PEM
    key_pem = private_key.private_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PrivateFormat.PKCS8,
        encryption_algorithm=serialization.NoEncryption()
    )
    with open('certs/private-key-export.pem', 'wb') as f:
        f.write(key_pem)
    
    print("✅ Certificate: certs/certificate-export.pem")
    print("✅ Private Key: certs/private-key-export.pem")
    print("\n✅ Files ready for AWS ACM import!")
    
except Exception as e:
    print(f"❌ Error: {e}")
    sys.exit(1)
