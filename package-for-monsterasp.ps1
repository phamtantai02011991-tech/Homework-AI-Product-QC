# ==============================================================================
# Script đóng gói triển khai (Package for MonsterASP.NET Deployment)
# Dự án: AI Product Defect Inspection & Quality Control Assistant
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " [1/4] Bắt đầu Build Frontend React (Vite)..." -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

Set-Location -Path "$PSScriptRoot\frontend"
npm run build

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " [2/4] Sao chép tệp tĩnh Frontend vào wwwroot của Backend..." -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

$backendWwwroot = "$PSScriptRoot\backend\AIProductQC.API\wwwroot"
if (!(Test-Path $backendWwwroot)) {
    New-Item -ItemType Directory -Path $backendWwwroot -Force | Out-Null
}

# Đảm bảo thư mục uploads tồn tại
$uploadsDir = Join-Path $backendWwwroot "uploads"
if (!(Test-Path $uploadsDir)) {
    New-Item -ItemType Directory -Path $uploadsDir -Force | Out-Null
}

# Sao chép các tệp dist từ frontend sang wwwroot của backend
Copy-Item -Path "$PSScriptRoot\frontend\dist\*" -Destination $backendWwwroot -Recurse -Force

Write-Host "--> Đã sao chép toàn bộ giao diện React vào: $backendWwwroot" -ForegroundColor Green

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " [3/4] Biên dịch & Đóng gói ASP.NET Core Web API (Release)..." -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

$publishDir = "$PSScriptRoot\publish"
if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
}

Set-Location -Path "$PSScriptRoot"
dotnet publish "$PSScriptRoot\backend\AIProductQC.API\AIProductQC.API.csproj" -c Release -o $publishDir

Write-Host "--> Hoàn tất dotnet publish vào thư mục: $publishDir" -ForegroundColor Green

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " [4/4] Nén gói triển khai thành file ZIP cho MonsterASP.NET..." -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

$zipPath = "$PSScriptRoot\MonsterASP_Deploy_AIProductQC.zip"
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}

Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -Force

$zipFileInfo = Get-Item $zipPath
$zipSizeMb = [Math]::Round($zipFileInfo.Length / 1MB, 2)

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " THANH CONG! GÓI DEPLOY MONSTERASP ĐÃ SẴN SÀNG" -ForegroundColor Green
Write-Host " File ZIP : $zipPath" -ForegroundColor Cyan
Write-Host " Kích thước: $zipSizeMb MB" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Green
