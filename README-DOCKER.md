# 🐳 Guía de Docker para EmailsP

Este documento explica cómo ejecutar el proyecto **EmailsP** usando Docker.

---

## 📋 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

1. **Docker Desktop** (incluye Docker y Docker Compose)

   - Windows/Mac: [Descargar Docker Desktop](https://www.docker.com/products/docker-desktop)
   - Linux: [Instalar Docker Engine](https://docs.docker.com/engine/install/)

2. Verificar la instalación:
   ```bash
   docker --version
   docker-compose --version
   ```

---

## 🔐 Configuración Inicial (IMPORTANTE)

### 1. Configurar Variables de Entorno

Antes de ejecutar, necesitas crear tu archivo `.env` con las credenciales:

```bash
# Copiar el archivo de ejemplo
cp .env.example .env

# Editar con tus credenciales reales
# En Windows:
notepad .env

# En Linux/macOS:
nano .env
```

**⚠️ IMPORTANTE:** El archivo `.env` contiene credenciales sensibles y **NO debe subirse a Git**.

### 2. Estructura de Archivos de Configuración

- **`.env`** - Tus credenciales reales (desarrollo local) - **NO en Git** ❌
- **`.env.example`** - Plantilla sin credenciales - **Sí en Git** ✅
- **`appsettings.json`** - Config base sin credenciales - **Sí en Git** ✅
- **`appsettings.Development.json`** - Config desarrollo - **NO en Git** ❌

📖 **Más información:** Lee [SECURITY.md](./SECURITY.md) para mejores prácticas.

---

## 🚀 Inicio Rápido

### Opción 1: Con PostgreSQL Local (Recomendado para desarrollo)

Este método levanta **TODO** en contenedores (tu app + PostgreSQL).

```bash
# 1. Configurar .env (ver sección anterior)
cp .env.example .env
# Edita .env con tus credenciales

# 2. Levantar los contenedores
docker-compose up -d

# 3. Ver los logs (opcional)
docker-compose logs -f

# 4. Tu API estará disponible en:
# http://localhost:5000
# http://localhost:5000/swagger
```

**¿Qué está pasando?**

- Docker lee las credenciales desde `.env`
- Se crea un contenedor con PostgreSQL
- Se compila y ejecuta tu aplicación .NET
- Ambos contenedores se conectan automáticamente

### Opción 2: Con Base de Datos Externa

Si quieres usar una base de datos externa:

```bash
# 1. Asegúrate de que .env tenga la CONNECTION_STRING correcta
# Ejemplo: CONNECTION_STRING=Host=servidor-externo.com;Database=...

# 2. Ejecutar
docker-compose -f docker-compose.external-db.yml up -d
```

---

## 📦 Comandos Útiles

### Ver contenedores activos

```bash
docker-compose ps
```

### Ver logs en tiempo real

```bash
# Todos los servicios
docker-compose logs -f

# Solo la API
docker-compose logs -f api

# Solo PostgreSQL
docker-compose logs -f postgres
```

### Detener los contenedores (sin eliminarlos)

```bash
docker-compose stop
```

### Detener y eliminar los contenedores

```bash
docker-compose down
```

### Detener y eliminar TODO (incluyendo volúmenes/datos)

```bash
docker-compose down -v
```

⚠️ **Cuidado**: Esto borra los datos de la base de datos local.

### Reconstruir la imagen (después de cambios en código)

```bash
docker-compose up -d --build
```

### Entrar a un contenedor (para debugging)

```bash
# Entrar a la API
docker exec -it emailsp_api /bin/bash

# Entrar a PostgreSQL
docker exec -it emailsp_postgres psql -U walteresvc -d ConexionCRUDWalter
```

---

## 🔧 Configuración

### Variables de Entorno

Las variables están definidas en `docker-compose.yml`. Si quieres personalizarlas:

1. Copia el archivo de ejemplo:

   ```bash
   cp .env.example .env
   ```

2. Edita `.env` con tus valores

3. Modifica `docker-compose.yml` para usar las variables del archivo `.env`

### Puertos

Por defecto:

- **API**: `http://localhost:5000`
- **PostgreSQL**: `localhost:5432`

Para cambiar el puerto de la API, edita en `docker-compose.yml`:

```yaml
ports:
  - "TU_PUERTO:8080" # Ejemplo: "3000:8080"
```

---

## 🗄️ Base de Datos

### Conectarse a PostgreSQL desde tu PC

```bash
# Usando psql
psql -h localhost -p 5432 -U walteresvc -d ConexionCRUDWalter

# Contraseña: YzQVcps2vSa90eC7
```

### Hacer backup de la base de datos

```bash
docker exec emailsp_postgres pg_dump -U walteresvc ConexionCRUDWalter > backup.sql
```

### Restaurar backup

```bash
docker exec -i emailsp_postgres psql -U walteresvc -d ConexionCRUDWalter < backup.sql
```

---

## 🚀 Despliegue en Servidor

### 1. Preparación

Sube estos archivos a tu servidor:

- `Dockerfile`
- `docker-compose.yml` (o `docker-compose.external-db.yml`)
- Todo el código fuente
- Carpeta `Infraestructure/credentials/`

### 2. En el servidor

```bash
# Clonar o subir el proyecto
cd /ruta/del/proyecto

# Construir y levantar
docker-compose up -d --build

# Verificar que todo esté corriendo
docker-compose ps
```

### 3. Configurar firewall/proxy inverso

Si usas **Nginx** o **Apache**, configura un proxy inverso al puerto 5000.

Ejemplo Nginx:

```nginx
server {
    listen 80;
    server_name tudominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

---

## 🐛 Solución de Problemas

### La aplicación no inicia

```bash
# Ver logs detallados
docker-compose logs api

# Verificar que PostgreSQL esté listo
docker-compose logs postgres
```

### Error de conexión a la base de datos

- Verifica que PostgreSQL esté corriendo: `docker-compose ps`
- Comprueba las credenciales en `docker-compose.yml`
- Asegúrate de que el `depends_on` esté configurado

### Puerto ya en uso

```bash
# En Windows, encontrar qué usa el puerto 5000
netstat -ano | findstr :5000

# Cambiar el puerto en docker-compose.yml
ports:
  - "OTRO_PUERTO:8080"
```

### Reconstruir desde cero

```bash
# Eliminar todo y empezar de nuevo
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

---

## 📚 Recursos

- [Documentación oficial de Docker](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET en Docker](https://learn.microsoft.com/es-es/dotnet/core/docker/introduction)

---

## ✅ Checklist de Despliegue

Antes de subir a producción:

- [ ] Cambiar `ASPNETCORE_ENVIRONMENT` a `Production`
- [ ] Usar contraseñas seguras (no las del ejemplo)
- [ ] Configurar HTTPS (certificado SSL)
- [ ] Habilitar `RequireHttpsMetadata = true` en JWT
- [ ] Configurar backups automáticos de la BD
- [ ] Revisar logs de seguridad
- [ ] Configurar límites de recursos (CPU/RAM) en docker-compose
- [ ] No exponer el puerto de PostgreSQL (5432) públicamente

---

## 🤝 Ayuda

Si tienes problemas:

1. Revisa los logs: `docker-compose logs -f`
2. Verifica que Docker Desktop esté corriendo
3. Asegúrate de tener los permisos necesarios
4. Consulta la sección de "Solución de Problemas" arriba
