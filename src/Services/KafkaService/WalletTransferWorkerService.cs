using System.Text.Json;
using Confluent.Kafka;
using DigitalWallet.Storage;
using DigitalWallet.Data;
using Microsoft.Extensions.Options;

namespace DigitalWallet.Services.KafkaService;

public class WalletTransferWorkerService : BackgroundService
{
    private readonly WalletTransferWorkerOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public WalletTransferWorkerService(IOptions<WalletTransferWorkerOptions> options,
        IServiceScopeFactory serviceScopeFactory)
    {
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            using (var consumer = new ConsumerBuilder<Null, string>(_options).Build())
            {
                consumer.Subscribe(_options.Topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);

                    using (var scope = _serviceScopeFactory.CreateAsyncScope())
                    {
                        var walletTransferEvent =
                            JsonSerializer.Deserialize<WalletTransferEventDto>(result.Message.Value);

                        var historyService = scope.ServiceProvider.GetRequiredService<IHistoryService>();

                        if (walletTransferEvent == null)
                            // Log null event
                            continue;

                        await historyService.SaveTransactionEvent(walletTransferEvent.ToCreateTransactionEventDto(),
                            stoppingToken);
                    }
                }
            }
        }, stoppingToken);
    }
}