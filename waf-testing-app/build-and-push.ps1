# Build and Push WAF Testing App to Azure Container Registry
param(
    [Parameter(Mandatory=$true)]
    [string]$AcrName,
    
    [Parameter(Mandatory=$false)]
    [string]$ImageTag = "latest",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName
)

Write-Host "🚀 Building and pushing WAF Testing App to Azure Container Registry..." -ForegroundColor Green

# Check if Docker is running
try {
    docker version | Out-Null
} catch {
    Write-Error "Docker is not running. Please start Docker Desktop and try again."
    exit 1
}

# Login to Azure if not already logged in
$context = az account show 2>$null
if (!$context) {
    Write-Host "🔐 Logging in to Azure..." -ForegroundColor Yellow
    az login
}

# Get ACR login server
Write-Host "🔍 Getting ACR details..." -ForegroundColor Yellow
$acrLoginServer = az acr show --name $AcrName --query loginServer --output tsv 2>$null
if (!$acrLoginServer) {
    Write-Error "Could not find ACR '$AcrName'. Please check the name and your permissions."
    exit 1
}

Write-Host "📋 ACR Login Server: $acrLoginServer" -ForegroundColor Cyan

# Login to ACR
Write-Host "🔑 Logging in to Azure Container Registry..." -ForegroundColor Yellow
az acr login --name $AcrName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to login to ACR. Please check your permissions."
    exit 1
}

# Build the Docker image
$imageName = "$acrLoginServer/waf-testing-app:$ImageTag"
Write-Host "🔨 Building Docker image: $imageName" -ForegroundColor Yellow

docker build -t $imageName .

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed."
    exit 1
}

# Push the image to ACR
Write-Host "📤 Pushing image to ACR..." -ForegroundColor Yellow
docker push $imageName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker push failed."
    exit 1
}

Write-Host "✅ Successfully built and pushed image: $imageName" -ForegroundColor Green

# Output the image URL for use in deployment
Write-Host ""
Write-Host "🌐 Image URL for deployment:" -ForegroundColor Cyan
Write-Host $imageName -ForegroundColor White

Write-Host ""
Write-Host "📝 To update your Bicep template, use this image:" -ForegroundColor Cyan
Write-Host "image: '$imageName'" -ForegroundColor White

Write-Host ""
Write-Host "🧪 Test endpoints after deployment:" -ForegroundColor Yellow
Write-Host "   Safe endpoint: http://your-gateway-url/api/waftest/safe" -ForegroundColor Gray
Write-Host "   SQL injection: http://your-gateway-url/api/waftest/sql-injection?userId=1' OR '1'='1" -ForegroundColor Gray
Write-Host "   XSS test: http://your-gateway-url/api/waftest/xss?comment=<script>alert('XSS')</script>" -ForegroundColor Gray
