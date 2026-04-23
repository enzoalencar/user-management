using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Authorization;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Domain.Users;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class AuthEndpointsIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private const string JwtIssuer = "https://localhost:7015";
    private const string JwtAudience = "https://localhost:7015";
    private const string JwtSecretKey = "integration-tests-secret-key-32chars-min";
    private const string RefreshTokenCookieName = "um_refresh_token";

    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public AuthEndpointsIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    var testSettings = new Dictionary<string, string?>
                    {
                        ["MongoDb:ConnectionString"] = _fixture.ConnectionString,
                        ["MongoDb:DatabaseName"] = _fixture.DatabaseName,
                        ["MongoDb:UsersCollectionName"] = _fixture.UsersCollectionName,
                        ["MongoDb:RefreshTokensCollectionName"] = _fixture.RefreshTokensCollectionName,
                        ["Jwt:Issuer"] = JwtIssuer,
                        ["Jwt:Audience"] = JwtAudience,
                        ["Jwt:SecretKey"] = JwtSecretKey,
                        ["Jwt:AccessTokenExpirationInMinutes"] = "3",
                        ["Jwt:RefreshTokenExpirationInMinutes"] = "60",
                        ["Jwt:RefreshTokenCookieName"] = RefreshTokenCookieName
                    };

                    configBuilder.AddInMemoryCollection(testSettings);
                });
            });

        _httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task PostAuthLogin_WhenCredentialsAreValid_ShouldReturnOkWithAccessTokenAndRefreshCookie()
    {
        var user = await SeedActiveUserAsync("valid");

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "MyStrongPassword123!"
        };

        var response = await _httpClient.PostAsJsonAsync("/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));

        var refreshToken = ExtractRefreshTokenFromSetCookie(response);
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));
    }

    [Fact]
    public async Task PostAuthLogin_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        var user = await SeedActiveUserAsync("invalid");

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "WrongPassword!"
        };

        var response = await _httpClient.PostAsJsonAsync("/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRefresh_WhenRefreshTokenCookieIsMissing_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.PostAsync("/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRefresh_WhenRefreshTokenCookieIsMalformed_ShouldReturnUnauthorized()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshTokenCookieName}=malformed-refresh-token");

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredAccessToken_ShouldBeRejected_AndValidRefreshTokenShouldIssueNewPair()
    {
        var user = await SeedActiveUserAsync("expiration");

        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = "MyStrongPassword123!"
        };

        var loginResponse = await _httpClient.PostAsJsonAsync("/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var originalRefreshToken = ExtractRefreshTokenFromSetCookie(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(originalRefreshToken));

        var expiredAccessToken = GenerateExpiredAccessToken(user);
        using var protectedEndpointRequest = new HttpRequestMessage(HttpMethod.Get, "/users");
        protectedEndpointRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", expiredAccessToken);

        var protectedEndpointResponse = await _httpClient.SendAsync(protectedEndpointRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, protectedEndpointResponse.StatusCode);

        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        refreshRequest.Headers.Add("Cookie", $"{RefreshTokenCookieName}={originalRefreshToken}");

        var refreshResponse = await _httpClient.SendAsync(refreshRequest);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshedBody = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(refreshedBody);
        Assert.False(string.IsNullOrWhiteSpace(refreshedBody.Token));

        var rotatedRefreshToken = ExtractRefreshTokenFromSetCookie(refreshResponse);
        Assert.False(string.IsNullOrWhiteSpace(rotatedRefreshToken));
        Assert.NotEqual(originalRefreshToken, rotatedRefreshToken);
    }

    private async Task<User> SeedActiveUserAsync(string suffix)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Integration",
            LastName = "User",
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = $"auth.{suffix}.{Guid.NewGuid():N}@test.com",
            Password = BCrypt.Net.BCrypt.HashPassword("MyStrongPassword123!"),
            DocumentNumber = $"DOC-{Guid.NewGuid():N}",
            PhoneNumber = ["+5511999999999"],
            IsActive = true
        };

        await _fixture.Repository.CreateAsync(user);
        return user;
    }

    private static string ExtractRefreshTokenFromSetCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            throw new InvalidOperationException("Refresh token cookie was not returned.");

        var refreshCookieHeader = setCookieHeaders.FirstOrDefault(header =>
            header.StartsWith($"{RefreshTokenCookieName}=", StringComparison.Ordinal));

        if (string.IsNullOrWhiteSpace(refreshCookieHeader))
            throw new InvalidOperationException("Refresh token cookie header not found.");

        var cookieValuePart = refreshCookieHeader.Split(';', 2, StringSplitOptions.TrimEntries)[0];
        var cookieTokens = cookieValuePart.Split('=', 2, StringSplitOptions.TrimEntries);
        if (cookieTokens.Length != 2 || string.IsNullOrWhiteSpace(cookieTokens[1]))
            throw new InvalidOperationException("Refresh token cookie value is invalid.");

        return cookieTokens[1];
    }

    private static string GenerateExpiredAccessToken(User user)
    {
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(AuthClaimTypes.IsActive, "true"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            notBefore: now.AddMinutes(-10),
            expires: now.AddMinutes(-1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
