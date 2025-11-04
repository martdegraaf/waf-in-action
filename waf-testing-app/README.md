# WAF Testing Application

A .NET 8 Web API application designed to test Azure Application Gateway Web Application Firewall (WAF) capabilities.

## Features

This application provides endpoints that simulate various attack patterns to test WAF rules:

### Available Endpoints

| Endpoint | Purpose | WAF Rule Type |
|----------|---------|---------------|
| `GET /` | Health check and application info | Safe |
| `GET /health` | Health probe endpoint | Safe |
| `GET /api/waftest` | List all available test endpoints | Safe |
| `GET /api/waftest/safe` | Safe endpoint for baseline testing | Safe |
| `GET /api/waftest/sql-injection` | Test SQL injection protection | OWASP Core Rules |
| `GET /api/waftest/xss` | Test XSS protection | OWASP Core Rules |
| `GET /api/waftest/path-traversal` | Test path traversal protection | OWASP Core Rules |
| `POST /api/waftest/malicious-payload` | Test malicious payload detection | OWASP Core Rules |
| `GET /api/waftest/bot-simulation` | Test bot protection | Bot Manager Rules |
| `GET /api/waftest/large-request` | Test request size limits | Policy Settings |
| `GET /api/waftest/protocol-attack` | Test protocol anomaly detection | Protocol Rules |

### Example Test Requests

1. **SQL Injection Test:**
   ```
   GET /api/waftest/sql-injection?userId=1' OR '1'='1
   ```

2. **XSS Test:**
   ```
   GET /api/waftest/xss?comment=<script>alert('XSS')</script>
   ```

3. **Path Traversal Test:**
   ```
   GET /api/waftest/path-traversal?filePath=../../../etc/passwd
   ```

4. **Bot Simulation:**
   ```
   GET /api/waftest/bot-simulation
   Headers: User-Agent: BadBot/1.0
   ```

## Building and Deployment

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- Azure CLI
- Azure Container Registry

### Build and Push to ACR

1. **Create Azure Container Registry (if not exists):**
   ```powershell
   az acr create --name myacrname --resource-group myresourcegroup --sku Standard --admin-enabled true
   ```

2. **Build and push the container:**
   ```powershell
   .\build-and-push.ps1 -AcrName "myacrname"
   ```

3. **Update your Bicep template** to use the new image in the Container App configuration.

### Local Development

1. **Run locally:**
   ```bash
   dotnet run
   ```

2. **Access Swagger UI:**
   ```
   http://localhost:5000/swagger
   ```

## Integration with Application Gateway

This application is designed to work behind Azure Application Gateway with WAF enabled. The WAF should block malicious requests while allowing legitimate traffic through.

### Expected Behavior

- **Safe endpoints** should return 200 OK
- **Malicious requests** should be blocked by WAF with 403 Forbidden
- **Bot requests** should be handled according to Bot Manager rules
- **Large requests** should be blocked based on policy settings

## Monitoring

The application logs all requests with appropriate log levels:
- `Information` for safe requests
- `Warning` for potentially malicious requests

Use Azure Application Insights or Log Analytics to monitor the application and WAF behavior.

## Docker Commands

### Build locally:
```bash
docker build -t waf-testing-app .
```

### Run locally:
```bash
docker run -p 8080:8080 waf-testing-app
```

### Test locally:
```bash
curl http://localhost:8080/api/waftest/safe
```
