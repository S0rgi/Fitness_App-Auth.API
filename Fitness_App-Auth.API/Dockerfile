# Базовый образ
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копирование проекта и восстановление зависимостей
COPY ["Fitness_App-Auth.API/Fitness_App-Auth.API.csproj", "Fitness_App-Auth.API/"]
RUN dotnet restore "Fitness_App-Auth.API/Fitness_App-Auth.API.csproj"

# Копирование исходников
COPY . .
WORKDIR "/src/Fitness_App-Auth.API"

# Сборка проекта
RUN dotnet build "Fitness_App-Auth.API.csproj" -c Release -o /app/build

# Публикация
RUN dotnet publish "Fitness_App-Auth.API.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fitness_App-Auth.API.dll"]
