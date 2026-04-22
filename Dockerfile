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

# Asegurar que el usuario 'app' tenga permisos sobre la carpeta (para SQLite)
USER root
RUN chown -R app:app /app
USER app

# Exponer el puerto que usará Render
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 10000

ENTRYPOINT ["dotnet", "IntegradorIdeas.dll"]
