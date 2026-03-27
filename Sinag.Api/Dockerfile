FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Sinag.Shared/Sinag.Shared.csproj Sinag.Shared/
COPY Sinag.Api/Sinag.Api.csproj Sinag.Api/
RUN dotnet restore Sinag.Api/Sinag.Api.csproj

COPY Sinag.Shared/ Sinag.Shared/
COPY Sinag.Api/ Sinag.Api/
RUN dotnet publish Sinag.Api/Sinag.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Sinag.Api.dll"]
