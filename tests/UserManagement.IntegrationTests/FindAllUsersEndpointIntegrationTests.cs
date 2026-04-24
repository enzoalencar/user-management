using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.Api.Features.Users.FindAllUsers;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class FindAllUsersEndpointIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public FindAllUsersEndpointIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;
        _factory = UsersEndpointTestHost.CreateFactory(_fixture);
        _httpClient = UsersEndpointTestHost.CreateHttpsClient(_factory);
    }

    [Fact]
    public async Task GetUsers_WhenAccessTokenIsValid_ShouldReturnOkWithUsers()
    {
        var seeded = await UsersEndpointTestHost.SeedUserAsync(_fixture, "find-all");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, _fixture, "find-all-auth");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var wwwAuthenticate = string.Join(" | ", response.Headers.WwwAuthenticate.Select(header => header.ToString()));
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Expected 200 OK but got {(int)response.StatusCode} {response.StatusCode}. " +
                $"WWW-Authenticate: '{wwwAuthenticate}'. Response body: '{responseBody}'.");
        }

        var body = await response.Content.ReadFromJsonAsync<List<FindAllUsersResult>>();
        Assert.NotNull(body);
        Assert.NotEmpty(body);
        Assert.Contains(body, user => user.Id == seeded.Id && user.Email == seeded.Email);
    }

    [Fact]
    public async Task GetUsers_WhenAccessTokenIsMissing_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.GetAsync("/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
