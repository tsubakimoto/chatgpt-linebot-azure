using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatGPTLineBot;

public class Bot
{
    private static readonly string _messagingApiUrl = "https://api.line.me/v2/bot/message/reply";
    private static HttpClient _httpClient = new();

    private readonly ILogger<Bot> logger;

    public Bot(ILogger<Bot> logger)
    {
        this.logger = logger;
    }

    [FunctionName("Bot")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        req.Headers.TryGetValue("X-Line-Signature", out var xLineSignature);

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        logger.LogDebug("Request body: {0}", requestBody);

        var json = JsonSerializer.Deserialize<LineMessageReceiveJson>(requestBody);
        logger.LogDebug("Message: {0}", json.Message);

        var channelSecret = Environment.GetEnvironmentVariable("LineChannelSecret");
        var accessToken = Environment.GetEnvironmentVariable("LineAccessToken");

        if (IsSignature(xLineSignature, requestBody, channelSecret)
            && json.EventType == "message")
        {
            var result = await RunCompletionAsync(json.Message);
            logger.LogDebug("ChatGPT: {0}", result);

            await ReplyAsync(json.ReplyToken, result, accessToken);
            return new OkResult();
        }
        return new BadRequestResult();
    }

    private static async Task<string> RunCompletionAsync(string prompt)
    {
        var resourceName = Environment.GetEnvironmentVariable("AzureOpenAIResourceName");
        var apiKey = Environment.GetEnvironmentVariable("AzureOpenAIApiKey");
        var deploymentName = Environment.GetEnvironmentVariable("AzureOpenAIDeploymentName");

        OpenAIClient client = new(
                new Uri($"https://{resourceName}.openai.azure.com/"),
                new AzureKeyCredential(apiKey));

        var response = await client.GetChatCompletionsAsync(
            deploymentName,
            new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information."),
                    new ChatMessage(ChatRole.User, prompt),
                },
                Temperature = (float)0.7,
                MaxTokens = 800,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            });

        return response?.Value?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }

    private static async Task ReplyAsync(string replyToken, string message, string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsJsonAsync(_messagingApiUrl, new LineTextReplyJson()
        {
            replyToken = replyToken,
            messages = new List<Message>()
            {
                new Message(){
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