namespace ChatGPTLineBot;

public class LineTextReplyJson
{
    public string replyToken { get; set; }
    public List<Message> messages { get; set; }
    public bool notificationDisabled { get; set; }
}