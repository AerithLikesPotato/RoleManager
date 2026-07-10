# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore first (better layer caching)
COPY ["WebApplication2/WebApplication2/WebApplication2.csproj", "./"]
RUN dotnet restore "./WebApplication2.csproj"

# Copy the rest of the source and publish
COPY ["WebApplication2/WebApplication2/", "./"]
RUN dotnet publish "./WebApplication2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApplication2.dll"]

