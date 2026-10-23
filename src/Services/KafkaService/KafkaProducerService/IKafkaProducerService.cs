namespace DigitalWallet.Services.KafkaService;

public interface IKafkaProducerService
{
    Task PublishKafkaEvent<T>(string topic, string messageKey, T messageValue, CancellationToken token);
}