namespace ChatGPTLineBot;

public class Echo
{
    private static readonly PromptExecutionSettings _settings = new AzureOpenAIPromptExecutionSettings
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    private readonly ILogger<Echo> _logger;
    private readonly IChatCompletionService _chatService;
    private readonly Kernel _kernel;
    private readonly ChatHistory _history = [];

    public Echo(
        ILogger<Echo> logger,
        IChatCompletionService chatService,
        Kernel kernel)
    {
        _logger = logger;
        _chatService = chatService;
        _kernel = kernel;
    }

    [Function("Echo")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
    {
        _logger.LogDebug("This is debug log on Echo.");

        string query = req.Query["q"];
        if (string.IsNullOrEmpty(query))
        {
            return new BadRequestObjectResult("no query.");
        }
        _history.AddUserMessage(query);

        var assistant = await _chatService.GetChatMessageContentAsync(_history, _settings, _kernel);
        _history.Add(assistant);

        return new OkObjectResult(assistant.ToString());
    }
}