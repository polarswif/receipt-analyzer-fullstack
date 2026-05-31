using System;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Program
{
    class Startup
    {
        static void Main()
        {
            // 1. Initialize the host application builder
            var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices(services =>
            {



                // 2. Pull the new OpenAI credentials from local.settings.json
                string? endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
                string? key = Environment.GetEnvironmentVariable("OPENAI_KEY");

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("Missing Azure OpenAI configuration in local.settings.json!");
        }



                // 3. Inject the Azure OpenAI Client into the app
                

                services.AddSingleton(new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key)));
            })
            // 4. Build and run the app
            .Build();

            host.Run();


        }
    }
}