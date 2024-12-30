namespace ChatGPTLineBot;

public class LineMessageReceiveJson
{
    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<Event>? Events { get; set; }

    private Event? FirstEvent => Events?.FirstOrDefault();

    public Message? FirstMessage => FirstEvent?.Message;

    public string MessageText => FirstEvent?.Message?.Text ?? string.Empty;

    public string ReplyToken => FirstEvent?.ReplyToken ?? string.Empty;

    public string EventType => FirstEvent?.Type ?? string.Empty;

    public bool IsMessageEvent => EventType == "message";

    public bool IsTextType => FirstMessage?.IsTextType ?? false;

    public bool IsImageType => FirstMessage?.IsImageType ?? false;
}

public class Event
{
    [JsonPropertyName("replyToken")]
    public string? ReplyToken { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("timestamp")]
    public object? Timestamp { get; set; }

    [JsonPropertyName("source")]
    public Source? Source { get; set; }

    [JsonPropertyName("message")]
    public Message? Message { get; set; }
}

public class Message
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("contentProvider")]
    public ContentProvider? ContentProvider { get; set; }

    public bool IsTextType => Type == "text";

    public bool IsImageType => Type == "image" && ContentProvider?.Type == "line";
}

public class ContentProvider
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class Source
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
}
