using Xunit;
using UserManagement.Domain.Users;

namespace UserManagement.IntegrationTests;

public class UserRepositoryIntegrationTests : IClassFixture<MongoFixture>
{
    private readonly MongoFixture _fixture;
    
    public UserRepositoryIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateAsync_ThenFindOneAsync_ShouldPersistAndReturnUser()
    {
        var userId = Guid.NewGuid();
        
        var user = new User
        {
            Id = userId,
            FirstName = "Enzo",
            LastName = "Alencar",
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = "enzo@test.com",
            Password = "hashed-password",
            DocumentNumber = "12345678900",
            PhoneNumber = ["+5511999999999"],
            IsActive = true
        };

        await _fixture.Repository.CreateAsync(user);
        var found = await _fixture.Repository.FindOneAsync(userId);
        
        Assert.NotNull(found);
        Assert.Equal(userId, found.Id);
        Assert.Equal("Enzo", found.FirstName);
        Assert.Equal("enzo@test.com", found.Email);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldReturnTrueAndPersistChanges()
    {
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "Enzo",
            LastName = "Alencar",
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = "enzo@test.com",
            Password = "hashed-password",
            DocumentNumber = "12345678900",
            PhoneNumber = ["+5511999999999"],
            IsActive = true
        };

        await _fixture.Repository.CreateAsync(user);

        var updatedUser = new User
        {
            Id = userId,
            FirstName = "Enzo Atualizado",
            LastName = "Alencar",
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = "enzo.atualizado@test.com",
            Password = "hashed-password-2",
            DocumentNumber = "12345678900",
            PhoneNumber = ["+5511988888888"],
            IsActive = true
        };

        var updated = await _fixture.Repository.UpdateAsync(updatedUser);
        var found = await _fixture.Repository.FindOneAsync(userId);

        Assert.True(updated);
        Assert.NotNull(found);
        Assert.Equal("Enzo Atualizado", found.FirstName);
        Assert.Equal("enzo.atualizado@test.com", found.Email);
        Assert.Single(found.PhoneNumber);
        Assert.Equal("+5511988888888", found.PhoneNumber[0]);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_ShouldReturnTrueAndRemoveUser()
    {
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "Enzo",
            LastName = "Alencar",
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = "enzo@test.com",
            Password = "hashed-password",
            DocumentNumber = "12345678900",
            PhoneNumber = ["+5511999999999"],
            IsActive = true
        };

        await _fixture.Repository.CreateAsync(user);

        var deleted = await _fixture.Repository.DeleteAsync(userId);
        var found = await _fixture.Repository.FindOneAsync(userId);

        Assert.True(deleted);
        Assert.Null(found);
    }
}
