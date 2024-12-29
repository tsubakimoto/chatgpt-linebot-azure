var builder = FunctionsApplication.CreateBuilder(args);
var c = builder.Configuration;

builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddAzureOpenAIChatCompletion(
    c["AzureOpenAIDeploymentName"]!,
    new AzureOpenAIClient(new Uri(c["AzureOpenAIEndpoint"]!), new ApiKeyCredential(c["AzureOpenAIApiKey"]!)))
    .AddDistributedMemoryCache();

builder.Services.AddKernel();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
