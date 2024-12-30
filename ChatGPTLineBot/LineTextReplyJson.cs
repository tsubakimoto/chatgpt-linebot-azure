namespace ChatGPTLineBot;

public class LineTextReplyJson
{
    [JsonPropertyName("replyToken")]
    public string ReplyToken { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();

    [JsonPropertyName("notificationDisabled")]
    public bool NotificationDisabled { get; set; }
}
