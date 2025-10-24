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

# Kestrel escucha en el puerto 80 del contenedor
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .
EXPOSE 80

ENTRYPOINT ["dotnet", "PrimitiveClash.Backend.dll"]
