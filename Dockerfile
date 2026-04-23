# Imagen base para compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos del proyecto y restaurar dependencias
# Se copian por separado para aprovechar la caché de capas de Docker
COPY ["IntegradorIdeas.csproj", "./"]
RUN dotnet restore "IntegradorIdeas.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
RUN dotnet publish "IntegradorIdeas.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen base para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Asegurar que el usuario 'app' tenga permisos de escritura para el archivo SQLite
USER root
RUN mkdir -p /app && chown -R app:app /app && chmod -R 755 /app
USER app

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Development
# Variable estándar de .NET para detectar que estamos en un contenedor Docker
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 10000

ENTRYPOINT ["dotnet", "IntegradorIdeas.dll"]
