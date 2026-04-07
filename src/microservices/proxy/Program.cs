var builder = WebApplication.CreateBuilder(args);

// Настройка конфигурации из переменных окружения
builder.Configuration.AddEnvironmentVariables();

// Добавление HTTP клиента
builder.Services.AddHttpClient();

var app = builder.Build();

// Middleware для проксирования
app.UseMiddleware<ProxyMiddleware>();

// Эндпоинт для health check
app.MapGet("/health", () => Results.Ok(new { status = true }));

app.Run();