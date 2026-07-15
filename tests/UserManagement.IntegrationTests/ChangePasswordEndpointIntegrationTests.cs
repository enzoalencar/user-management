using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Api.Features.Users.ChangePassword;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class ChangePasswordEndpointIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public ChangePasswordEndpointIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;
        _factory = UsersEndpointTestHost.CreateFactory(_fixture);
        _httpClient = UsersEndpointTestHost.CreateHttpsClient(_factory);
    }

    [Fact]
    public async Task PutPassword_WhenOwnerProvidesCurrentPassword_ShouldChangePassword()
    {
        var user = await UsersEndpointTestHost.SeedUserAsync(_fixture, "password-owner");
        var oldPasswordHash = user.Password;
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, user);
        var requestBody = new ChangePasswordRequest
        {
            CurrentPassword = "MyStrongPassword123!",
            NewPassword = "NewStrongPassword456!"
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{user.Id}/password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var persisted = await _fixture.Repository.FindOneAsync(user.Id);
        Assert.NotNull(persisted);
        Assert.NotEqual(oldPasswordHash, persisted.Password);
        Assert.True(BCrypt.Net.BCrypt.Verify(requestBody.NewPassword, persisted.Password));

        var loginResponse = await _httpClient.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = requestBody.NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task PutPassword_WhenCommonUserTargetsAnotherAccount_ShouldReturnForbidden()
    {
        var target = await UsersEndpointTestHost.SeedUserAsync(_fixture, "password-target");
        var originalHash = target.Password;
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(
            _httpClient,
            _fixture,
            "password-attacker");

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{target.Id}/password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ChangePasswordRequest
        {
            CurrentPassword = "MyStrongPassword123!",
            NewPassword = "CompromisedPassword456!"
        });

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var persisted = await _fixture.Repository.FindOneAsync(target.Id);
        Assert.NotNull(persisted);
        Assert.Equal(originalHash, persisted.Password);
    }

    [Fact]
    public async Task PutPassword_WhenCurrentPasswordIsInvalid_ShouldReturnUnauthorized()
    {
        var user = await UsersEndpointTestHost.SeedUserAsync(_fixture, "password-invalid");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, user);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{user.Id}/password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewStrongPassword456!"
        });

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
