namespace ChatGPTLineBot;

public class LineMessageReceiveJson
{
    [JsonPropertyName("destination")]
    public string? Destination { get; set; }

    [JsonPropertyName("events")]
    public List<Event>? Events { get; set; }

    private Event? FirstEvent => Events?.FirstOrDefault();

    public Message? FirstMessage => FirstEvent?.Message;

    public string? MessageText => FirstEvent?.Message?.Text;

    public string? ReplyToken => FirstEvent?.ReplyToken;

    public string? EventType => FirstEvent?.Type;

    public bool IsTextType => FirstMessage.Type == "text";

    public bool IsImageType => FirstMessage.Type == "image" && FirstMessage.ContentProvider.Type == "line";
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
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("contentProvider")]
    public ContentProvider ContentProvider { get; set; }
}

public class ContentProvider
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class Source
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
}
