FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Bewirtung.Webserver/Bewirtung.Webserver.csproj Bewirtung.Webserver/
COPY Bewirtung.Fiskal/Bewirtung.Fiskal.csproj Bewirtung.Fiskal/
RUN dotnet restore Bewirtung.Webserver/Bewirtung.Webserver.csproj
COPY . .
RUN dotnet publish Bewirtung.Webserver/Bewirtung.Webserver.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# PostgreSQL-Client 18 (pg_dump/pg_restore) für Backup/Restore installieren.
# Muss >= Serverversion (18.x) sein -> offizielles PGDG-Repo einbinden.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
    && install -d /usr/share/postgresql-common/pgdg \
    && curl -fsSL https://www.postgresql.org/media/keys/ACCC4CF8.asc -o /usr/share/postgresql-common/pgdg/apt.postgresql.org.asc \
    && echo "deb [signed-by=/usr/share/postgresql-common/pgdg/apt.postgresql.org.asc] https://apt.postgresql.org/pub/repos/apt $(. /etc/os-release && echo $VERSION_CODENAME)-pgdg main" > /etc/apt/sources.list.d/pgdg.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client-18 \
    && apt-get purge -y --auto-remove curl gnupg \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bewirtung.Webserver.dll"]
