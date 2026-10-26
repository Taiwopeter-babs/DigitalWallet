using System.Text.Json;
using Confluent.Kafka;
using DigitalWallet.Data;
using DigitalWallet.Services.KafkaService.KafkaProducerService;
using Microsoft.EntityFrameworkCore;


namespace DigitalWallet.Services.EventService;

public class EventService : BackgroundService
{
    private const int BatchSize = 50;
    private const int PollingInterval = 60;

    private readonly IServiceScopeFactory _serviceScopeFactory;


    public EventService(
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // Poll interval (check outbox every 10 seconds)
            await Task.Delay(TimeSpan.FromSeconds(PollingInterval), stoppingToken);
        }
    }

    private async Task ProcessOutboxBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var kafkaProducerService = scope.ServiceProvider.GetRequiredService<IKafkaProducerService>();


        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => !m.Processed)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (!pendingMessages.Any()) return;

        foreach (var outbox in pendingMessages)
            try
            {
                var message = new Message<string, string>
                {
                    Key = outbox.Key,
                    Value = outbox.Payload.ToString() ?? ""
                };

                Console.WriteLine(message.Value);


                var deliveryResult =
                    await kafkaProducerService.PublishKafkaEvent(outbox.Topic, outbox.Key, message, cancellationToken);

                if (deliveryResult is null) continue;

                outbox.Processed = true;
                outbox.ProcessedAt = DateTimeOffset.UtcNow;
                outbox.ErrorMessage = null;

                Console.WriteLine(
                    "Outbox message {0} successfully published to Kafka topic {1} at offset {2}", outbox.Id,
                    outbox.Topic, deliveryResult.Offset);
            }
            catch (Exception ex)
            {
                // Kafka is likely down. save the error and keep Processed = false to retry later
                outbox.ErrorMessage = ex.Message;
            }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}