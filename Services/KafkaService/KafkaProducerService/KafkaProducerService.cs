using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using DigitalWallet.Data;
using DigitalWallet.Storage;
using Microsoft.Extensions.Options;

namespace DigitalWallet.Services.KafkaService.KafkaProducerService;

public record EventPublishPayload
{
    public required string Topic { get; init; }
    public required string Key { get; init; }
    public required OutboxMessagePayload Payload { get; init; }
}

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly WalletProducerOptions _options;

    public KafkaProducerService(IProducer<string, string> kafkaProducer,
        IOptions<WalletProducerOptions> options)
    {
        _kafkaProducer = kafkaProducer;
        _options = options.Value;
    }

    public async Task<DeliveryResult<string, string>?> PublishKafkaEvent<T>(string topic, string messageKey,
        T messageValue, CancellationToken token)
    {
        var message = new Message<string, string>
        {
            Key = messageKey,
            Value = JsonSerializer.Serialize(messageValue)
        };
        try
        {
            return await _kafkaProducer.ProduceAsync(topic, message, token);
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<DeliveryResult<string, string>?[]> PublishKafkaEvents(List<EventPublishPayload> eventsToPublish, CancellationToken token)
    {
        List<Task<DeliveryResult<string, string>>> payloadsToPublish = [];

        try
        {
            foreach (EventPublishPayload payload in eventsToPublish)
            {
                var message = new Message<string, string>
                {
                    Key = payload.Key,
                    Value = JsonSerializer.Serialize(payload.Payload)
                };

                payloadsToPublish.Add(_kafkaProducer.ProduceAsync(payload.Topic, message, token));
            }


            return await Task.WhenAll(payloadsToPublish);
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task SendToConsumerDlqAsync(
       ConsumeResult<string, string> failedResult,
       Exception ex,
       CancellationToken cancellationToken)
    {
        try
        {
            failedResult.Message.Headers ??= [];
            failedResult.Message.Headers.Add("x-original-topic", Encoding.UTF8.GetBytes(failedResult.Topic));
            failedResult.Message.Headers.Add("x-error-message", Encoding.UTF8.GetBytes(ex.Message));
            failedResult.Message.Headers.Add("x-failed-at", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o")));

            var dlqMessage = new Message<string, string>
            {
                Key = failedResult.Message.Key,
                Value = failedResult.Message.Value,
                Headers = failedResult.Message.Headers
            };

            // 2. Produce to the DLQ topic using your producer
            await _kafkaProducer.ProduceAsync(_options.WalletDLQTopic, dlqMessage, cancellationToken);

            Console.WriteLine(
                "Poison message from topic '{0}' routed to DLQ '{1}' successfully. Key: {2}",
                failedResult.Topic, _options.WalletDLQTopic, failedResult.Message.Key);
        }
        catch (Exception dlqEx)
        {
            // Absolute worst-case: Kafka is so broken that even the DLQ produce failed
            Console.WriteLine(

                "CRITICAL: Failed to route poison message to DLQ '{0}'. Message risk of loss!\nException: {1}",
                _options.WalletDLQTopic, dlqEx);
        }
    }
}