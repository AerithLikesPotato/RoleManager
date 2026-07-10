FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["WebApplication2/WebApplication2/WebApplication2.csproj", "WebApplication2/WebApplication2/"]
RUN dotnet restore "WebApplication2/WebApplication2/WebApplication.csproj"

COPY . .
WORKDIR "/src/WebApplication2/WebApplication2"
RUN dotnet restore "WebApplication2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet build "WebApplication2.cproj" -c release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication2.dll"]