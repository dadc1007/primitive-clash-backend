# Script para generar reporte de cobertura de codigo
# Usarlo así: .\generate-coverage.ps1

Write-Host "Verificando Docker Desktop..." -ForegroundColor Cyan

# Verificar si Docker está corriendo
$dockerRunning = $false
try {
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -eq 0) {
        $dockerRunning = $true
        Write-Host "Docker Desktop está corriendo" -ForegroundColor Green
    }
} catch {
    $dockerRunning = $false
}

if (-not $dockerRunning) {
    Write-Host "Docker Desktop no está corriendo. Intentando iniciarlo..." -ForegroundColor Yellow
    
    # Iniciar Docker Desktop
    $dockerPath = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    if (Test-Path $dockerPath) {
        Start-Process $dockerPath
        Write-Host "Esperando a que Docker Desktop inicie (esto puede tardar 30-60 segundos)..." -ForegroundColor Yellow
        
        # Esperar hasta que Docker esté disponible 
        $maxAttempts = 24
        $attempts = 0
        $dockerReady = $false
        
        while ($attempts -lt $maxAttempts -and -not $dockerReady) {
            Start-Sleep -Seconds 5
            $attempts++
            try {
                $dockerInfo = docker info 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $dockerReady = $true
                    Write-Host "Docker Desktop está listo!" -ForegroundColor Green
                } else {
                    Write-Host "Esperando... ($attempts/$maxAttempts)" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "Esperando... ($attempts/$maxAttempts)" -ForegroundColor Yellow
            }
        }
        
        if (-not $dockerReady) {
            Write-Host "Docker Desktop no se pudo iniciar correctamente. Por favor, inícialo manualmente y vuelve a ejecutar el script." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "Docker Desktop no está instalado en la ruta esperada." -ForegroundColor Red
        Write-Host "Por favor, inicia Docker Desktop manualmente y vuelve a ejecutar el script." -ForegroundColor Red
        exit 1
    }
}

Write-Host "`nEjecutando tests con recoleccion de coverage..." -ForegroundColor Cyan

if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
    Write-Host "Reportes anteriores eliminados" -ForegroundColor Green
}

# Comando que ejecuta las test
dotnet test primitive-clash-backend.sln --collect:"XPlat Code Coverage" --results-directory:TestResults --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Los tests fallaron" -ForegroundColor Red
    exit 1
}

Write-Host "Tests ejecutados exitosamente" -ForegroundColor Green

# Verifica si está instalado ReportGenerator
$reportGenInstalled = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"

if (-not $reportGenInstalled) {
    Write-Host "Instalando ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Refrescar PATH
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","User") + ";" + [System.Environment]::GetEnvironmentVariable("Path","Machine")

Write-Host "Generando reporte HTML..." -ForegroundColor Cyan

# Generar reporte con exclusiones de Migrations y Program.cs,
# No las incluhimos en el reporte debido a que no representan logica de negocio
reportgenerator `
    -reports:"TestResults/**/coverage.cobertura.xml" `
    -targetdir:"TestResults/CoverageReport" `
    -reporttypes:"Html;MarkdownSummary;Badges" `
    -classfilters:"-PrimitiveClash.Backend.Migrations.*;-Program" `
    -filefilters:"-**/Migrations/**;-**/Program.cs"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generando el reporte" -ForegroundColor Red
    exit 1
}

Write-Host "Reporte generado en TestResults/CoverageReport" -ForegroundColor Green

# Resumen en consola
if (Test-Path "TestResults/CoverageReport/Summary.md") {
    Write-Host "`nResumen de Cobertura:" -ForegroundColor Cyan
    Get-Content "TestResults/CoverageReport/Summary.md"
}

# reporte en navegador
Write-Host "`nAbriendo reporte en el navegador..." -ForegroundColor Cyan
Start-Process "TestResults/CoverageReport/index.html"

Write-Host "`nProceso completado!" -ForegroundColor Green
