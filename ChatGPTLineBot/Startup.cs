[assembly: FunctionsStartup(typeof(ChatGPTLineBot.Startup))]
namespace ChatGPTLineBot;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient();
    }
}