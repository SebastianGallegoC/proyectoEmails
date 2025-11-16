# ===================================
# ETAPA 1: BUILD (Compilación)
# ===================================
# Usamos la imagen oficial de .NET SDK 8.0 que incluye todas las herramientas para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los archivos .csproj primero (aprovecha el cache de Docker)
# Esto significa que si solo cambias código, no se volverán a descargar los paquetes NuGet
COPY ["EmailsP/EmailsP.csproj", "EmailsP/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infraestructure/Infraestructure.csproj", "Infraestructure/"]

# Restauramos las dependencias (paquetes NuGet)
RUN dotnet restore "EmailsP/EmailsP.csproj"

# Ahora copiamos todo el código fuente
COPY . .

# Compilamos el proyecto en modo Release (optimizado para producción)
WORKDIR "/src/EmailsP"
RUN dotnet build "EmailsP.csproj" -c Release -o /app/build

# ===================================
# ETAPA 2: PUBLISH (Publicación)
# ===================================
FROM build AS publish
# Publicamos la aplicación (genera los archivos finales listos para ejecutar)
RUN dotnet publish "EmailsP.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===================================
# ETAPA 3: RUNTIME (Imagen final)
# ===================================
# Usamos una imagen más ligera que SOLO tiene el runtime de ASP.NET (no las herramientas de build)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Exponemos el puerto 8080 (puerto estándar para aplicaciones .NET en contenedores)
EXPOSE 8080

# Copiamos los archivos publicados desde la etapa anterior
COPY --from=publish /app/publish .

# Copiamos las credenciales de Gmail (necesarias para el servicio de email)
COPY ["Infraestructure/credentials/", "/app/credentials/"]

# Comando que se ejecuta cuando inicia el contenedor
ENTRYPOINT ["dotnet", "EmailsP.dll"]
