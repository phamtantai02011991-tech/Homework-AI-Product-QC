# ==============================================================================
# Script tự động Deploy trực tiếp lên MonsterASP.NET qua FTP
# Website: homework-gana.runasp.net (site102179)
# ==============================================================================

$ftpServer = "ftp://site102179.siteasp.net/wwwroot"
$username  = "site102179"
$password  = "2j!KtG6#?3qJ"
$sourceDir = "$PSScriptRoot\publish"

if (!(Test-Path $sourceDir)) {
    Write-Error "Thư mục publish chưa tồn tại. Hãy chạy package-for-monsterasp.ps1 trước."
    exit 1
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " ĐANG TỰ ĐỘNG DEPLOY LÊN MONSTERASP.NET (homework-gana)..." -ForegroundColor Yellow
Write-Host " Server: site102179.siteasp.net | Root: /wwwroot" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Xóa file iisstart.htm mặc định nếu có
try {
    $delReq = [System.Net.FtpWebRequest]::Create("$ftpServer/iisstart.htm")
    $delReq.Credentials = New-Object System.Net.NetworkCredential($username, $password)
    $delReq.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
    $delReq.UsePassive = $true
    $delRes = $delReq.GetResponse()
    $delRes.Close()
    Write-Host "--> Đã dọn dẹp file mặc định iisstart.htm trên server." -ForegroundColor Green
} catch {
    # Bỏ qua nếu không có file
}

# 2. Hàm tạo thư mục trên FTP nếu chưa có
function Ensure-FtpDirectory {
    param([string]$targetUrl)
    try {
        $req = [System.Net.FtpWebRequest]::Create($targetUrl)
        $req.Credentials = New-Object System.Net.NetworkCredential($username, $password)
        $req.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
        $req.UsePassive = $true
        $res = $req.GetResponse()
        $res.Close()
    } catch {
        # Đã tồn tại thư mục, bỏ qua
    }
}

# 3. Quét toàn bộ thư mục và file từ publish
$allDirs = Get-ChildItem -Path $sourceDir -Recurse -Directory
foreach ($dir in $allDirs) {
    $relativePath = $dir.FullName.Substring($sourceDir.Length).Replace("\", "/")
    $ftpDirUrl = "$ftpServer$relativePath"
    Ensure-FtpDirectory -targetUrl $ftpDirUrl
}

$allFiles = Get-ChildItem -Path $sourceDir -Recurse -File
$total = $allFiles.Count
$current = 0

Write-Host "`nĐang tải lên $total tệp ứng dụng..." -ForegroundColor Yellow

$webClient = New-Object System.Net.WebClient
$webClient.Credentials = New-Object System.Net.NetworkCredential($username, $password)

foreach ($file in $allFiles) {
    $current++
    $relativePath = $file.FullName.Substring($sourceDir.Length).Replace("\", "/")
    $targetFileUrl = "$ftpServer$relativePath"

    $percent = [Math]::Round(($current / $total) * 100)
    Write-Progress -Activity "Deploying to MonsterASP.NET" -Status "Uploading [$current/$total] $relativePath" -PercentComplete $percent

    try {
        $webClient.UploadFile($targetFileUrl, "STOR", $file.FullName) | Out-Null
        Write-Host "[$current/$total] Uploaded: $relativePath" -ForegroundColor DarkGray
    } catch {
        Write-Warning "Thử lại tải tệp: $relativePath ($($_.Exception.Message))"
        Start-Sleep -Milliseconds 500
        $webClient.UploadFile($targetFileUrl, "STOR", $file.FullName) | Out-Null
    }
}

$webClient.Dispose()

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " DEPLOY HOÀN TẤT LÊN MONSTERASP.NET THÀNH CÔNG!" -ForegroundColor Green
Write-Host " Domain: http://homework-gana.runasp.net" -ForegroundColor Cyan
Write-Host " Swagger: http://homework-gana.runasp.net/swagger" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Green
