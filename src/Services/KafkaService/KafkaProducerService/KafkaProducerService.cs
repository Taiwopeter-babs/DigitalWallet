using System.Text.Json;
using Confluent.Kafka;
using DigitalWallet.Repository;
using DigitalWallet.Storage;
using Microsoft.Extensions.Options;

namespace DigitalWallet.Services.KafkaService;

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

    public async Task PublishKafkaEvent<T>(string topic, string messageKey, T messageValue, CancellationToken token)
    {
        var message = new Message<string, string>
        {
            Key = messageKey,
            Value = JsonSerializer.Serialize(messageValue)
        };
        try
        {
            await _kafkaProducer.ProduceAsync(_options.WalletTransferTopic, message, token);

            return;
        }
        catch (ProduceException<string, string> ex)
        {
            // Handle exceptions with logs and dead letter queue, and if kafka fails fully, use an outbox
            Console.WriteLine(ex.Message);
        }
    }
}