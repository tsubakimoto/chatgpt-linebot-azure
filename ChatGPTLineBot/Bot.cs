namespace ChatGPTLineBot;

public class Bot
{
    private static readonly string _messagingApiUrl = "https://api.line.me/v2/bot/message/reply";
    private static readonly string _baseSystemMessage = "You are a helpful assistant.";
    private static readonly ChatRequestMessage _systemRoleChatMessage = new ChatRequestSystemMessage(_baseSystemMessage);
    private static readonly IDictionary<string, IList<ChatRequestMessage>> _conversations = new Dictionary<string, IList<ChatRequestMessage>>();

    private readonly ILogger<Bot> logger;
    private readonly IConfiguration configuration;
    private readonly HttpClient httpClient;

    public Bot(ILogger<Bot> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.configuration = configuration;

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

        if (!_conversations.ContainsKey(json.destination))
        {
            _conversations.Add(json.destination, new List<ChatRequestMessage> { _systemRoleChatMessage });
            logger.LogDebug("Init conversation: {0}", json.destination);
        }

        var channelSecret = configuration["LineChannelSecret"];
        var accessToken = configuration["LineAccessToken"];

        if (IsSignature(xLineSignature, requestBody, channelSecret)
            && json.EventType == "message")
        {
            var result = await RunCompletionAsync(json.destination, json.Message);
            logger.LogDebug("ChatGPT: {0}", result);

            await ReplyAsync(json.ReplyToken, result, accessToken);
            return new OkResult();
        }
        return new BadRequestResult();
    }

    private async Task<string> RunCompletionAsync(string userId, string prompt)
    {
        _conversations[userId].Add(new ChatRequestUserMessage(prompt));

        var resourceName = configuration["AzureOpenAIResourceName"];
        var apiKey = configuration["AzureOpenAIApiKey"];
        var deploymentName = configuration["AzureOpenAIDeploymentName"];

        OpenAIClient client = new(
                new Uri($"https://{resourceName}.openai.azure.com/"),
                new AzureKeyCredential(apiKey));

        ChatCompletionsOptions options = new()
        {
            DeploymentName = deploymentName,
            Temperature = (float)0.7,
            MaxTokens = 500,
            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };
        foreach (var c in _conversations[userId])
        {
            options.Messages.Add(c);
        }

        var response = await client.GetChatCompletionsAsync(options);
        var choice = response?.Value?.Choices?.FirstOrDefault();
        var content = choice?.Message?.Content ?? string.Empty;
        _conversations[userId].Add(new ChatRequestAssistantMessage(content));
        logger.LogDebug("Finish reason: {0}", choice?.FinishReason);
        logger.LogDebug("Conversation count: {0}", _conversations[userId].Count);

        return content;
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
