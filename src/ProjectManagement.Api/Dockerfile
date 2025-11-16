FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

USER $APP_UID
WORKDIR /app
EXPOSE 7000

ENV ASPNETCORE_URLS=https://+:7000
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/aspnetapp.pfx"

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

COPY ["src/ProjectManagement.Api/ProjectManagement.Api.csproj", "src/ProjectManagement.Api/"]
RUN dotnet restore "src/ProjectManagement.Api/ProjectManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/ProjectManagement.Api"
RUN dotnet build "./ProjectManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG CERT_PASSWORD=YourSecurePassword123!
RUN dotnet publish "./ProjectManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet dev-certs https -ep /app/publish/aspnetapp.pfx -p $CERT_PASSWORD

FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .
RUN chown -R $APP_UID:$APP_UID /app

USER $APP_UID
ENTRYPOINT ["dotnet", "ProjectManagement.Api.dll"]
