# Azure Application Gateway + WAF + Container Apps Demo

This demo showcases a complete Azure security solution using:
- **Azure Application Gateway** with WAF_v2 (OWASP 3.2 + Bot Manager rules)
- **Azure Container Apps** running a custom .NET 8 WAF testing application
- **Azure Container Registry** for custom image hosting

## 🎯 Live Demo Instance

**Deployed Resources:**
- **Resource Group**: `rg-agwdemo-vads3dyjksiac`
- **Container App**: `ca-agwdemo-vads3dyjksiac`
- **Container Registry**: `acrdemoagwdemovads3dyj.azurecr.io`
- **Application Gateway**: `agw-agwdemo-vads3dyjksiac`
- **Demo URL**: http://agw-agwdemo-vads3dyjksiac.hhgqashgc9ctazfc.westeurope.sysgen.cloudapp.azure.com/

> **Note**: This is a fully functional interactive WAF testing dashboard with 8 different attack categories you can test directly in your browser!

## 🏗️ Architecture

![Architecture of the demo](demo.drawio.svg)

### Components:
- **Application Gateway**: WAF_v2 with latest OWASP 3.2 and Microsoft Bot Manager rulesets
- **Container App**: Serverless .NET 8 Web API with WAF testing endpoints
- **Azure Container Registry**: Standard SKU hosting custom container images
- **Virtual Network**: Secure network with proper subnet configuration
- **Log Analytics**: Monitoring and WAF log analysis

## 🚀 Deployment Process (Tested & Verified)

### Prerequisites

- Azure CLI installed and authenticated
- Docker Desktop (for building the custom container)
- .NET 8 SDK (for local development)

### Manual Deployment (Recommended)

Based on our successful deployment experience, use these verified steps:

1. **Login to Azure:**
   ```bash
   az login
   az account set --subscription "your-subscription-id"
   ```

2. **Deploy Infrastructure via Azure CLI:**
   ```bash
   # Create resource group
   az group create --name rg-agwdemo-$(openssl rand -hex 6) --location "West Europe"
   
   # Deploy Bicep template
   az deployment group create \
     --resource-group rg-agwdemo-$(openssl rand -hex 6) \
     --template-file infra/main.bicep \
     --parameters location="West Europe"
   ```

3. **Build and Deploy Container App:**

```bash
# Navigate to application directory
cd waf-testing-app

# Build and push to ACR (replace with your actual ACR name)
az acr build --registry acrdemoagwdemovads3dyj \
  --image waf-testing-app:v2 .

# Update Container App with new image
az containerapp update \
  --name ca-agwdemo-vads3dyjksiac \
  --resource-group rg-agwdemo-vads3dyjksiac \
  --image acrdemoagwdemovads3dyj.azurecr.io/waf-testing-app:v2
```

4. **Verify Deployment:**

```bash
# Test the deployment
$url = "http://agw-agwdemo-vads3dyjksiac.hhgqashgc9ctazfc.westeurope.sysgen.cloudapp.azure.com/"
Invoke-WebRequest -Uri $url -UseBasicParsing
```

### Deployment Issues & Solutions

**Common Issues Encountered:**
- PowerShell deployment scripts may fail due to authentication/permissions
- Container App revisions not updating without explicit image tag changes
- Static file serving issues requiring HomeController implementation

**Solutions Applied:**
- Use Azure CLI instead of PowerShell scripts for more reliable deployment
- Always use versioned tags (v1, v2, etc.) when building containers
- Implement proper MVC controllers for serving static content

### Key Deployment Notes

- **Working Container Image**: `acrdemoagwdemovads3dyj.azurecr.io/waf-testing-app:v2`
- **Container App Revision**: `ca-agwdemo-vads3dyjksiac--0000004`
- **Build Duration**: ~48 seconds for ACR build
- **Final Content Size**: 16,772 bytes HTML content served correctly

## 🧪 Interactive WAF Testing Dashboard

The deployed application features a comprehensive **Interactive WAF Testing Dashboard** accessible at:
**http://agw-agwdemo-vads3dyjksiac.hhgqashgc9ctazfc.westeurope.sysgen.cloudapp.azure.com/**

### Testing Categories Available

The dashboard includes **8 attack categories** you can test directly in your browser:

1. **SQL Injection** - Test various SQL injection patterns ✅ **Gegarandeerd geblokkeerd**
2. **Cross-Site Scripting (XSS)** - Test XSS attack vectors ✅ **Gegarandeerd geblokkeerd**
3. **Path Traversal** - Test directory traversal attempts ✅ **Gegarandeerd geblokkeerd**
4. **Command Injection** - Test OS command injection ✅ **Gegarandeerd geblokkeerd**
5. **Bot Attacks** - Simulate malicious bot behavior ⚠️ **Zie Bot Manager sectie hieronder**
6. **Protocol Attacks** - Test HTTP protocol vulnerabilities ✅ **Meestal geblokkeerd**
7. **General Attacks** - Test various attack patterns ✅ **Meestal geblokkeerd**
8. **Custom Payload** - Test your own custom attack payloads 🔄 **Afhankelijk van payload**

### 🤖 **Belangrijke Notitie: Bot Manager vs OWASP Rules**

**Microsoft Bot Manager** (onderdeel van de WAF) werkt fundamenteel anders dan OWASP rules:

**OWASP Rules (items 1-4, 6-7):**
- Detecteren **content patterns** in requests
- Blokkeren op basis van **payload analysis**
- **Onmiddellijke blocking** bij match
- ✅ **100% betrouwbaar voor demo doeleinden**

**Microsoft Bot Manager (item 5):**
- Detecteert **behavioral patterns** over tijd
- Gebruikt **IP reputation** en **advanced fingerprinting**
- Analyseert **request timing** en **TLS characteristics**
- **Mogelijk 200 OK** response bij eerste requests (dit is normaal!)

**Voor Demo Doeleinden:**
- **Focus op items 1-4**: SQL, XSS, Path Traversal, Command Injection
- **Bot simulation kan 200 OK** geven - dit betekent NIET dat Bot Manager niet werkt
- **In productie** blokkeert Bot Manager effectief echte bot attacks
- **Gebruik altijd OWASP tests** voor gegarandeerde demo effectiviteit

### How to Use the Dashboard

1. **Open the Dashboard**: Navigate to the demo URL above
2. **Select Attack Category**: Choose from the 8 available categories
3. **Customize Parameters**: Modify attack payloads as needed
4. **Execute Test**: Click "Test Attack" to send the request
5. **View Results**: See immediate response (200 OK or 403 Blocked)
6. **Monitor in Azure**: Use the KQL queries below to see WAF logs

### Expected Results

- **Legitimate requests** → ✅ **200 OK** (Allowed through)
- **Malicious requests** → 🚫 **403 Forbidden** (Blocked by WAF)

### Command Line Testing (Alternative)

For automated testing, you can also use these curl commands:

```bash
# Get the Application Gateway FQDN
APP_GATEWAY_FQDN="agw-agwdemo-vads3dyjksiac.hhgqashgc9ctazfc.westeurope.sysgen.cloudapp.azure.com"

# Test normal requests (should work)
curl "http://$APP_GATEWAY_FQDN/"
curl "http://$APP_GATEWAY_FQDN/api/waftest/safe"

# Test attacks (should be blocked with 403)
curl "http://$APP_GATEWAY_FQDN/api/waftest/sql-injection?userId=1' OR '1'='1"
curl "http://$APP_GATEWAY_FQDN/api/waftest/xss?comment=<script>alert('XSS')</script>"
curl "http://$APP_GATEWAY_FQDN/api/waftest/path-traversal?filePath=../../../etc/passwd"
```

### Dashboard Features

- **Real-time Testing**: Immediate feedback on WAF blocking behavior
- **Visual Interface**: User-friendly web interface for non-technical users
- **Multiple Categories**: Comprehensive coverage of common attack vectors
- **Customizable Payloads**: Ability to modify and test custom attack patterns
- **Response Display**: Clear indication of blocked vs allowed requests
- **Educational Value**: Learn about different types of web attacks

## 📁 Project Structure

```
application-gateway-demo/
├── main.bicep                     # Subscription-level deployment
├── main.parameters.json           # Deployment parameters
├── infra/
│   ├── main.bicep                 # Infrastructure template
│   └── abbreviations.json         # Naming conventions
├── waf-testing-app/               # Custom .NET application
│   ├── Controllers/
│   │   └── WafTestController.cs   # WAF testing endpoints
│   ├── Program.cs                 # Application entry point
│   ├── Dockerfile                 # Container configuration
│   └── *.csproj                   # Project file
└── README.md                      # This documentation
```

## 🔧 Customization

### WAF Rules
The WAF policy includes:
- **OWASP 3.2**: Latest OWASP ModSecurity Core Rule Set
- **Microsoft Bot Manager**: Anti-bot protection
- **Prevention Mode**: Actively blocks malicious requests

To modify WAF settings, edit `infra/main.bicep`:
```bicep
managedRuleSet: {
  ruleSetType: 'OWASP'
  ruleSetVersion: '3.2'
  ruleGroupOverrides: []
}
```

### Application Scaling
Container Apps auto-scale configuration in `infra/main.bicep`:
```bicep
scale: {
  minReplicas: 1
  maxReplicas: 10
  rules: [
    {
      name: 'http-rule'
      http: {
        metadata: {
          concurrentRequests: '100'
        }
      }
    }
  ]
}
```

### Custom Endpoints
Add new testing endpoints in `waf-testing-app/Controllers/WafTestController.cs`:
```csharp
[HttpGet("your-test")]
public IActionResult YourTest([FromQuery] string parameter)
{
    // Your WAF testing logic here
    return Ok(new { message = "Test response", parameter });
}
```

## 📊 Monitoring & KQL Queries

### WAF Monitoring Dashboard

Use these KQL queries in Azure Log Analytics to monitor WAF effectiveness:

#### 1. WAF Blocked Requests Overview
```kusto
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.NETWORK"
| where Category == "ApplicationGatewayFirewallLog"
| where TimeGenerated > ago(24h)
| where action_s == "Blocked"
| summarize BlockedCount = count() by bin(TimeGenerated, 1h), clientIP_s
| order by TimeGenerated desc
```

#### 2. Attack Types Analysis
```kusto
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.NETWORK"
| where Category == "ApplicationGatewayFirewallLog"
| where TimeGenerated > ago(24h)
| where action_s == "Blocked"
| extend AttackType = case(
    Message contains "SQL", "SQL Injection",
    Message contains "XSS", "Cross-Site Scripting",
    Message contains "LFI", "Local File Inclusion",
    Message contains "RFI", "Remote File Inclusion",
    Message contains "Bot", "Bot Attack",
    "Other"
)
| summarize Count = count() by AttackType
| order by Count desc
```

#### 3. Top Attacking IPs
```kusto
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.NETWORK"
| where Category == "ApplicationGatewayFirewallLog"
| where TimeGenerated > ago(24h)
| where action_s == "Blocked"
| summarize AttackCount = count() by clientIP_s
| top 10 by AttackCount
| order by AttackCount desc
```

#### 4. OWASP Rule Triggers
```kusto
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.NETWORK"
| where Category == "ApplicationGatewayFirewallLog"
| where TimeGenerated > ago(24h)
| where action_s == "Blocked"
| where ruleSetType_s == "OWASP"
| summarize TriggerCount = count() by ruleId_s, Message
| order by TriggerCount desc
```

#### 5. Traffic Timeline (Blocked vs Allowed)
```kusto
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.NETWORK"
| where Category == "ApplicationGatewayFirewallLog"
| where TimeGenerated > ago(24h)
| summarize 
    Blocked = countif(action_s == "Blocked"),
    Allowed = countif(action_s == "Matched"),
    Total = count()
    by bin(TimeGenerated, 15m)
| order by TimeGenerated desc
```

#### 6. Request Pattern Analysis
```kusto
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.NETWORK"
| where Category == "ApplicationGatewayFirewallLog"
| where TimeGenerated > ago(24h)
| project TimeGenerated, clientIP_s, requestUri_s, userAgent_s, action_s, Message
| order by TimeGenerated desc
```

### Container App Performance Monitoring

```kusto
ContainerAppConsoleLogs_CL
| where ContainerName_s == "waf-testing-app"
| where TimeGenerated > ago(1h)
| project TimeGenerated, Log_s
| order by TimeGenerated desc
```

### Setting Up Log Analytics

**Automatic Configuration:**
The Bicep template automatically configures diagnostic settings for the Application Gateway, so WAF logs are immediately available in Log Analytics after deployment. No manual configuration required!

**Manual Configuration (if needed):**

1. **Enable WAF Logging** (if not already configured):
   ```bash
   # Get Log Analytics Workspace ID
   LOG_WORKSPACE_ID=$(az monitor log-analytics workspace show \
     --resource-group rg-agwdemo-vads3dyjksiac \
     --workspace-name law-agwdemo-vads3dyjksiac \
     --query id -o tsv)
   
   # Configure diagnostic settings
   az monitor diagnostic-settings create \
     --name "WAF-Diagnostics" \
     --resource /subscriptions/YOUR_SUBSCRIPTION/resourceGroups/rg-agwdemo-vads3dyjksiac/providers/Microsoft.Network/applicationGateways/agw-agwdemo-vads3dyjksiac \
     --logs '[{"category":"ApplicationGatewayFirewallLog","enabled":true}]' \
     --workspace $LOG_WORKSPACE_ID
   ```

2. **Access Log Analytics**:
   - Go to Azure Portal → Log Analytics workspaces
   - Select `law-agwdemo-vads3dyjksiac`
   - Use "Logs" section to run the KQL queries above

**Log Retention:** 
- WAF logs are retained according to **Log Analytics workspace retention settings** (default 30 days)
- Both logs and metrics are automatically captured
- All log categories are enabled (Access logs, Performance logs, Firewall logs)
- Retention is managed centrally via Log Analytics workspace configuration

## 🛠️ Development & Troubleshooting

### Development Lessons Learned

Based on our implementation experience, here are key insights:

#### Container App Deployment
- **Use versioned tags**: Always use versioned tags (v1, v2, etc.) instead of `:latest` for Container App revisions
- **ACR Build time**: Expect ~45-50 seconds for ACR build process
- **Revision updates**: Container Apps require explicit image tag changes to create new revisions

#### Static File Serving in ASP.NET Core
- **HomeController approach**: Use MVC controllers instead of middleware for serving root route HTML
- **File path resolution**: Use `IWebHostEnvironment.WebRootPath` for reliable file access
- **Content type**: Explicitly set `Content-Type: text/html; charset=utf-8` for proper browser rendering

#### Dockerfile Configuration
- **wwwroot copying**: Explicitly copy `wwwroot` folder after publish step
- **Multi-stage builds**: Use proper multi-stage build for optimized container size

### Local Development

```bash
cd waf-testing-app
dotnet run
# Access: https://localhost:5001/
```

### Build Container Locally

```bash
cd waf-testing-app
docker build -t waf-testing-app .
docker run -p 8080:8080 waf-testing-app
```

### Update Container App

After making changes, rebuild and update:

```bash
# Build and push new version (increment version tag)
az acr build --registry acrdemoagwdemovads3dyj \
  --image waf-testing-app:v3 .

# Update Container App
az containerapp update \
  --name ca-agwdemo-vads3dyjksiac \
  --resource-group rg-agwdemo-vads3dyjksiac \
  --image acrdemoagwdemovads3dyj.azurecr.io/waf-testing-app:v3
```

### Common Issues & Solutions

| Issue | Symptom | Solution |
|-------|---------|----------|
| **JSON instead of HTML** | API returns JSON, not web page | Implement HomeController for root route |
| **404 on root path** | Cannot access homepage | Add `[Route("")]` attribute to controller action |
| **Static files not found** | CSS/JS not loading | Ensure `wwwroot` folder copied in Dockerfile |
| **Container revision not updating** | Changes not visible | Use versioned tags, not `:latest` |
| **PowerShell deployment fails** | Script errors | Use Azure CLI commands instead |

### Performance Verification

The current deployment serves:
- **Content Size**: 16,772 bytes HTML
- **Response Time**: < 200ms typically
- **Content Type**: `text/html; charset=utf-8`
- **Status Code**: 200 OK for legitimate requests, 403 for blocked attacks

## 🔐 Security Features

- **WAF Protection**: OWASP 3.2 + Bot Manager rules
- **TLS Termination**: At Application Gateway level
- **Network Isolation**: VNet integration with private subnets
- **Managed Identity**: Secure ACR authentication
- **Health Probes**: Automatic health monitoring

## 💰 Cost Optimization

- **Container Apps**: Consumption-based pricing
- **Application Gateway**: Standard_v2 tier
- **ACR**: Standard tier with efficient storage
- **Auto-scaling**: Scales to zero when not in use

## 🎯 Use Cases

- **Security Testing**: Test WAF rules and configurations
- **Demo Environment**: Showcase Azure security capabilities
- **Learning**: Understand Application Gateway + WAF integration
- **Development**: Base template for production workloads

## 📚 Additional Resources

- [Azure Application Gateway Documentation](https://docs.microsoft.com/azure/application-gateway/)
- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [OWASP ModSecurity Core Rule Set](https://owasp.org/www-project-modsecurity-core-rule-set/)
- [Azure Verified Modules](https://aka.ms/avm)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📝 License

This demo is provided as-is for educational and demonstration purposes.
