namespace ChatGPTLineBot;

public class Bot
{
    private static readonly string _messagingApiUrl = "https://api.line.me/v2/bot/message/reply";
    private static readonly string _baseSystemMessage = "You are a helpful assistant.";
    private static readonly Dictionary<string, ChatHistory> _histories = new();
    private static readonly AzureOpenAIPromptExecutionSettings _settings = new()
    {
        Temperature = (float)0.7,
        MaxTokens = 500,
        FrequencyPenalty = 0,
        PresencePenalty = 0,
    };

    private readonly ILogger<Bot> logger;
    private readonly IConfiguration configuration;
    private readonly IChatCompletionService chatService;
    private readonly HttpClient httpClient;

    public Bot(
        ILogger<Bot> logger,
        IConfiguration configuration,
        IChatCompletionService chatService,
        IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.chatService = chatService;

        httpClient = httpClientFactory.CreateClient();
    }

    [Function("Bot")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        req.Headers.TryGetValue("X-Line-Signature", out var xLineSignature);

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        logger.LogDebug("Request body: {0}", requestBody);

        var json = JsonSerializer.Deserialize<LineMessageReceiveJson>(requestBody);
        logger.LogDebug("Message: {0}", json.Message);

        if (!_histories.ContainsKey(json.destination))
        {
            var history = new ChatHistory();
            history.AddSystemMessage(_baseSystemMessage);
            _histories.Add(json.destination, history);
            logger.LogDebug("Init conversation: {0}", json.destination);
        }

        var channelSecret = configuration["LineChannelSecret"];
        var accessToken = configuration["LineAccessToken"];

        if (IsSignature(xLineSignature, requestBody, channelSecret)
            && json.EventType == "message")
        {
            var history = _histories[json.destination];
            var result = await RunCompletionAsync(history, json.destination, json.Message);

            await ReplyAsync(json.ReplyToken, result, accessToken);
            return new OkResult();
        }
        return new BadRequestResult();
    }

    private async Task<string> RunCompletionAsync(ChatHistory history, string userId, string prompt)
    {
        history.AddUserMessage(prompt);
        logger.LogDebug($"{history.Last().Role} >>> {history.Last().Content}");

        var assistant = await chatService.GetChatMessageContentAsync(history, _settings);
        history.Add(assistant);
        logger.LogDebug($"{history.Last().Role} >>> {history.Last().Content}");

        return assistant.ToString();
    }

    private async Task ReplyAsync(string replyToken, string message, string accessToken)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.PostAsJsonAsync(_messagingApiUrl, new LineTextReplyJson()
        {
            replyToken = replyToken,
            messages = new List<Message>()
            {
                new Message
                {
                    type = "text",
                    text = message
                }
            }
        });
        response.EnsureSuccessStatusCode();
    }

    private static bool IsSignature(string signature, string text, string key)
    {
        var textBytes = Encoding.UTF8.GetBytes(text);
        var keyBytes = Encoding.UTF8.GetBytes(key);

        using (HMACSHA256 hmac = new(keyBytes))
        {
            var hash = hmac.ComputeHash(textBytes, 0, textBytes.Length);
            var hash64 = Convert.ToBase64String(hash);

            return signature == hash64;
        }
    }
}
