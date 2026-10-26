# Receipt Analyzer

Upload a photo of a receipt and get back a clean summary of what you bought, why it's useful, and what to watch out for. The web page sends the image to a C# Azure Function, which asks GPT-4o mini on Azure OpenAI to read and analyze it.

## What it does
For each receipt, the analysis comes back in three sections:

1. **Receipt summary**: store name, date and time, total spent, and item count
2. **Purchased items and benefits**: items grouped by category, with the practical, nutritional or household benefit of each
3. **Cons and watch-outs**: ultra-processed or high-sugar items, impulse buys, and perishables to use soon

## Tech stack
- **Frontend:** a single HTML page with Tailwind CSS (CDN) and vanilla JavaScript
- **Backend:** C# / .NET 8 Azure Function (isolated worker, Functions v4)
- **AI:** Azure OpenAI, `gpt-4o-mini` deployment, through the `Azure.AI.OpenAI` SDK
- **Hosting:** Azure Functions (Zip Deploy), with Application Insights for logging

## How it works
1. The user picks a receipt image on the page and clicks **Analyze Receipt**.
2. The page POSTs the raw image bytes (`application/octet-stream`) to `/api/analyze`.
3. `AnalyzeImage.cs` reads the bytes and sends them to GPT-4o mini with a system prompt that defines the three-section format.
4. The model's text reply is returned to the page and shown in the results panel.

The Azure OpenAI client is created once in `Startup.cs` and passed into the function through dependency injection.

## History
This project was first built with Google Gemini as the AI model. I later rebuilt it as a C# Azure Function that calls GPT-4o mini on Azure OpenAI, which is the version in this repository.

## Project structure
```
index.html            Web page: upload, loading state, results
backend/
  AnalyzeImage.cs     HTTP-triggered function (POST /api/analyze)
  Startup.cs          Host setup and Azure OpenAI client registration
  Program.csproj      .NET 8 project and NuGet packages
  host.json           Functions host and logging settings
```

## Running it locally
**Requirements:** .NET 8 SDK, Azure Functions Core Tools v4, and an Azure OpenAI resource with a `gpt-4o-mini` deployment.

1. Create `backend/local.settings.json` (not committed):
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "OPENAI_ENDPOINT": "https://<your-resource>.openai.azure.com/",
       "OPENAI_KEY": "<your-key>"
     }
   }
   ```
2. Start the function:
   ```
   cd backend
   func start
   ```
3. In `index.html`, point the `fetch` URL at `http://localhost:7071/api/analyze`, then open the page in a browser.

You can also test the endpoint directly:
```
curl -X POST --data-binary @receipt.png http://localhost:7071/api/analyze
```

## Deployment
The function is published to Azure Functions with Zip Deploy. Set `OPENAI_ENDPOINT` and `OPENAI_KEY` as application settings on the Function App. The page calls the deployed endpoint, so it can be hosted anywhere static (or opened locally).

## Possible improvements
- Return structured JSON and render the sections as cards instead of plain text
- Send the image's real content type instead of always `image/png`
- Basic upload limits and rate limiting on the public endpoint
- Spending history across receipts
