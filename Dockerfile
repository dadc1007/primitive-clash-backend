FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copia csproj y restaura dependencias
COPY ["PrimitiveClash.Backend.csproj", "./"]
RUN dotnet restore "PrimitiveClash.Backend.csproj"

# copia todo y publica
COPY . .
RUN dotnet publish "PrimitiveClash.Backend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Instalar wget para healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Kestrel escucha en el puerto 80 del contenedor
ENV ASPNETCORE_URLS=http://+:80
# Configuración para Production en contenedor
ENV ASPNETCORE_ENVIRONMENT=Production
# Deshabilitar IPv6 para evitar problemas de DNS
ENV DOTNET_System__Net__Sockets__IPv6Enabled=false

COPY --from=build /app/publish .
EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "PrimitiveClash.Backend.dll"]
