FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for better cache
COPY ["WebApplication2/WebApplication2.csproj", "WebApplication2/WebApplication2.csproj"]
RUN dotnet restore "WebApplication2/WebApplication2.csproj"

# Copy everything else
COPY . .

# Publish from the correct project directory
RUN dotnet publish "WebApplication2/WebApplication2.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApplication2.dll"]


