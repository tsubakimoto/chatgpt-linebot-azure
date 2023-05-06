# ChatGPT LINE Bot on Azure

Azure Functions と LINE Messaging API を使って ChatGPT とやり取りできるサービスを作ってみた  
https://zenn.dev/takunology/articles/linebotandazure-gpt

## Run on local

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "LineAccessToken": "",
    "LineChannelSecret": "",
    "AzureOpenAIResourceName": "",
    "AzureOpenAIDeploymentName": "",
    "AzureOpenAIApiKey": ""
  }
}
```

## Run on GitHub Codespaces

1. Start Debugging
2. Change port 7071 visibility to public
3. Open port 7071 in browser
4. Copy URL and Paste in LINE Developer Console
