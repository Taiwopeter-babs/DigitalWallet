using System.Text.Json;
using Confluent.Kafka;
using DigitalWallet.Storage;
using DigitalWallet.Data;
using Microsoft.Extensions.Options;
using DigitalWallet.Services.KafkaService.KafkaProducerService;
using DigitalWallet.Services.KafkaService.WalletWorkerService;

namespace DigitalWallet.Services.KafkaService;

public class WalletCreationWorkerService : BackgroundService
{
    private readonly WalletCreationWorkerOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public WalletCreationWorkerService(IOptions<WalletCreationWorkerOptions> options,
        IServiceScopeFactory serviceScopeFactory)
    {
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {


            using var consumer = new ConsumerBuilder<string, string>(_options).SetKeyDeserializer(Deserializers.Utf8).Build();
            consumer.Subscribe(_options.Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                await using var scope = _serviceScopeFactory.CreateAsyncScope();

                try
                {
                    var historyService = scope.ServiceProvider.GetRequiredService<IHistoryService>();
                    var walletWorkerService = scope.ServiceProvider.GetRequiredService<IWalletWorkerService>();


                    var walletCreationEvent = JsonSerializer.Deserialize<WalletEventDto>(result.Message.Value);

                    if (walletCreationEvent == null)
                        continue;

                    var transactionEventResult = await walletWorkerService
                        .SaveTransactionAndInboxData(walletCreationEvent.ToCreateTransactionEventDto(), stoppingToken);

                    if (transactionEventResult)
                    {
                        consumer.Commit(result);
                    }

                }
                catch (Exception ex)
                {
                    var kafkaProducerService = scope.ServiceProvider.GetRequiredService<IKafkaProducerService>();

                    await kafkaProducerService.SendToConsumerDlqAsync(result, ex, stoppingToken);

                    consumer.Commit(result);
                }

            }
        }, stoppingToken);
    }
}