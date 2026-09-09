# Build Aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyasını kopyala ve restore et
COPY src/MCP/MCP.csproj src/MCP/
RUN dotnet restore src/MCP/MCP.csproj

# Kaynak kodları kopyala ve derle
COPY src/MCP/ src/MCP/
WORKDIR /app/src/MCP
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MCP.dll"]
