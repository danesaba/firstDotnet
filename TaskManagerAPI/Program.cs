using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TaskManagerAPI.Data;
using TaskManagerAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Configure and register the Redis connection
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new InvalidOperationException("Redis connection string is not configured.");
}
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

// 2. Configure and register the Entity Framework DbContext for PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Configure the HTTP request pipeline ---

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Tell the application to use our task endpoints
app.MapTaskEndpoints();

// --- Automatically create/update the database on startup ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();
