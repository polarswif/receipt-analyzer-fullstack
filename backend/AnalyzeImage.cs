using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using OpenAI.Chat; 
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Program
{
    public class AnalyzeImage
    {
        private readonly ILogger _logger;
        private readonly AzureOpenAIClient _openAIClient;

        //Dependency Injection automatically grabs the client
        public AnalyzeImage(ILoggerFactory loggerFactory, AzureOpenAIClient openAIClient)
        {
            _logger = loggerFactory.CreateLogger<AnalyzeImage>();
            _openAIClient = openAIClient; ;
        }

        // Analyzes image with [Function]
        [Function("AnalyzeImage")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "analyze")] HttpRequestData req)
        {

            _logger.LogInformation("Processing image with Azure OpenAI (GPT-4o mini)...");

            // 1. Read the image from the curl request
            using var memoryStream = new MemoryStream();
            await req.Body.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();
            BinaryData imageBinary = BinaryData.FromBytes(imageBytes);

            // 2. Connect to Gpt-4o deployment

            ChatClient chatClient = _openAIClient.GetChatClient("gpt-4o-mini");

            // 3. Build the conversational prompt + image attachment
            List<ChatMessage> messages =
            [
                new SystemChatMessage(@"You are an expert receipt analyzer and personal lifestyle assistant. 
                Your job is to take the raw text extracted from a scanned receipt and turn it into a clean, actionable summary.

                Please analyze the following receipt text and provide a response structured exactly into these three sections:

                ### 1. Receipt Summary
                * **Store Name:** [Identify store]
                * **Date & Time:** [Identify date/time]
                * **Total Amount Spent:** [Identify total cost]
                * **Item Count:** [Total number of items purchased]

                ### 2. Purchased Items & Benefits
                List the specific items bought (grouping them if it makes sense, like ""Produce"" or ""Electronics""). 
                For the items or categories, explain the practical, nutritional, or household benefits of having purchased them. 
                *(Example: ""Spinach & Avocados: Excellent source of healthy fats and micronutrients for meal prep."")*

                ### 3. Cons or Watch-Outs
                Provide a constructive critique of the contents purchased. Highlight things like:
                * High amounts of ultra-processed foods, sodium, or sugar.
                * Impulse buys or non-essential spending.
                * Items that have a short shelf-life and need to be used immediately to prevent waste."),


                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart
                    ("Analyze this image in detail and tell me what it is and its significance."),
                    ChatMessageContentPart.CreateImagePart(imageBinary, "image/png")
                )

            ];
            // 4. Send to the cloud and wait for the AI to write the response
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages);
            string textResponse = completion.Content[0].Text;

            // 5. Send results of image analysis to user
            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(textResponse);

            return response;
        }
    }
}
