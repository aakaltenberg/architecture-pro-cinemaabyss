using Confluent.Kafka;

namespace EventsService.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _bootstrapServers;
    private readonly string[] _topics;
    private readonly int _retryDelaySeconds = 5;

    public KafkaConsumerService(IConfiguration config, ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;
        _bootstrapServers = config["KAFKA_BROKERS"] ?? "kafka:9092";
        _topics = new[]
        {
            config["KAFKA_TOPIC_MOVIE"] ?? "movie-events",
            config["KAFKA_TOPIC_USER"] ?? "user-events",
            config["KAFKA_TOPIC_PAYMENT"] ?? "payment-events"
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka Consumer Service starting...");
        _ = Task.Run(() => RunConsumerLoopAsync(stoppingToken), stoppingToken);
        await Task.CompletedTask;
    }

    private async Task RunConsumerLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunConsumerLoop(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer loop cancelled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer loop crashed. Restarting in {Delay}s", _retryDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds), stoppingToken);
            }
        }
        _logger.LogInformation("Kafka Consumer Service stopped.");
    }

    private async Task RunConsumerLoop(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "events-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            SessionTimeoutMs = 30000,
            MaxPollIntervalMs = 600000
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

        // Подписка с ретраями
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                consumer.Subscribe(_topics);
                _logger.LogInformation("Subscribed to topics: {Topics}", string.Join(", ", _topics));
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Subscribe failed, retrying in {Delay}s", _retryDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds), stoppingToken);
            }
        }

        // Цикл потребления
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                _logger.LogInformation("Consumed {Topic}[{Partition}]@{Offset}: {Value}",
                    consumeResult.Topic, consumeResult.Partition, consumeResult.Offset, consumeResult.Message.Value);
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogWarning(ex, "Topic not available, re-subscribing");
                break; // выходим из цикла, внешний цикл перезапустит подписку
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consume error, continuing");
                await Task.Delay(1000, stoppingToken);
            }
        }

        consumer.Close();
    }
}