using System.Text.Json;
using Confluent.Kafka;
using DigitalWallet.Storage;
using DigitalWallet.Data;
using Microsoft.Extensions.Options;

namespace DigitalWallet.Services.KafkaService;

public class WalletFundWorkerService : BackgroundService
{
    private readonly WalletFundWorkerOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public WalletFundWorkerService(IOptions<WalletFundWorkerOptions> options,
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
                        var historyService = scope.ServiceProvider.GetRequiredService<IHistoryService>();

                        var walletFundEvent =
                            JsonSerializer.Deserialize<WalletFundEventDto>(result.Message.Value);

                        if (walletFundEvent == null)
                            // Log null event
                            continue;

                        await historyService.SaveTransactionEvent(walletFundEvent.ToCreateTransactionEventDto(),
                            stoppingToken);
                    }
                }
            }
        }, stoppingToken);
    }
}