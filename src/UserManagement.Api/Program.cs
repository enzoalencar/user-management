using UserManagement.Infrastructure.DependencyInjection;
using UserManagement.Api.Utils.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();

var app = builder.Build();

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
