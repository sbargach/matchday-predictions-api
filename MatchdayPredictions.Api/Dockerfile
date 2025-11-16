FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MatchdayPredictions.Api/MatchdayPredictions.Api.csproj", "MatchdayPredictions.Api/"]
COPY ["MatchdayPredictions.Api.Tests/MatchdayPredictions.Api.Tests.csproj", "MatchdayPredictions.Api.Tests/"]
COPY ["MatchdayPredictions.Database/MatchdayPredictions.Database.sqlproj", "MatchdayPredictions.Database/"]

RUN dotnet restore "MatchdayPredictions.Api/MatchdayPredictions.Api.csproj"

COPY . .
WORKDIR "/src/MatchdayPredictions.Api"
RUN dotnet build "MatchdayPredictions.Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/MatchdayPredictions.Api"
RUN dotnet publish "MatchdayPredictions.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MatchdayPredictions.Api.dll"]
