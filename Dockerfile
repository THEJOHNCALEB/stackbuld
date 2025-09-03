FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/Stackbuld.ProductOrdering.Api/*.csproj ./src/Stackbuld.ProductOrdering.Api/
COPY src/Stackbuld.ProductOrdering.Application/*.csproj ./src/Stackbuld.ProductOrdering.Application/
COPY src/Stackbuld.ProductOrdering.Domain/*.csproj ./src/Stackbuld.ProductOrdering.Domain/
COPY src/Stackbuld.ProductOrdering.Infrastructure/*.csproj ./src/Stackbuld.ProductOrdering.Infrastructure/

RUN dotnet restore src/Stackbuld.ProductOrdering.Api/Stackbuld.ProductOrdering.Api.csproj

COPY . .

RUN dotnet build src/Stackbuld.ProductOrdering.Api/Stackbuld.ProductOrdering.Api.csproj -c Release --no-restore

RUN dotnet publish src/Stackbuld.ProductOrdering.Api/Stackbuld.ProductOrdering.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Stackbuld.ProductOrdering.Api.dll"]
