var builder = FunctionsApplication.CreateBuilder(args);
var c = builder.Configuration;

builder.ConfigureFunctionsWebApplication();

//builder.Services.AddHttpClient();
builder.Services.AddHttpClient("LineMessagingApi", client =>
{
    client.BaseAddress = new Uri("https://api.line.me/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("LineContentApi", client =>
{
    client.BaseAddress = new Uri("https://api-data.line.me/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddAzureOpenAIChatCompletion(
    c["AzureOpenAIDeploymentName"]!,
    new AzureOpenAIClient(
        new Uri(c["AzureOpenAIEndpoint"]!),
        new DefaultAzureCredential()))
    .AddDistributedMemoryCache();

#pragma warning disable SKEXP0050
// Semantic Kernel Plugins
builder.Services.AddSingleton(sp =>
{
    return KernelPluginFactory.CreateFromObject(new WebSearchEnginePlugin(new BingConnector(c["BingApiKey"])));
});

// Add Semantic Kernel
builder.Services.AddKernel();
#pragma warning restore SKEXP0050

/*
#pragma warning disable SKEXP0050
builder.Services.AddSingleton(sp =>
{
    return new WebSearchEnginePlugin(new BingConnector(c["BingApiKey"]));
});

builder.Services.AddTransient(sp =>
{
    KernelPluginCollection plugins = [];
    plugins.AddFromObject(sp.GetRequiredService<WebSearchEnginePlugin>());
    return new Kernel(sp, plugins);
});
#pragma warning restore SKEXP0050
*/

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
