using Microsoft.Extensions.Options;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

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
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Identity.Interfaces.IRoleTabPermissionsRepository, SmartSolarMicrogrid.API.Components.Identity.Repositories.RoleTabPermissionsRepository>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Identity.Interfaces.IRoleTabPermissionsService, SmartSolarMicrogrid.API.Components.Identity.Services.RoleTabPermissionsService>();

// Add Microgrid Component DI
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Microgrid.Interfaces.IMicrogridStationRepository, SmartSolarMicrogrid.API.Components.Microgrid.Repositories.MicrogridStationRepository>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Microgrid.Interfaces.IEnergySlotRepository, SmartSolarMicrogrid.API.Components.Microgrid.Repositories.EnergySlotRepository>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Microgrid.Interfaces.IMicrogridService, SmartSolarMicrogrid.API.Components.Microgrid.Services.MicrogridService>();


builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Reservations.Interfaces.IReservationRepository, SmartSolarMicrogrid.API.Components.Reservations.Repositories.ReservationRepository>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Reservations.Interfaces.IReservationService, SmartSolarMicrogrid.API.Components.Reservations.Services.ReservationService>();
builder.Services.AddScoped<SmartSolarMicrogrid.API.Components.Microgrid.Interfaces.IReservationChecker, SmartSolarMicrogrid.API.Components.Reservations.Services.ReservationService>();


builder.Services.AddOpenApi();

var app = builder.Build();


try
{
    var settings = app.Services.GetRequiredService<MongoDbSettings>();
    var client = new MongoDB.Driver.MongoClient(settings.ConnectionString);
    var database = client.GetDatabase(settings.DatabaseName);
    
    // Ping to verify connection
    database.RunCommand((MongoDB.Driver.Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n=======================================================");
    Console.WriteLine(" SUCCESS: Successfully connected to MongoDB Database! ");
    Console.WriteLine($" Database Name: {settings.DatabaseName}");
    Console.WriteLine("=======================================================\n");
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n=======================================================");
    Console.WriteLine(" ERROR: Failed to connect to MongoDB cluster! ");
    Console.WriteLine($" Details: {ex.Message}");
    Console.WriteLine("=======================================================\n");
    Console.ResetColor();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

