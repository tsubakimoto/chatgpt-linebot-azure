namespace ChatGPTLineBot;

public class LineMessageReceiveJson
{
    public string destination { get; set; }
    public List<Event> events { get; set; }

    private Event FirstEvent => events?.FirstOrDefault();
    public string Message => FirstEvent?.message?.text;
    public string ReplyToken => FirstEvent?.replyToken;
    public string EventType => FirstEvent?.type;
}

public class Event
{
    public string replyToken { get; set; }
    public string type { get; set; }
    public object timestamp { get; set; }
    public Source source { get; set; }
    public Message message { get; set; }
}

public class Message
{
    public string id { get; set; }
    public string type { get; set; }
    public string text { get; set; }
}

public class Source
{
    public string type { get; set; }
    public string userId { get; set; }
}