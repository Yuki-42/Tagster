# Setup

I haven't made a setup script yet, as the project is still being actively devloped. The project assumes a number of 
prerequisie steps and other applications are running.

## 1. Nginx

Nginx is crucial for the API to function properly, as it handles reverse proxying and SSL. 

```config
server {
    listen 443 ssl;

    server_name example.com;

    ssl_certificate your_certificate.crt;
    ssl_certificate_key your_private.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    client_max_body_size 250M;

    location / {
        proxy_pass http://127.0.0.1:8000;

        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```