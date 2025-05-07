# 🛡️ Fitness App Auth API

**Fitness App Auth API** — это микросервис аутентификации для фитнес-приложения. Поддерживает регистрацию и вход по JWT, а также защиту эндпоинтов с помощью API-ключей.

## 🚀 Возможности

- Регистрация и авторизация пользователей через JWT
- API-ключи для дополнительной безопасности
- Защищённые эндпоинты с middleware
- Swagger UI с поддержкой авторизации
- PostgreSQL база данных
- Готов к деплою через Fly.io
- Хранение секретов через переменные окружения

## 📁 Структура проекта

```plaintext
.
├── Fitness_App-Auth.API/
│   ├── Controllers/
│   ├── Data/
│   ├── Middleware/
│   ├── Secure/
│   ├── appsettings.json
│   └── Program.cs
├── docker-compose.yml
└── README.md
```

## 🛠️ Локальный запуск

### 1. Клонируй репозиторий

```bash
git clone https://github.com/S0rgi/Fitness_App-Auth.API.git
cd Fitness_App-Auth.API
```


### 2. Создай .env фалй с секретами
```env
#database
ConnectionStrings__AuthDb="Host=localhost;Port=5432;Database=fitness_auth;Username=postgres;Password=yourpassword"

# JWT
Jwt__Issuer=http://localhost
Jwt__Audience=http://localhost
Jwt__SecretKey=your_jwt_secret_key

#api
ApiKey__HeaderName=your_api_head
ApiKey=your_api_key
```


### 3. Запусти сервисы через Docker
```bash 
docker-compose up --build
```

### 4. Проверь доступность
Открой браузер и перейди на:
```
http://localhost:8080/swagger
```

## ☁️ Деплой на Fly.io

### 1. Загрузи секреты:
```shell
fly secrets set `
  ConnectionStrings__AuthDb="Host=yourhost;Port=5432;Database=fitness_auth;Username=postgres;Password=yourpassword" `
  Jwt__Issuer="https://yourdomain.fly.dev" `
  Jwt__Audience="https://yourdomain.fly.dev" `
  Jwt__SecretKey="your_production_jwt_secret" `
  ApiKey__HeaderName="your_api_head"`
  ApiKey="your_production_api_key"
```

### 2. Деплой
```bash
fly deploy
```


## 🔐 Аутентификация
### JWT
Передавай Authorization: Bearer <token> в заголовках для защищённых эндпоинтов.

### API Key
Для эндпоинтов с API-ключом используй заголовок:

```bash
-H 'X-API-KEY: your-api-key' 
```
## 🧪 Swagger UI
Swagger UI уже настроен на работу с JWT и API-ключами.

Открыть можно по адресу:

```bash
http://localhost:8080/swagger
```
## ⚠️ Безопасность
- .env исключён из Git (см. .gitignore)
- Секреты не хранятся в appsettings.json в репозитории
- История коммитов была очищена для удаления следов старых ключей