using Confluent.Kafka;
using EventsService.Models;
using System.Text.Json;

namespace EventsService.Services;

public class KafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly string _topicMovie;
    private readonly string _topicUser;
    private readonly string _topicPayment;

    public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        var bootstrapServers = config["KAFKA_BROKERS"] ?? "kafka:9092";
        _topicMovie = config["KAFKA_TOPIC_MOVIE"] ?? "movie-events";
        _topicUser = config["KAFKA_TOPIC_USER"] ?? "user-events";
        _topicPayment = config["KAFKA_TOPIC_PAYMENT"] ?? "payment-events";

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "events-service-producer"
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        _logger.LogInformation("Kafka Producer initialized for brokers: {Brokers}", bootstrapServers);
    }

    public async Task ProduceMovieEventAsync(MovieEvent movieEvent)
    {
        var message = JsonSerializer.Serialize(movieEvent);
        await _producer.ProduceAsync(_topicMovie, new Message<string, string> { Key = movieEvent.MovieId.ToString(), Value = message });
        _logger.LogInformation("Produced movie event to {Topic}: {Event}", _topicMovie, message);
    }

    public async Task ProduceUserEventAsync(UserEvent userEvent)
    {
        var message = JsonSerializer.Serialize(userEvent);
        await _producer.ProduceAsync(_topicUser, new Message<string, string> { Key = userEvent.UserId.ToString(), Value = message });
        _logger.LogInformation("Produced user event to {Topic}: {Event}", _topicUser, message);
    }

    public async Task ProducePaymentEventAsync(PaymentEvent paymentEvent)
    {
        var message = JsonSerializer.Serialize(paymentEvent);
        await _producer.ProduceAsync(_topicPayment, new Message<string, string> { Key = paymentEvent.PaymentId.ToString(), Value = message });
        _logger.LogInformation("Produced payment event to {Topic}: {Event}", _topicPayment, message);
    }
}