using DigitalWallet.Data;

namespace DigitalWallet.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Register database context
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string? databaseConnectionString;

        if (environment == "Development")
        {
            databaseConnectionString = configuration.GetConnectionString("PostgresqlDatabase");
        }
        else
        /// Get database string from secrets vault
        {
            databaseConnectionString = configuration.GetConnectionString("PostgresqlDatabaseStaging");
        }

        services.AddDbContext<DatabaseContext>(options =>
            options.UseNpgsql(
                databaseConnectionString,
                op => op.MigrationsHistoryTable("DigitalWalletMigrationsHistory")));
    }

    /// <summary>
	/// Configure cors for application
	/// </summary>
	/// <param name="services"></param>
	public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });
    }

   
    /// <summary>
    /// Configure a scoped repositories and services manager dependency injection
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureManagers(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IServiceManager, ServiceManager>();
    }
    
}