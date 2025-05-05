# Проверка наличия Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "❌ Docker не установлен или не добавлен в PATH."
    exit 1
}

# Проверка наличия docker-compose
if (-not (docker compose version)) {
    Write-Error "❌ Docker Compose V2 не найден. Убедись, что у тебя Docker версии 20.10+."
    exit 1
}

# Проверка наличия psql
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    Write-Error "❌ Утилита 'psql' не найдена. Установи PostgreSQL и добавь её в PATH."
    exit 1
}

# Установка пароля PostgreSQL (чтобы psql/createdb не спрашивал вручную)
$env:PGPASSWORD = "12345"

# Параметры подключения (именуем безопасно, чтобы не конфликтовать)
$dbHost = "localhost"
$dbPort = 5433
$dbUser = "postgres"
$dbPassword = "12345"
$dbName = "auth_db"

# Установка пароля PostgreSQL (чтобы psql/createdb не спрашивал вручную)
$env:PGPASSWORD = $dbPassword

# Проверка, существует ли база данных
Write-Host "`n🔍 Проверка существования базы данных '$dbName'..."
$checkDbCommand = "SELECT 1 FROM pg_database WHERE datname = '$dbName';"
$result = psql -h $dbHost -p $dbPort -U $dbUser -d postgres -c "$checkDbCommand" -t -A

if ($result -eq "1") {
    Write-Host "✅ База данных '$dbName' уже существует."
} else {
    Write-Host "📦 Создаём базу данных '$dbName'..."
    createdb -h $dbHost -p $dbPort -U $dbUser $dbName
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ База '$dbName' успешно создана."
    } else {
        Write-Error "❌ Ошибка при создании базы данных '$dbName'."
        exit 1
    }
}


# Запуск docker-compose
Write-Host "`n🚀 Запуск docker-compose..."
docker compose up --build -d

Write-Host "`n✅ Всё готово! Swagger будет доступен по адресу: http://localhost:5000/swagger"
