using UserManagement.Infrastructure.DependencyInjection;
using UserManagement.Api.Features.Users.CreateUser;
using UserManagement.Api.Features.Users.DeleteUser;
using UserManagement.Api.Features.Users.FindAllUsers;
using UserManagement.Api.Features.Users.FindOneUser;
using UserManagement.Api.Features.Users.UpdateUser;
using UserManagement.Domain.Users;
using UserManagement.Infrastructure.Persistence.Mongo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserRepository, UserRepository>(); //TODO: Remove

var app = builder.Build();

app.UseMiddleware<UserManagement.Api.Utils.Middleware.GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapCreateUser();
app.MapDeleteUser();
app.MapUpdateUser();
app.MapFindOneUser();
app.MapFindAllUser();

app.Run();
