FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY AuthenticationService.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "AuthenticationService.dll"]

# Librerias para agregar
# JWT 
# SQL SERVER y Tools
# dotnet add package Microsoft.EntityFrameworkCore.SqlServer
# dotnet add package Microsoft.EntityFrameworkCore.Tools

# Solo para entorno de desarrollo ejecutar y colocar la clave secreta 
# dotnet user-secrets init
# dotnet user-secrets set "JwtSettings:SecretKey" "zY7BxE84TUiWYtPQk2sJ9vL5FdHRg5VkjxVqM2yV3Wk"
