using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Store.API.Extensions;
using Store.API.HealthChecks;
using Store.API.Middleware;
using Store.Application;
using Store.Infrastructure;
using Store.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Store API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext());

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddAppHealthChecks();
    builder.Services.AddSwaggerDocumentation();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });

    var app = builder.Build();

    await InitializeDatabaseAsync(app);

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseHttpsRedirection();
    app.UseSwaggerDocumentation();
    app.MapAppHealthChecks();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    const int maxRetries = 15;
    const int delaySeconds = 5;

    string connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;

    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Ensuring database exists (attempt {Attempt}/{Max})...", attempt, maxRetries);
            await EnsureDatabaseExistsAsync(connectionString);
            logger.LogInformation("Database is ready.");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning("SQL Server not ready: {Message}. Retrying in {Delay}s...", ex.Message, delaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Creating schema (attempt {Attempt}/{Max})...", attempt, maxRetries);

            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            bool created = await db.Database.EnsureCreatedAsync();

            logger.LogInformation(created ? "Schema created." : "Schema already exists.");

            return;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex, "Schema creation failed. Retrying in {Delay}s...", delaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }

    AppDbContext dbFinal = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbFinal.Database.EnsureCreatedAsync();
}


static async Task EnsureDatabaseExistsAsync(string connectionString)
{
    SqlConnectionStringBuilder csb = new(connectionString);

    string databaseName = csb.InitialCatalog;

    csb.InitialCatalog = "master";

    await using var connection = new SqlConnection(csb.ConnectionString);

    await connection.OpenAsync();

    string sql = $"""
        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
            CREATE DATABASE [{databaseName}];
        """;

    await using var cmd = new SqlCommand(sql, connection);

    await cmd.ExecuteNonQueryAsync();
}

public partial class Program { }
