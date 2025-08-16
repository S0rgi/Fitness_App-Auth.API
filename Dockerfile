# Базовый образ
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копирование проекта и восстановление зависимостей
COPY Gainly_Auth_API/Gainly_Auth_API.csproj ./Gainly_Auth_API/
RUN dotnet restore "Gainly_Auth_API/Gainly_Auth_API.csproj"

# Копирование исходников
COPY . .
WORKDIR /src/Gainly_Auth_API

# Сборка проекта
RUN dotnet build "Gainly_Auth_API.csproj" -c Release -o /app/build

# Публикация
RUN dotnet publish "Gainly_Auth_API.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gainly_Auth_API.dll"]



