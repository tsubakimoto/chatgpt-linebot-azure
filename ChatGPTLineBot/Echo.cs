namespace ChatGPTLineBot;

public class Echo
{
    private readonly ILogger<Echo> _logger;

    public Echo(ILogger<Echo> logger)
    {
        _logger = logger;
    }

    [Function("Echo")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
    {
        _logger.LogDebug("This is debug log on Echo.");
        return new OkObjectResult($"Now: {DateTime.UtcNow}");
    }
}