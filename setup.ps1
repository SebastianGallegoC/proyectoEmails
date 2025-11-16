# ============================================
# Script de Setup para EmailsP (PowerShell)
# ============================================
# Este script te ayuda a configurar el proyecto
# por primera vez de forma segura
# ============================================

$ErrorActionPreference = "Stop"

Write-Host "🚀 Configuración inicial de EmailsP" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker esté instalado
try {
    docker --version | Out-Null
    Write-Host "✅ Docker está instalado" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker no está instalado" -ForegroundColor Red
    Write-Host "   Descárgalo desde: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

try {
    docker-compose --version | Out-Null
} catch {
    Write-Host "❌ Docker Compose no está instalado" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Crear archivo .env si no existe
if (-Not (Test-Path .env)) {
    Write-Host "📝 Creando archivo .env desde plantilla..." -ForegroundColor Yellow
    Copy-Item .env.example .env
    Write-Host "✅ Archivo .env creado" -ForegroundColor Green
    Write-Host ""
    Write-Host "⚠️  IMPORTANTE: Edita el archivo .env con tus credenciales reales" -ForegroundColor Yellow
    Write-Host "   - JWT_KEY: Genera una clave segura de al menos 32 caracteres"
    Write-Host "   - POSTGRES_PASSWORD: Cambia la contraseña por defecto"
    Write-Host "   - SMTP_PASSWORD: Usa un App Password de Gmail"
    Write-Host ""
    
    # Abrir el archivo en el editor por defecto
    notepad .env
    
    Write-Host "Presiona Enter cuando hayas guardado los cambios en .env..." -ForegroundColor Cyan
    Read-Host
} else {
    Write-Host "✅ Archivo .env ya existe" -ForegroundColor Green
}

Write-Host ""
Write-Host "🏗️  Opciones de inicio:" -ForegroundColor Cyan
Write-Host "1) PostgreSQL local (recomendado para desarrollo)"
Write-Host "2) Base de datos externa"
$option = Read-Host "Selecciona una opción (1 o 2)"

Write-Host ""

if ($option -eq "1") {
    Write-Host "🐘 Levantando PostgreSQL local + API..." -ForegroundColor Yellow
    docker-compose up -d
} elseif ($option -eq "2") {
    Write-Host "🌐 Levantando API con BD externa..." -ForegroundColor Yellow
    Write-Host "   Asegúrate de que CONNECTION_STRING en .env apunte a tu servidor externo"
    docker-compose -f docker-compose.external-db.yml up -d
} else {
    Write-Host "❌ Opción inválida" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "⏳ Esperando a que los contenedores inicien..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "📊 Estado de los contenedores:" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "✅ ¡Configuración completada!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Tu API está disponible en:" -ForegroundColor Cyan
Write-Host "   - http://localhost:5000"
Write-Host "   - http://localhost:5000/swagger"
Write-Host ""
Write-Host "📝 Comandos útiles:" -ForegroundColor Cyan
Write-Host "   - Ver logs:        docker-compose logs -f"
Write-Host "   - Detener:         docker-compose down"
Write-Host "   - Reconstruir:     docker-compose up -d --build"
Write-Host ""
Write-Host "📚 Más información:" -ForegroundColor Cyan
Write-Host "   - README-DOCKER.md - Guía de uso"
Write-Host "   - SECURITY.md      - Mejores prácticas de seguridad"
Write-Host ""
