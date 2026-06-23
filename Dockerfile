FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NICK2.0-core.Api/NICK2.0-core.Api.csproj NICK2.0-core.Api/
COPY NICK2.0-core.Application/NICK2.0-core.Application.csproj NICK2.0-core.Application/
COPY NICK2.0-core.Domain/NICK2.0-core.Domain.csproj NICK2.0-core.Domain/
COPY NICK2.0-core.Infrastructure/NICK2.0-core.Infrastructure.csproj NICK2.0-core.Infrastructure/
RUN dotnet restore NICK2.0-core.Api/NICK2.0-core.Api.csproj

COPY . ./
RUN dotnet publish NICK2.0-core.Api/NICK2.0-core.Api.csproj -c Release -o /app/publish --no-restore /m:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080

CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet NICK2.0-core.Api.dll"]
