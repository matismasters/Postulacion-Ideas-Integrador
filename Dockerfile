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

# curl se usa para el healthcheck del compose (la imagen aspnet no lo trae).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Variables de entorno. El puerto y el environment se pueden sobreescribir desde compose
# (en el servidor de Codice: PORT=8080, ASPNETCORE_ENVIRONMENT=Production).
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IntegradorIdeas.dll"]
