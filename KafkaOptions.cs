using Confluent.Kafka;

namespace DigitalWallet.Storage;

public class WalletTransferWorkerOptions : ConsumerConfig
{
    public string Topic { get; set; } = string.Empty;
}

public class WalletFundWorkerOptions : ConsumerConfig
{
    public string Topic { get; set; } = string.Empty;
}

public class WalletCreationWorkerOptions : ConsumerConfig
{
    public string Topic { get; set; } = string.Empty;
}

public class WalletProducerOptions : ProducerConfig
{
    public string WalletProducerKafkaKey { get; set; } = string.Empty;
    public string WalletTransferTopic { get; set; } = string.Empty;
    public string WalletFundingTopic { get; set; } = string.Empty;
    public string WalletCreationTopic { get; set; } = string.Empty;

    public string WalletDLQTopic { get; set; } = string.Empty;


}