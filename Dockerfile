# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/RealEstate.API/RealEstate.API.csproj", "src/RealEstate.API/"]
COPY ["src/RealEstate.Infrastructure/RealEstate.Infrastructure.csproj", "src/RealEstate.Infrastructure/"]
COPY ["src/RealEstate.Application/RealEstate.Application.csproj", "src/RealEstate.Application/"]
COPY ["src/RealEstate.Domain/RealEstate.Domain.csproj", "src/RealEstate.Domain/"]

# Restore dependencies
RUN dotnet restore "src/RealEstate.API/RealEstate.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/RealEstate.API"
RUN dotnet build "RealEstate.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealEstate.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RealEstate.API.dll"]

# .dockerignore
**/bin/
**/obj/
**/out/
**/.vs/
**/.vscode/
**/.idea/
**/*.user
**/.DS_Store
**/node_modules/
**/.git/
**/.gitignore
**/*.md
**/docker-compose*
**/.env
**/Dockerfile*