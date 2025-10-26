# Script para generar reporte de cobertura de codigo
# Uso: .\generate-coverage.ps1

Write-Host "Ejecutando tests con recoleccion de coverage..." -ForegroundColor Cyan

# Limpiar reportes anteriores
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
    Write-Host "Reportes anteriores eliminados" -ForegroundColor Green
}

# Ejecutar tests con coverage
dotnet test primitive-clash-backend.sln --collect:"XPlat Code Coverage" --results-directory:TestResults --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Los tests fallaron" -ForegroundColor Red
    exit 1
}

Write-Host "Tests ejecutados exitosamente" -ForegroundColor Green

# Verificar si ReportGenerator esta instalado
$reportGenInstalled = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"

if (-not $reportGenInstalled) {
    Write-Host "Instalando ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Refrescar PATH
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","User") + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine")

Write-Host "Generando reporte HTML..." -ForegroundColor Cyan

# Generar reporte
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:"Html;MarkdownSummary;Badges"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generando el reporte" -ForegroundColor Red
    exit 1
}

Write-Host "Reporte generado en TestResults/CoverageReport" -ForegroundColor Green

# Mostrar resumen en consola
if (Test-Path "TestResults/CoverageReport/Summary.md") {
    Write-Host "`nResumen de Cobertura:" -ForegroundColor Cyan
    Get-Content "TestResults/CoverageReport/Summary.md"
}

# Abrir reporte en navegador
Write-Host "`nAbriendo reporte en el navegador..." -ForegroundColor Cyan
Start-Process "TestResults/CoverageReport/index.html"

Write-Host "`nProceso completado!" -ForegroundColor Green
