FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Bewirtung.Webserver/Bewirtung.Webserver.csproj Bewirtung.Webserver/
RUN dotnet restore Bewirtung.Webserver/Bewirtung.Webserver.csproj
COPY . .
RUN dotnet publish Bewirtung.Webserver/Bewirtung.Webserver.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bewirtung.Webserver.dll"]
