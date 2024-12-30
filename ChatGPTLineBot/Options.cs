namespace ChatGPTLineBot;

public class LineOptions
{
    public static readonly string SectionName = "Line";

    public required string AccessToken { get; set; }

    public required string ChannelSecret { get; set; }
}

public class AzureOptions
{
    public static readonly string SectionName = "Azure";

    public required OpenAIOptions OpenAI { get; set; }

    public class OpenAIOptions
    {
        public static readonly string SectionName = "Azure:OpenAI";

        public required string ResourceName { get; set; }

        public required string DeploymentName { get; set; }

        public required string Endpoint { get; set; }
    }
}
