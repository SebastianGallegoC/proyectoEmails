# 🔐 Mejoras de Seguridad Implementadas

## Resumen de Cambios

Este documento resume todas las mejoras de seguridad realizadas en el proyecto EmailsP.

---

## ✅ Archivos Creados

### Configuración de Seguridad

- **`.env`** - Variables de entorno con credenciales reales (desarrollo)
- **`.env.example`** - Plantilla sin credenciales (se sube a Git)
- **`.env.production.example`** - Plantilla para producción
- **`SECURITY.md`** - Guía completa de seguridad

### Scripts de Ayuda

- **`setup.sh`** - Script de configuración inicial (Linux/macOS)
- **`setup.ps1`** - Script de configuración inicial (Windows)

---

## 📝 Archivos Modificados

### 1. `appsettings.json`

**Antes:** Contenía credenciales hardcodeadas

```json
"Password": "YzQVcps2vSa90eC7"
```

**Ahora:** Solo placeholders (seguro para Git)

```json
"Password": "your-app-password"
```

### 2. `appsettings.Development.json`

**Añadido:** Configuración completa para desarrollo local (no se sube a Git)

### 3. `docker-compose.yml`

**Antes:** Credenciales directamente en el archivo

```yaml
POSTGRES_PASSWORD: YzQVcps2vSa90eC7
```

**Ahora:** Lee desde archivo `.env`

```yaml
POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
```

### 4. `.gitignore`

**Añadido:**

- `.env` y variantes
- `appsettings.*.json` (excepto base)
- Archivos de credenciales
- Tokens y claves

### 5. `README-DOCKER.md`

**Añadido:** Sección de seguridad y configuración inicial

---

## 🔒 Principios de Seguridad Implementados

### 1. Separación de Secretos

❌ **Antes:** Credenciales en código fuente
✅ **Ahora:** Credenciales en variables de entorno

### 2. Diferentes Credenciales por Ambiente

- **Desarrollo:** `.env` local
- **Producción:** `.env.production` (diferentes contraseñas)

### 3. Protección en Git

- Archivos sensibles en `.gitignore`
- Solo plantillas en Git

### 4. Jerarquía de Configuración

```
appsettings.json (base)
  ↓
appsettings.{Environment}.json
  ↓
Variables de Entorno (.env)  ← Máxima prioridad
```

---

## 🎯 Cómo Funciona Ahora

### Desarrollo Local (sin Docker)

1. Credenciales en `appsettings.Development.json`
2. No se sube a Git
3. Cada desarrollador tiene sus propias credenciales

### Desarrollo con Docker

1. Credenciales en archivo `.env`
2. Docker Compose las lee automáticamente
3. Sobrescriben valores de `appsettings.json`

### Producción

1. Opción A: Archivo `.env.production`
2. Opción B: Variables de entorno del sistema
3. Opción C: Servicios de secretos (Azure Key Vault, etc.)

---

## 📊 Comparación Antes/Después

| Aspecto              | Antes                  | Ahora                      |
| -------------------- | ---------------------- | -------------------------- |
| Credenciales en Git  | ❌ Sí (inseguro)       | ✅ No (seguro)             |
| Passwords visibles   | ❌ En appsettings.json | ✅ En .env (no en Git)     |
| Diferentes ambientes | ❌ Mismas credenciales | ✅ Diferentes credenciales |
| Fácil de cambiar     | ❌ Editar código       | ✅ Editar .env             |
| Docker Compose       | ❌ Hardcoded           | ✅ Desde .env              |

---

## 🚀 Primeros Pasos

### Para Nuevos Desarrolladores

1. **Clonar el repositorio**

   ```bash
   git clone <repo-url>
   cd proyectoEmails
   ```

2. **Ejecutar script de setup**

   ```bash
   # Windows PowerShell
   .\setup.ps1

   # Linux/macOS/Git Bash
   bash setup.sh
   ```

3. **El script hará:**
   - Crear `.env` desde `.env.example`
   - Abrir el archivo para que edites las credenciales
   - Levantar Docker Compose

### Para Configuración Manual

1. **Crear archivo .env**

   ```bash
   cp .env.example .env
   ```

2. **Editar con tus credenciales**

   ```bash
   notepad .env  # Windows
   nano .env     # Linux/macOS
   ```

3. **Levantar Docker**
   ```bash
   docker-compose up -d
   ```

---

## ⚠️ Importante: Migración de Credenciales

Si ya tenías el proyecto configurado:

### 1. Respalda tus credenciales actuales

Copia tus valores de `appsettings.json`:

- ConnectionStrings
- JWT Key
- SMTP User/Password

### 2. Pégalas en el nuevo .env

```bash
cp .env.example .env
# Edita .env con tus credenciales respaldadas
```

### 3. Verifica que funcione

```bash
docker-compose up -d
docker-compose logs -f api
```

### 4. Si usas Git

```bash
# Verifica que .env NO esté en staging
git status

# Si aparece, removelo
git rm --cached .env
git rm --cached appsettings.Development.json
```

---

## 📚 Documentación Relacionada

- **[README-DOCKER.md](./README-DOCKER.md)** - Guía de uso de Docker
- **[SECURITY.md](./SECURITY.md)** - Mejores prácticas completas
- **[.env.example](./.env.example)** - Plantilla de variables
- **[.env.production.example](./.env.production.example)** - Plantilla para producción

---

## 🆘 Problemas Comunes

### "No puedo conectar a la base de datos"

- Verifica que `.env` tenga las credenciales correctas
- Si usas Docker, el host debe ser `postgres`, no `localhost`
- Si NO usas Docker, el host debe ser `localhost`

### "Las variables de .env no se cargan"

- Docker Compose lee `.env` automáticamente
- Para .NET sin Docker, usa `appsettings.Development.json`

### "Subí .env a Git por error"

1. Remover: `git rm --cached .env`
2. Verificar .gitignore: `git check-ignore -v .env`
3. **CAMBIAR todas las contraseñas** que estaban en ese archivo

---

## ✅ Checklist de Seguridad

Antes de hacer commit:

- [ ] `.env` está en `.gitignore`
- [ ] `appsettings.json` no tiene credenciales reales
- [ ] `git status` no muestra archivos sensibles
- [ ] Credenciales de producción son diferentes a desarrollo

Antes de desplegar:

- [ ] JWT Key es aleatorio y largo (64+ caracteres)
- [ ] Passwords de BD son fuertes
- [ ] Usando App Password de Gmail
- [ ] HTTPS configurado
- [ ] CORS limitado a tu dominio

---

**Fecha de implementación:** Noviembre 2025
**Versión:** 1.0
