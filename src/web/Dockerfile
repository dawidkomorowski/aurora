FROM node:22 AS build-client
ARG ISSUES_SERVICE_API_URI=http://localhost:8201
WORKDIR /src/web

COPY Aurora.Web.Client ./Aurora.Web.Client/
WORKDIR /src/web/Aurora.Web.Client/

# It is a workaround for npm ci hanging. Remove once the problem is solved.
RUN npm cache clean --force

RUN npm ci
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-host
WORKDIR /src/web

COPY Aurora.Web.Host ./Aurora.Web.Host/
WORKDIR /src/web/Aurora.Web.Host/
RUN dotnet publish -c release -o /app


FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-host /app ./
COPY --from=build-client /src/web/Aurora.Web.Client/dist ./wwwroot
ENTRYPOINT [ "dotnet", "Aurora.Web.Host.dll" ]