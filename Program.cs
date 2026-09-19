using Microsoft.Extensions.Options;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure MongoDB Settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<MongoDbSettings>(sp =>
    sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

// Add Identity Component DI
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Identity.Interfaces.IUserRepository, SmartSolarMicrogrid.API.Components.Identity.Repositories.UserRepository>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Identity.Interfaces.IProsumerRepository, SmartSolarMicrogrid.API.Components.Identity.Repositories.ProsumerRepository>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Identity.Interfaces.IAuthService, SmartSolarMicrogrid.API.Components.Identity.Services.AuthService>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Identity.Interfaces.IUserService, SmartSolarMicrogrid.API.Components.Identity.Services.UserService>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Identity.Interfaces.IProsumerService, SmartSolarMicrogrid.API.Components.Identity.Services.ProsumerService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
