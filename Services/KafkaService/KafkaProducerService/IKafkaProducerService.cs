using Confluent.Kafka;

namespace DigitalWallet.Services.KafkaService.KafkaProducerService;

public interface IKafkaProducerService
{
    Task<DeliveryResult<string, string>?> PublishKafkaEvent<T>(string topic, string messageKey, T messageValue,
        CancellationToken token);

    Task SendToConsumerDlqAsync(
       ConsumeResult<string, string> failedResult,
       Exception ex,
       CancellationToken cancellationToken);
}