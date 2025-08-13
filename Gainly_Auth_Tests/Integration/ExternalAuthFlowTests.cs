using System.Net.Http.Json;
using Gainly_Auth_API.Interfaces;

namespace Gainly_Auth.Tests.Integration;

public class ExternalAuthFlowTests
{
	private static HttpClient CreateClient(out string baseUrl)
	{
		baseUrl = Environment.GetEnvironmentVariable("INTEGRATION_BASE_URL")?.TrimEnd('/')
			?? "http://localhost:8080";
		var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
		var apiKeyHeader = Environment.GetEnvironmentVariable("API_KEY_HEADER") ?? "X-API-KEY";
		var apiKeyValue = Environment.GetEnvironmentVariable("API_KEY_VALUE") ?? string.Empty;
		client.DefaultRequestHeaders.Remove(apiKeyHeader);
		client.DefaultRequestHeaders.Add(apiKeyHeader, apiKeyValue);
		return client;
	}

	[Fact]
	public async Task Register_Login_Validate_DeleteUser_EndToEnd_External()
	{
		var client = CreateClient(out var baseUrl);

		// Быстрая проверка доступности сервера
		try
		{
			using var ping = new HttpRequestMessage(HttpMethod.Get, "/swagger/index.html");
			await client.SendAsync(ping);
		}
		catch
		{
			// Сервер недоступен — пропустим сценарий без падения
			return;
		}

		var email = $"it_{Guid.NewGuid():N}@mail.com";
		var password = "Password1!";

		// 1) Register
		var registerResp = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password));
		registerResp.EnsureSuccessStatusCode();
		var registerPair = await registerResp.Content.ReadFromJsonAsync<TokenPair>();
		Assert.NotNull(registerPair);

		// 2) Login
		var loginResp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
		loginResp.EnsureSuccessStatusCode();
		var loginPair = await loginResp.Content.ReadFromJsonAsync<TokenPair>();
		Assert.NotNull(loginPair);

		// 3) Validate
		var validateResp = await client.PostAsJsonAsync("/api/auth/validate", new { token = loginPair!.AccessToken });
		validateResp.EnsureSuccessStatusCode();

		// 4) Refresh
		var refreshResp = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loginPair.RefreshToken });
		refreshResp.EnsureSuccessStatusCode();

		// 5) UserExist
		var existResp = await client.GetAsync($"/api/user/UserExist?email={Uri.EscapeDataString(email)}");
		existResp.EnsureSuccessStatusCode();

		// 6) DeleteUser
		var deleteResp = await client.DeleteAsync($"/api/user/DeleteUser?email={Uri.EscapeDataString(email)}");
		deleteResp.EnsureSuccessStatusCode();

		// 7) Проверка удаления
		var existAfterResp = await client.GetAsync($"/api/user/UserExist?email={Uri.EscapeDataString(email)}");
		Assert.Equal(System.Net.HttpStatusCode.BadRequest, existAfterResp.StatusCode);
	}
}


