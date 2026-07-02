FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY backend/src/Fundo.Domain/Fundo.Domain.csproj backend/src/Fundo.Domain/
COPY backend/src/Fundo.Application/Fundo.Application.csproj backend/src/Fundo.Application/
COPY backend/src/Fundo.Infrastructure/Fundo.Infrastructure.csproj backend/src/Fundo.Infrastructure/
COPY backend/src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj backend/src/Fundo.Applications.WebApi/

RUN dotnet restore backend/src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj

COPY backend/src/ backend/src/

RUN dotnet publish backend/src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]
