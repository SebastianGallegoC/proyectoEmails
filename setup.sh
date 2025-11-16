#!/bin/bash

# ============================================
# Script de Setup para EmailsP
# ============================================
# Este script te ayuda a configurar el proyecto
# por primera vez de forma segura
# ============================================

set -e  # Salir si hay errores

echo "🚀 Configuración inicial de EmailsP"
echo "===================================="
echo ""

# Verificar que Docker esté instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker no está instalado"
    echo "   Descárgalo desde: https://www.docker.com/products/docker-desktop"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose no está instalado"
    exit 1
fi

echo "✅ Docker está instalado"
echo ""

# Crear archivo .env si no existe
if [ ! -f .env ]; then
    echo "📝 Creando archivo .env desde plantilla..."
    cp .env.example .env
    echo "✅ Archivo .env creado"
    echo ""
    echo "⚠️  IMPORTANTE: Edita el archivo .env con tus credenciales reales"
    echo "   - JWT_KEY: Genera una clave segura de al menos 32 caracteres"
    echo "   - POSTGRES_PASSWORD: Cambia la contraseña por defecto"
    echo "   - SMTP_PASSWORD: Usa un App Password de Gmail"
    echo ""
    read -p "Presiona Enter cuando hayas editado .env..." 
else
    echo "✅ Archivo .env ya existe"
fi

echo ""
echo "🏗️  Opciones de inicio:"
echo "1) PostgreSQL local (recomendado para desarrollo)"
echo "2) Base de datos externa"
read -p "Selecciona una opción (1 o 2): " option

echo ""

if [ "$option" == "1" ]; then
    echo "🐘 Levantando PostgreSQL local + API..."
    docker-compose up -d
elif [ "$option" == "2" ]; then
    echo "🌐 Levantando API con BD externa..."
    echo "   Asegúrate de que CONNECTION_STRING en .env apunte a tu servidor externo"
    docker-compose -f docker-compose.external-db.yml up -d
else
    echo "❌ Opción inválida"
    exit 1
fi

echo ""
echo "⏳ Esperando a que los contenedores inicien..."
sleep 5

echo ""
echo "📊 Estado de los contenedores:"
docker-compose ps

echo ""
echo "✅ ¡Configuración completada!"
echo ""
echo "🌐 Tu API está disponible en:"
echo "   - http://localhost:5000"
echo "   - http://localhost:5000/swagger"
echo ""
echo "📝 Comandos útiles:"
echo "   - Ver logs:        docker-compose logs -f"
echo "   - Detener:         docker-compose down"
echo "   - Reconstruir:     docker-compose up -d --build"
echo ""
echo "📚 Más información:"
echo "   - README-DOCKER.md - Guía de uso"
echo "   - SECURITY.md      - Mejores prácticas de seguridad"
echo ""
