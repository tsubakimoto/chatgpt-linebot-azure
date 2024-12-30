namespace ChatGPTLineBot;

public class Bot
{
    private static readonly string _messagingApiUrl = "https://api.line.me/v2/bot/message/reply";
    private static readonly string _contentApiUrl = "https://api-data.line.me/v2/bot/message/{0}/content";
    private static readonly string _baseSystemMessage = "You are a helpful assistant.";
    private static readonly TextContent _imageExplainContent = new("Ç±ÇÃâÊëúÇê‡ñæÇµÇƒÇ≠ÇæÇ≥Ç¢ÅB");
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
        logger.LogDebug("Message: {0}", json.MessageText);

        if (!_histories.ContainsKey(json.Destination))
        {
            var history = new ChatHistory();
            history.AddSystemMessage(_baseSystemMessage);
            _histories.Add(json.Destination, history);
            logger.LogDebug("Init conversation: {0}", json.Destination);
        }

        var channelSecret = configuration["LineChannelSecret"];
        var accessToken = configuration["LineAccessToken"];

        if (IsSignature(xLineSignature, requestBody, channelSecret)
            && json.EventType == "message")
        {
            var history = _histories[json.Destination];

            string result = string.Empty;
            if (json.IsTextType)
            {
                result = await RunCompletionAsync(history, json.Destination, json.MessageText);
            }
            else if (json.IsImageType)
            {
                result = await ExplainImageAsync(history, json.FirstMessage.Id, accessToken);
            }

            await ReplyAsync(json.ReplyToken, result, accessToken);
            return new OkResult();
        }
        return new BadRequestResult();
    }

    private async Task<string> RunCompletionAsync(ChatHistory history, string userId, string prompt)
    {
        history.AddUserMessage(prompt);
        Log(history);

        var assistant = await chatService.GetChatMessageContentAsync(history, _settings);
        history.Add(assistant);
        Log(history);

        return assistant.ToString();
    }

    private async Task<string> ExplainImageAsync(ChatHistory history, string messageId, string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        ImageContent imageContent;
        using (MemoryStream stream = new())
        {
            var response = await httpClient.GetStreamAsync(string.Format(_contentApiUrl, messageId));
            response.CopyTo(stream);
            imageContent = new ImageContent(stream.ToArray(), "image/jpeg");
        }

        history.AddUserMessage(new ChatMessageContentItemCollection { _imageExplainContent, imageContent });
        Log(history);

        var assistant = await chatService.GetChatMessageContentAsync(history, _settings);
        history.Add(assistant);
        Log(history);

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
                        Type = "text",
                        Text = message
                    }
                }
        });
        response.EnsureSuccessStatusCode();
    }

    private void Log(ChatHistory history) => logger.LogDebug($"{history.Last().Role} >>> {history.Last().Content}");

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
