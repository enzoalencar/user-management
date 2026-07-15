using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.Api.Features.Users.UpdateUserStatus;
using UserManagement.Domain.Users;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class UpdateUserStatusEndpointIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public UpdateUserStatusEndpointIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;
        _factory = UsersEndpointTestHost.CreateFactory(_fixture);
        _httpClient = UsersEndpointTestHost.CreateHttpsClient(_factory);
    }

    [Fact]
    public async Task PutStatus_WhenAdministratorIsAuthenticated_ShouldChangeStatus()
    {
        var target = await UsersEndpointTestHost.SeedUserAsync(_fixture, "status-target");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(
            _httpClient,
            _fixture,
            "status-admin",
            UserRole.Administrator);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{target.Id}/status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new UpdateUserStatusRequest { IsActive = false });

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UpdateUserStatusResult>();
        Assert.NotNull(result);
        Assert.Equal(target.Id, result.Id);
        Assert.False(result.IsActive);

        var persisted = await _fixture.Repository.FindOneAsync(target.Id);
        Assert.NotNull(persisted);
        Assert.False(persisted.IsActive);
    }

    [Fact]
    public async Task PutStatus_WhenCommonUserIsAuthenticated_ShouldReturnForbidden()
    {
        var target = await UsersEndpointTestHost.SeedUserAsync(_fixture, "status-forbidden-target");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(
            _httpClient,
            _fixture,
            "status-common");

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{target.Id}/status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new UpdateUserStatusRequest { IsActive = false });

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var persisted = await _fixture.Repository.FindOneAsync(target.Id);
        Assert.NotNull(persisted);
        Assert.True(persisted.IsActive);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
