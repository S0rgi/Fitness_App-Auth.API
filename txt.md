Отлично! Чтобы создать `WorkoutService`, который будет **идентифицировать пользователей через gRPC к AuthService**, нужно собрать ключевую информацию из `AuthService`, необходимую для:

* 🔐 **аутентификации**
* 👤 **идентификации пользователей**
* 🔗 **gRPC взаимодействия**
* 📢 **возможной публикации событий в Kafka (если потребуется позже)**

---

## ✅ Что нужно знать про AuthService

### 1. **User Identification Model**

* `User` содержит:

  * `Guid Id`
  * `string Email`
  * `string Username`
  * `string PasswordHash`
  * `DateTime RegistrationDate`

➡️ **GUID (Id)** — основной идентификатор пользователя, по которому `WorkoutService` будет сохранять и извлекать данные.

---

### 2. **gRPC-методы, которые понадобятся в WorkoutService**

Ты должен **добавить и использовать** следующий gRPC-метод в AuthService:

#### 📌 Прототип gRPC метода (AuthService):

```proto
// auth.proto
service AuthGrpc {
  rpc ValidateToken (TokenRequest) returns (TokenResponse);
}

message TokenRequest {
  string accessToken = 1;
}

message TokenResponse {
  string userId = 1;
  string email = 2;
  string username = 3;
}
```

#### Реализация `AuthGrpcService`:

```csharp
public override async Task<TokenResponse> ValidateToken(TokenRequest request, ServerCallContext context)
{
    var token = request.AccessToken;

    var handler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]);

    try
    {
        var claims = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        }, out _);

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var user = await _context.Users.FindAsync(Guid.Parse(userId));

        return new TokenResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            Username = user.Username
        };
    }
    catch
    {
        throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
    }
}
```

---

### 3. **Что нужно добавить в appsettings (Auth)**

```json
"Jwt": {
  "SecretKey": "super_secret_key_123456",
  "Issuer": "fitness-app",
  "Audience": "fitness-app-users",
  "AccessTokenLifetimeMinutes": 15,
  "RefreshTokenLifetimeDays": 7
}
```

---

### 4. **Что отдаёт Auth при логине/регистрации**

* `accessToken` (JWT)
* `refreshToken`
* ➕ **Добавь `userId` в ответ**, чтобы `WorkoutService` в будущем не обязательно вызывал gRPC при логине (если ему сразу нужно привязать данные к юзеру).

---

### 5. **Настроенный gRPC-сервер**

* AuthService должен иметь в `Program.cs`:

```csharp
app.MapGrpcService<AuthGrpcService>();
```

* И настройки в `appsettings.json`:

```json
"Kestrel": {
  "Endpoints": {
    "Grpc": {
      "Url": "https://localhost:5001",
      "Protocols": "Http2"
    }
  }
}
```

---

### 6. **Какие nuget-пакеты установлены в AuthService**

Для gRPC:

```bash
dotnet add package Grpc.AspNetCore
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
```

---

## ✅ Что потребуется в WorkoutService

1. Подключение к Auth gRPC:

   * `Grpc.Net.Client`
   * `auth.proto` сгенерированный код
2. Проверка токена через `ValidateToken(...)`
3. Сохранение тренировок с `UserId`
4. (По желанию) логирование/отказ при ошибке валидации

---

Хочешь — помогу настроить gRPC клиент и начать с минимального `WorkoutController`. Готов?
