# Azure Application Gateway + WAF Demo - AI Agent Instructions

## Project Context
This is a **live security demonstration** showcasing WAF protection. NOT a typical web app - it's designed to **trigger WAF blocking** for cybersecurity education.

**Architecture**: `Internet → Application Gateway (WAF OWASP 3.2 + Bot Manager) → Container App (.NET API)`
**Live Demo**: `http://agw-agwdemo-vads3dyjksiac.hhgqashgc9ctazfc.westeurope.sysgen.cloudapp.azure.com/`

## Critical Development Patterns

### PowerShell Environment (IMPORTANT!)
- **Command separation**: Use `;` NOT `&&` (bash syntax)
- **Example**: `cd waf-testing-app; az acr build --registry myregistry --image app:v3 .`

### Container Deployment (CRITICAL)
**Always use versioned tags** (v1, v2, v3) - never `:latest`
```powershell
az acr build --registry acrdemoagwdemovads3dyj --image waf-testing-app:v3 .; az containerapp update --name ca-agwdemo-vads3dyjksiac --image acrdemoagwdemovads3dyj.azurecr.io/waf-testing-app:v3
```
**Why**: Container Apps require explicit tag changes for new revisions.

### WAF Testing Endpoints (`waf-testing-app/Controllers/WafTestController.cs`)
```csharp
[HttpGet("sql-injection")]
public IActionResult SqlInjectionTest([FromQuery] string userId = "")
{
    // Return OK - let WAF handle blocking
    return Ok(new { Status = "This should be blocked by WAF" });
}
```
**Pattern**: Endpoints accept malicious input, WAF blocks before reaching app.

### Static Files in Dockerfile
```dockerfile
COPY --from=publish /app/publish .
COPY wwwroot ./wwwroot  # CRITICAL for dashboard
```

## WAF Testing Priority
1. **SQL Injection** → **Guaranteed 403**
2. **XSS** → **Guaranteed 403**  
3. **Path Traversal** → **Guaranteed 403**
4. **Bot Simulation** → ⚠️ **May return 200 OK** (normal Bot Manager behavior)

**Demo Strategy**: Focus on OWASP rules (1-3) for reliable blocking demonstrations.

## Key Files
- `infra/main.bicep` - Complete infrastructure
- `waf-testing-app/Controllers/WafTestController.cs` - WAF testing logic
- `waf-testing-app/wwwroot/index.html` - Interactive dashboard
- `waf-testing-app/Dockerfile` - Container with static file handling
