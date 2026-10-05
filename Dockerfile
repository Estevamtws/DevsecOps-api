# syntax=docker/dockerfile:1
#
# Dockerfile multi-stage da API de demonstracao do pipeline DevSecOps (C# / .NET 10).
# Etapa 1: compila e publica a aplicacao com o SDK do .NET.
# Etapa 2: copia apenas os binarios publicados para uma imagem de runtime minima,
#          rodando com um usuario sem privilegios.

# ---------- Etapa de build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /build

# Copia primeiro o arquivo de projeto para aproveitar o cache de restauracao do Docker
COPY src/DevSecOpsApi/DevSecOpsApi.csproj src/DevSecOpsApi/
RUN dotnet restore src/DevSecOpsApi/DevSecOpsApi.csproj

# Copia o restante do codigo-fonte e publica em modo Release
COPY src/DevSecOpsApi/ src/DevSecOpsApi/
RUN dotnet publish src/DevSecOpsApi/DevSecOpsApi.csproj -c Release -o /app/publish --no-restore

# ---------- Etapa final ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

COPY --from=build /app/publish ./

# A imagem oficial do ASP.NET ja traz o usuario "app" (UID 1654); usamos ele em vez de root
USER $APP_UID

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget --quiet --spider http://localhost:8080/actuator/health || exit 1

ENTRYPOINT ["dotnet", "api.dll"]
