# API ve dahili MCP uç noktasını aynı anda ayağa kaldırır
$serverDir = $PSScriptRoot

Write-Host "Starting API with hosted MCP endpoint..." -ForegroundColor Cyan
$apiJob = Start-Process dotnet -ArgumentList "run --project `"$serverDir/src/API`"" -PassThru

Write-Host "`nBackend API ve dahili MCP sunucusu başlatıldı!" -ForegroundColor Green
Write-Host "API: http://localhost:5000" -ForegroundColor Yellow
Write-Host "MCP: http://localhost:5000/mcp" -ForegroundColor Yellow