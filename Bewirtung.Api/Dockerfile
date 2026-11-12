FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Bewirtung.Api/Bewirtung.Api.csproj Bewirtung.Api/
COPY Bewirtung.Fiskal/Bewirtung.Fiskal.csproj Bewirtung.Fiskal/
RUN dotnet restore Bewirtung.Api/Bewirtung.Api.csproj
COPY . .
RUN dotnet publish Bewirtung.Api/Bewirtung.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 5080
ENTRYPOINT ["dotnet", "Bewirtung.Api.dll"]
