FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/services

COPY . ./issues-service/
WORKDIR /src/services/issues-service/Aurora.IssuesService.Host
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT [ "dotnet", "Aurora.IssuesService.Host.dll" ]