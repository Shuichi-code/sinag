using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Sinag.Api.Data;
using Sinag.Api.Endpoints;
using Sinag.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Database — PostgreSQL in production (Railway), SQLite for local dev
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var configConnString = builder.Configuration.GetConnectionString("DefaultConnection");
bool usePostgres = !string.IsNullOrEmpty(databaseUrl) || !string.IsNullOrEmpty(configConnString);

if (usePostgres)
{
    string connectionString;
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                           $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
    else
    {
        connectionString = configConnString!;
    }
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Local dev: SQLite file in project directory
    var sqlitePath = Path.Combine(AppContext.BaseDirectory, "sinag-dev.db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("estimate", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromHours(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Rate limit exceeded. Try again later.", cancellationToken);
    };
});

// Services
builder.Services.AddScoped<CalculationService>();
builder.Services.AddScoped<BomService>();
builder.Services.AddScoped<FinancialService>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<RateService>();
builder.Services.AddHttpClient<IrradianceService>();
builder.Services.AddScoped<IrradianceService>();

var app = builder.Build();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    // Cleanup expired saved estimates
    var now = DateTime.UtcNow;
    var expired = await db.SavedEstimates.Where(e => e.ExpiresAt < now).ToListAsync();
    if (expired.Count > 0)
    {
        db.SavedEstimates.RemoveRange(expired);
        await db.SaveChangesAsync();
    }
}

// Middleware
app.UseRateLimiter();

// Endpoints
app.MapHealthEndpoints();
app.MapEstimateEndpoints();
app.MapPricingEndpoints();
app.MapRatesEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
