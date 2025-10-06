# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os csproj para restauração (contexto é a raiz do repo)
COPY src/FCG.Users.Api/FCG.Users.Api.csproj FCG.Users.Api/
COPY src/FCG.Users.Application/FCG.Users.Application.csproj FCG.Users.Application/
COPY src/FCG.Users.Domain/FCG.Users.Domain.csproj FCG.Users.Domain/
COPY src/FCG.Users.Infra/FCG.Users.Infra.csproj FCG.Users.Infra/
RUN dotnet restore FCG.Users.Api/FCG.Users.Api.csproj

# Copia o restante do código
COPY src/ ./
RUN dotnet publish FCG.Users.Api/FCG.Users.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FCG.Users.Api.dll"]
