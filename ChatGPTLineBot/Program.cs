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

builder.Services.AddKernel();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
