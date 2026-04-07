# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY ProxyService.csproj .
RUN dotnet restore

# Копируем остальной код и собираем
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Этап выполнения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8000

ENV ASPNETCORE_URLS=http://+:8000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProxyService.dll"]