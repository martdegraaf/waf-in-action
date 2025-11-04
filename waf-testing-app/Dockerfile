FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["waf-testing-app.csproj", "."]
RUN dotnet restore "./waf-testing-app.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "waf-testing-app.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "waf-testing-app.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Explicitly copy wwwroot to ensure static files are included
COPY wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "waf-testing-app.dll"]
