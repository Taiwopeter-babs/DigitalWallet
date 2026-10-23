using Confluent.Kafka;
using DigitalWallet.Services.WalletService;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using DigitalWallet.Data;
using DigitalWallet.Repository;
using DigitalWallet.Services;
using DigitalWallet.Services.HistoryService;
using DigitalWallet.Services.KafkaService;
using DigitalWallet.Storage;

namespace DigitalWallet.Extensions;

public static class WalletServiceExtensions
{
    extension(IServiceCollection services)
    {
        public void ConfigureDatabaseContext(IConfiguration configuration)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var databaseConnectionString = string.Empty;

            databaseConnectionString = configuration.GetConnectionString("PostgresqlDatabase");

            if (string.IsNullOrEmpty(databaseConnectionString))
                throw new Exception("Database connection string not configured");


            services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(
                    databaseConnectionString,
                    op => op.MigrationsHistoryTable("DigitalWalletMigrationsHistory")));
        }

        public void ConfigureCors()
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }


        public void ConfigureManagers()
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
        }

        public void ConfigureServices()
        {
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<IKafkaProducerService, KafkaProducerService>();
        }


        // <summary>
        /// Register validators
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureValidators()
        {
            // services.AddScoped<IValidator<CreateWalletDto>, CreateWalletValidator>();
            // services.AddScoped<IValidator<FundWalletDto>, WalletFundValidator>();
            // services.AddValidatorsFromAssemblyContaining<CreateWalletValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateWalletValidator>();
        }

        /// <summary>
        /// Register background services here
        /// </summary>
        public void ConfigureHostedServices()
        {
            services.AddHostedService<WalletTransferWorkerService>();
            services.AddHostedService<WalletFundWorkerService>();
        }

        public void ConfigureKafkaOptions(IConfiguration configuration)
        {
            services.Configure<WalletTransferWorkerOptions>(
                configuration.GetSection("Kafka:WalletTransferWorker"));

            services.Configure<WalletFundWorkerOptions>(
                configuration.GetSection("Kafka:WalletFundingWorker"));

            services.Configure<WalletProducerOptions>(
                configuration.GetSection("Kafka:WalletProducer"));

            services.AddSingleton<IProducer<string, string>>(sp =>
            {
                var config = configuration.GetSection("Kafka:WalletProducer").Get<ProducerConfig>();
                return new ProducerBuilder<string, string>(config).Build();
            });
        }
    }
}