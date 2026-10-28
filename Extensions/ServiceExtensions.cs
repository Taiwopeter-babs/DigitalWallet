using Confluent.Kafka;
using DigitalWallet.Services.WalletService;
using Microsoft.EntityFrameworkCore;
using DigitalWallet.Data;
using DigitalWallet.Repository;
using DigitalWallet.Services;
using DigitalWallet.Services.EventService;
using DigitalWallet.Services.HistoryService;
using DigitalWallet.Services.KafkaService;
using DigitalWallet.Services.KafkaService.KafkaProducerService;
using DigitalWallet.Storage;
using FluentValidation;
using DigitalWallet.Services.KafkaService.WalletWorkerService;

namespace DigitalWallet.Extensions;

public static class WalletServiceExtensions
{
    extension(IServiceCollection services)
    {
        public void ConfigureDatabaseContext(IConfiguration configuration)
        {
            var databaseConnectionString = configuration.GetConnectionString("PostgresqlDatabase");
            Console.WriteLine(databaseConnectionString);

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
            services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
            services.AddScoped<IInboxMessageRepository, InboxMessageRepository>();

        }

        public void ConfigureServices()
        {
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
            services.AddScoped<IWalletWorkerService, WalletWorkerService>();
        }


        // <summary>
        /// Register validators
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureValidators()
        {
            services.AddValidatorsFromAssemblyContaining<Program>();
        }

        /// <summary>
        /// Register background services
        /// </summary>
        public void ConfigureHostedServices()
        {
            services.AddHostedService<WalletTransferWorkerService>();
            services.AddHostedService<WalletFundWorkerService>();
            services.AddHostedService<WalletCreationWorkerService>();
            services.AddHostedService<EventService>();
        }

        public void ConfigureKafkaOptions(IConfiguration configuration)
        {
            services.Configure<WalletTransferWorkerOptions>(
                configuration.GetSection("Kafka:WalletTransferWorker"));

            services.Configure<WalletFundWorkerOptions>(
                configuration.GetSection("Kafka:WalletFundingWorker"));

            services.Configure<WalletCreationWorkerOptions>(
                configuration.GetSection("Kafka:WalletCreationWorker"));

            services.Configure<WalletProducerOptions>(
                configuration.GetSection("Kafka:WalletProducer"));

            services.AddSingleton(sp =>
            {
                var config = configuration.GetSection("Kafka:WalletProducer").Get<ProducerConfig>();
                return new ProducerBuilder<string, string>(config).Build();
            });
        }
    }
}