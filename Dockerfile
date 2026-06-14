FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NICK2.0-core.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish NICK2.0-core.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080

CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet NICK2.0-core.dll"]
