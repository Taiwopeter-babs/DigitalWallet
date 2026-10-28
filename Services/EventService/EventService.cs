using DigitalWallet.Data;
using DigitalWallet.Services.KafkaService.KafkaProducerService;
using Microsoft.EntityFrameworkCore;


namespace DigitalWallet.Services.EventService;

public record OutboxUpdateFailureData
{
    public required OutboxMessage OutboxMessage { get; init; }
    public required string ExceptionMessage { get; init; }
}

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

            // Poll interval (check outbox every 60 seconds)
            await Task.Delay(TimeSpan.FromSeconds(PollingInterval), stoppingToken);
        }
    }

    private async Task ProcessOutboxBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var kafkaProducerService = scope.ServiceProvider.GetRequiredService<IKafkaProducerService>();


        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => m.Processed == false || m.InboxMessage == null || (m.InboxMessage != null && m.InboxMessage.Received == false))
            .Include(om => om.InboxMessage)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        List<EventPublishPayload> eventsToPublish = [];

        Console.WriteLine("Count of pending: {0}", pendingMessages.Count);

        if (pendingMessages.Count == 0) return;

        var currentDatetime = DateTimeOffset.UtcNow;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {

            foreach (var outbox in pendingMessages)
            {
                try
                {
                    Console.WriteLine("Outbox Id and Inbox Received: {0} == {1}", outbox.Id, outbox.InboxMessage?.Received);

                    if (outbox.InboxMessage != null && outbox.InboxMessage.Received is true)
                    {
                        Console.WriteLine("Inbox is not null and received: {0}", outbox.Id);
                        continue;
                    }


                    if (outbox.InboxMessage == null)
                    {
                        InboxMessage newInboxMessage = new()
                        {
                            Topic = outbox.Topic,
                            Key = outbox.Key,
                            CorrelationId = outbox.Payload.CorrelationId,
                            OutboxId = outbox.Id
                        };

                        dbContext.InboxMessages.Add(newInboxMessage);
                    }

                    outbox.Processed = true;
                    outbox.ProcessedAt = currentDatetime;

                    Console.WriteLine("Oubox success addition {0}", outbox.Id);

                    eventsToPublish.Add(new EventPublishPayload
                    {
                        Topic = outbox.Topic,
                        Key = outbox.Key,
                        Payload = outbox.Payload
                    });


                }
                catch (Exception ex)
                {
                    outbox.ErrorMessage = ex.Message;

                    Console.WriteLine("Oubox error addition {0}", outbox.Id);

                    continue;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }

        if (eventsToPublish.Count > 0)
        {
            await kafkaProducerService.PublishKafkaEvents(eventsToPublish, cancellationToken);

        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}