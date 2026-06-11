# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY Ecommerce_Api.sln ./
COPY Ecommerce_Api/Ecommerce_Api.csproj Ecommerce_Api/
COPY Ecommerce_Entity/Ecommerce_Entity.csproj Ecommerce_Entity/
COPY Ecommerce_Service/Ecommerce_Service.csproj Ecommerce_Service/
COPY Ecommerce_Common/Ecommerce_Common.csproj Ecommerce_Common/

RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish Ecommerce_Api/Ecommerce_Api.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Ecommerce_Api.dll"]
