using UserManagement.Infrastructure.DependencyInjection;
using UserManagement.Api.Utils.Extensions;
using MongoDB.Driver;
using UserManagement.Domain.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();

var app = builder.Build();

// TODO: Move MongoDB index creation to the infrastructure layer.
await using (var scope = app.Services.CreateAsyncScope())
{
    var users = scope.ServiceProvider.GetRequiredService<IMongoCollection<User>>();
    var emailIndex = new CreateIndexModel<User>(
        Builders<User>.IndexKeys.Ascending(user => user.Email),
        new CreateIndexOptions { Name = "ux_users_email", Unique = true });

    await users.Indexes.CreateOneAsync(emailIndex);
}

app.UseMiddleware<UserManagement.Api.Utils.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapApiEndpoints();

app.Run();
