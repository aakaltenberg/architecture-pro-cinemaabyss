FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EventsService.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8082

ENV ASPNETCORE_URLS=http://+:8082

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EventsService.dll"]