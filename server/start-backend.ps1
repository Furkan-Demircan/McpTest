# API ve MCP projelerini aynı anda ayağa kaldırır
$serverDir = $PSScriptRoot

Write-Host "Starting API on background..." -ForegroundColor Cyan
$apiJob = Start-Process dotnet -ArgumentList "run --project `"$serverDir/src/API`"" -PassThru

Write-Host "Starting MCP on background..." -ForegroundColor Magenta
$mcpJob = Start-Process dotnet -ArgumentList "run --project `"$serverDir/src/MCP`"" -PassThru

Write-Host "`nBackend API ve MCP projeleri başlatıldı!" -ForegroundColor Green
Write-Host "API: http://localhost:5000 / http://localhost:52113" -ForegroundColor Yellow
Write-Host "MCP: http://localhost:5001" -ForegroundColor Yellow
