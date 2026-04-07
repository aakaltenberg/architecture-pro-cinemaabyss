using System.Text;

public class ProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProxyMiddleware> _logger;
    private readonly IConfiguration _configuration;

    // URL сервисов из переменных окружения
    private readonly string _monolithUrl;
    private readonly string _moviesServiceUrl;
    private readonly string _eventsServiceUrl;

    // Feature-флаги
    private readonly bool _gradualMigration;
    private readonly int _moviesMigrationPercent;

    private readonly Random _random = new();

    public ProxyMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory,
        ILogger<ProxyMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;

        _monolithUrl = _configuration["MONOLITH_URL"] ?? "http://monolith:8080";
        _moviesServiceUrl = _configuration["MOVIES_SERVICE_URL"] ?? "http://movies-service:8081";
        _eventsServiceUrl = _configuration["EVENTS_SERVICE_URL"] ?? "http://events-service:8082";

        // Парсинг feature-флагов
        var gradualMigrationStr = _configuration["GRADUAL_MIGRATION"]?.ToLower();
        _gradualMigration = gradualMigrationStr == "true" || gradualMigrationStr == "1";
        _moviesMigrationPercent = int.TryParse(_configuration["MOVIES_MIGRATION_PERCENT"], out var percent)
            ? Math.Clamp(percent, 0, 100)
            : 0;

        _logger.LogInformation("Proxy configuration: Monolith={Monolith}, MoviesService={MoviesService}, " +
                               "EventsService={EventsService}, GradualMigration={Gradual}, MigrationPercent={Percent}",
            _monolithUrl, _moviesServiceUrl, _eventsServiceUrl, _gradualMigration, _moviesMigrationPercent);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        // Health check обрабатывается отдельным эндпоинтом
        if (path == "/health")
        {
            await _next(context);
            return;
        }

        string? targetUrl = null;

        // Маршрутизация на основе пути
        if (path.StartsWith("/api/movies", StringComparison.OrdinalIgnoreCase))
        {
            targetUrl = ShouldUseMoviesService()
                ? _moviesServiceUrl
                : _monolithUrl;
            _logger.LogInformation("Routing {Path} -> {Target}", path, targetUrl);
        }
        else if (path.StartsWith("/api/events", StringComparison.OrdinalIgnoreCase))
        {
            targetUrl = _eventsServiceUrl;
            _logger.LogInformation("Routing {Path} -> EventsService", path);
        }
        else if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            targetUrl = _monolithUrl;
            _logger.LogInformation("Routing {Path} -> Monolith", path);
        }
        else
        {
            // Неизвестный путь – передаём дальше (404)
            await _next(context);
            return;
        }

        // Проксирование запроса
        await ProxyRequest(context, targetUrl);
    }

    private bool ShouldUseMoviesService()
    {
        if (!_gradualMigration) return false;
        if (_moviesMigrationPercent == 0) return false;
        if (_moviesMigrationPercent == 100) return true;

        // Случайное распределение процента трафика
        return _random.Next(100) < _moviesMigrationPercent;
    }

    private async Task ProxyRequest(HttpContext context, string baseUrl)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(60); // Увеличенный таймаут
        var targetUri = BuildTargetUri(baseUrl, context.Request);

        using var requestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(context.Request.Method),
            RequestUri = targetUri
        };

        // Копирование заголовков запроса
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                continue;
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        // Копирование тела запроса
        if (context.Request.Body != null && context.Request.ContentLength != 0)
        {
            var bodyContent = await ReadRequestBodyAsync(context.Request);
            requestMessage.Content = new StringContent(bodyContent, Encoding.UTF8, context.Request.ContentType ?? "application/json");
        }

        try
        {
            using var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, context.RequestAborted);

            context.Response.StatusCode = (int)responseMessage.StatusCode;

            // Копирование заголовков ответа
            foreach (var header in responseMessage.Headers)
            {
                if (header.Key.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase))
                    continue;
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            foreach (var header in responseMessage.Content.Headers)
            {
                if (header.Key.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase))
                    continue;
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // Полностью буферизуем ответ и отправляем одним куском
            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            await context.Response.WriteAsync(responseBody, Encoding.UTF8, context.RequestAborted);
            await context.Response.Body.FlushAsync(context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying request to {TargetUri}", targetUri);
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsync("Bad Gateway");
        }
    }

    private static Uri BuildTargetUri(string baseUrl, HttpRequest request)
    {
        var path = request.Path.Value ?? "";
        var query = request.QueryString.Value ?? "";
        var fullPath = path + query;
        return new Uri(new Uri(baseUrl), fullPath);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }
}
