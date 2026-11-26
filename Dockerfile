FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MatchdayPredictions.Api/MatchdayPredictions.Api.csproj MatchdayPredictions.Api/
RUN dotnet restore MatchdayPredictions.Api/MatchdayPredictions.Api.csproj

COPY MatchdayPredictions.Api/ MatchdayPredictions.Api/
RUN dotnet publish MatchdayPredictions.Api/MatchdayPredictions.Api.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MatchdayPredictions.Api.dll"]
