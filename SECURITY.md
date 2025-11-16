# 🔐 Guía de Seguridad - EmailsP

Este documento explica las mejoras de seguridad implementadas y mejores prácticas.

---

## ✅ Mejoras Implementadas

### 1. Separación de Credenciales

**Antes:** Las credenciales estaban hardcodeadas en `appsettings.json`

```json
"Password": "YzQVcps2vSa90eC7"  ❌ Visible en Git
```

**Ahora:** Las credenciales están en archivos `.env` (no se suben a Git)

```bash
POSTGRES_PASSWORD=YzQVcps2vSa90eC7  ✅ Seguro
```

### 2. Jerarquía de Configuración

ASP.NET Core lee la configuración en este orden (el último sobrescribe):

1. `appsettings.json` - Configuración base (SIN credenciales)
2. `appsettings.{Environment}.json` - Por ambiente
3. **Variables de entorno** - Máxima prioridad (Docker/servidor)

**Resultado:** Las credenciales en `.env` sobrescriben todo lo demás.

### 3. Archivos Protegidos en .gitignore

Estos archivos **NO se suben a Git**:

- `.env` - Credenciales de desarrollo
- `.env.production` - Credenciales de producción
- `appsettings.Development.json` - Config de desarrollo
- `appsettings.Production.json` - Config de producción
- `**/credentials/*.json` - Tokens de OAuth

Estos archivos **SÍ se suben a Git** (son plantillas):

- `.env.example` - Estructura sin credenciales reales
- `.env.production.example` - Plantilla para producción
- `appsettings.json` - Configuración base

---

## 🎯 Cómo Usar en Diferentes Entornos

### Desarrollo Local (sin Docker)

1. Copia el archivo de ejemplo:

   ```bash
   cp .env.example .env
   ```

2. Edita `.env` con tus credenciales locales

3. Ejecuta normalmente:
   ```bash
   dotnet run --project EmailsP/EmailsP.csproj
   ```

Las variables del `.env` **NO se cargan automáticamente** en .NET sin Docker.
Usa `appsettings.Development.json` en su lugar.

### Desarrollo con Docker

1. Asegúrate de que `.env` existe con tus credenciales

2. Ejecuta:
   ```bash
   docker-compose up -d
   ```

Docker Compose lee automáticamente el archivo `.env`.

### Producción

#### Opción 1: Archivo .env.production

```bash
# Crear archivo de producción
cp .env.production.example .env.production

# Editar con credenciales reales (NUNCA las de desarrollo)
nano .env.production

# Ejecutar con archivo específico
docker-compose --env-file .env.production up -d
```

#### Opción 2: Variables de Entorno del Sistema

En servidores, es mejor usar variables del sistema:

```bash
# Linux/macOS
export JWT_KEY="clave-super-secreta-de-produccion"
export POSTGRES_PASSWORD="password-fuerte-de-produccion"

# Windows PowerShell
$env:JWT_KEY="clave-super-secreta-de-produccion"
$env:POSTGRES_PASSWORD="password-fuerte-de-produccion"

# Luego ejecutar
docker-compose up -d
```

#### Opción 3: Servicios de Secretos (Recomendado para producción real)

- **Azure Key Vault**
- **AWS Secrets Manager**
- **Google Cloud Secret Manager**
- **HashiCorp Vault**

---

## 🔒 Mejores Prácticas de Seguridad

### 1. Contraseñas Fuertes

❌ **MAL:**

```bash
JWT_KEY="123456"
POSTGRES_PASSWORD="admin"
```

✅ **BIEN:**

```bash
JWT_KEY="9KpR7vX2nQ5wL8mT4zB6cF3hJ1gD0sA9pE7vX2nQ5wL8mT4zB6cF3hJ1gD"
POSTGRES_PASSWORD="Kj#9mP@2xL$5vN!8qW^3zA"
```

**Generar claves seguras:**

```bash
# Linux/macOS/Git Bash
openssl rand -base64 48

# PowerShell
-join ((48..122) | Get-Random -Count 48 | ForEach-Object {[char]$_})
```

### 2. Diferentes Credenciales por Ambiente

| Ambiente   | Credenciales                         |
| ---------- | ------------------------------------ |
| Desarrollo | Simples, en `.env` local             |
| Staging    | Similares a producción, BD de prueba |
| Producción | **Aleatorias, largas, únicas**       |

**Nunca uses las mismas credenciales en desarrollo y producción.**

### 3. Rotación de Credenciales

Cambia estas credenciales regularmente:

- **JWT Key:** Cada 3-6 meses (invalida tokens viejos)
- **Passwords de BD:** Cada 6 meses
- **App Passwords de Gmail:** Si se comprometen

### 4. Contraseñas de Aplicación de Gmail

⚠️ **NUNCA uses tu contraseña real de Gmail** en `SMTP_PASSWORD`.

**Crear App Password:**

1. Ve a https://myaccount.google.com/apppasswords
2. Crea una contraseña para "Aplicación de correo"
3. Usa esa password en `SMTP_PASSWORD`

**Ventajas:**

- Si se compromete, revocas solo esa app
- No expones tu contraseña real
- Funciona aunque tengas 2FA activado

### 5. Verificar qué se Sube a Git

**Antes de hacer commit:**

```bash
# Ver qué archivos vas a subir
git status

# Verificar que .env NO aparezca
git ls-files | grep .env

# Si .env aparece, removelo del índice:
git rm --cached .env
```

### 6. HTTPS en Producción

En producción, **SIEMPRE usa HTTPS**:

```bash
# En .env.production
ASPNETCORE_URLS=https://+:443;http://+:80
```

Necesitarás un certificado SSL. Opciones:

- **Let's Encrypt** (gratis)
- Certificado comprado
- Certificado del cloud provider

### 7. Limitar CORS en Producción

En `Program.cs`, cambia esto para producción:

```csharp
if (app.Environment.IsProduction())
{
    app.UseCors(policy => policy
        .WithOrigins("https://tudominio.com")  // Solo tu dominio
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
}
else
{
    app.UseCors("DevAll");  // Permisivo solo en desarrollo
}
```

---

## ⚠️ Qué Hacer si se Filtra una Credencial

### 1. Si subes .env a Git por error

```bash
# 1. Remover del staging
git rm --cached .env

# 2. Agregar a .gitignore (si no está)
echo ".env" >> .gitignore

# 3. Commit
git add .gitignore
git commit -m "Remove .env and add to gitignore"

# 4. IMPORTANTE: Las credenciales YA están en el historial de Git
# Necesitas cambiar TODAS las passwords que estaban en ese archivo
```

### 2. Cambiar credenciales comprometidas

1. **Base de datos:** Cambiar password del usuario
2. **JWT:** Generar nueva clave (invalida tokens existentes)
3. **Gmail:** Revocar App Password y crear uno nuevo
4. **Desplegar** con las nuevas credenciales

### 3. Limpiar historial de Git (avanzado)

Si el .env está en el historial:

```bash
# Usar BFG Repo-Cleaner
java -jar bfg.jar --delete-files .env

git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push --force
```

⚠️ **Cuidado:** Esto reescribe la historia. Coordina con tu equipo.

---

## 📋 Checklist de Seguridad

Antes de desplegar a producción:

- [ ] `.env` está en `.gitignore`
- [ ] `appsettings.json` NO tiene credenciales reales
- [ ] JWT Key tiene al menos 64 caracteres aleatorios
- [ ] Contraseñas de BD son fuertes y únicas
- [ ] Usando App Password de Gmail (no password real)
- [ ] HTTPS configurado (puerto 443)
- [ ] CORS limitado solo a tu dominio
- [ ] `RequireHttpsMetadata = true` en JWT (producción)
- [ ] Logs NO muestran passwords o tokens
- [ ] Backups de BD cifrados
- [ ] Firewall configurado (solo puertos necesarios)
- [ ] Rate limiting implementado (evitar spam/DDoS)

---

## 🔍 Auditoría de Seguridad

### Verificar archivos expuestos

```bash
# Buscar posibles credenciales en el código
git grep -i "password"
git grep -i "secret"
git grep -i "token"

# Ver qué archivos están en Git
git ls-files

# Verificar .gitignore
git check-ignore -v .env
```

### Logs seguros

Asegúrate de que los logs NO muestren credenciales:

```csharp
// ❌ MAL
_logger.LogInformation($"Connecting with password: {password}");

// ✅ BIEN
_logger.LogInformation("Connecting to database...");
```

---

## 📚 Recursos Adicionales

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/es-es/aspnet/core/security/)
- [Docker Security](https://docs.docker.com/engine/security/)
- [Git Secrets](https://github.com/awslabs/git-secrets)

---

## 🆘 Preguntas Frecuentes

### ¿Debo subir .env.example a Git?

**Sí**, es una plantilla sin credenciales reales.

### ¿Puedo tener múltiples archivos .env?

**Sí**: `.env`, `.env.production`, `.env.staging`, etc.

### ¿Cómo comparto credenciales con mi equipo?

Usa un **gestor de passwords** como:

- 1Password
- LastPass
- Bitwarden
- Azure Key Vault (para empresas)

**Nunca por email o chat.**

### ¿Cada desarrollador necesita su propio .env?

**Sí**, cada uno debe tener sus credenciales locales.

---

**Última actualización:** Noviembre 2025
