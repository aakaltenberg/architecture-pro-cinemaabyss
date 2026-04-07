using Microsoft.AspNetCore.Mvc;
using EventsService.Models;
using EventsService.Services;

namespace EventsService.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly KafkaProducerService _producer;
    private readonly ILogger<EventsController> _logger;

    public EventsController(KafkaProducerService producer, ILogger<EventsController> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    [HttpPost("movie")]
    public async Task<IActionResult> CreateMovieEvent([FromBody] MovieEvent movieEvent)
    {
        if (movieEvent == null) return BadRequest();
        await _producer.ProduceMovieEventAsync(movieEvent);
        var response = new { status = "success", message = "Movie event produced" };
        return StatusCode(201, response);
    }

    [HttpPost("user")]
    public async Task<IActionResult> CreateUserEvent([FromBody] UserEvent userEvent)
    {
        if (userEvent == null) return BadRequest();
        await _producer.ProduceUserEventAsync(userEvent);
        var response = new { status = "success", message = "User event produced" };
        return StatusCode(201, response);
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePaymentEvent([FromBody] PaymentEvent paymentEvent)
    {
        if (paymentEvent == null) return BadRequest();
        await _producer.ProducePaymentEventAsync(paymentEvent);
        var response = new { status = "success", message = "Payment event produced" };
        return StatusCode(201, response);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = true });
}