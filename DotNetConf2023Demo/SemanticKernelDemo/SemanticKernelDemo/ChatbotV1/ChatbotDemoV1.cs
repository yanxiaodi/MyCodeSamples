using Azure;
using Azure.AI.OpenAI;
using SemanticKernelDemo.Config;
using System.Text;

namespace SemanticKernelDemo.ChatbotV1;
internal class ChatbotDemoV1
{
    public static async Task Run()
    {
        Console.WriteLine("Hello, World! Ask a question. Press q to exit.");
        var (_, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();
        var openAiClient = new OpenAIClient(new Uri(azureEndpoint), new AzureKeyCredential(apiKey));
        var systemMessage = """
                            You are a friendly assistant who likes to follow the rules. You will complete required steps
                            and request approval before taking any consequential actions. If the user doesn't provide
                            enough information for you to complete a task, you will keep asking questions until you have
                            enough information to complete the task.
                            """;
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = model,
            Messages =
            {
                new ChatRequestSystemMessage(systemMessage)
            },
            Temperature = (float)0.7,
            MaxTokens = 800,
            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };

        while (true)
        {
            Console.Write("User > ");
            var userMessageString = Console.ReadLine();
            if (userMessageString?.Trim().ToLower() == "q")
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            var userMessage = new ChatRequestUserMessage(userMessageString);
            chatCompletionsOptions.Messages.Add(userMessage);

            //var response = await openAiClient.GetChatCompletionsStreamingAsync(chatCompletionsOptions);

            var assistantMessageStringBuilder = new StringBuilder();
            await foreach (StreamingChatCompletionsUpdate chatUpdate in openAiClient.GetChatCompletionsStreaming(chatCompletionsOptions))
            {
                if (chatUpdate.Role.HasValue)
                {
                    Console.Write($"{chatUpdate.Role.Value.ToString().ToUpperInvariant()}: ");
                }
                if (!string.IsNullOrEmpty(chatUpdate.ContentUpdate))
                {
                    assistantMessageStringBuilder.Append(chatUpdate.ContentUpdate);
                    Console.Write(chatUpdate.ContentUpdate);
                }
            }
            Console.WriteLine();
            var assistantMessage = new ChatRequestAssistantMessage(assistantMessageStringBuilder.ToString());
            chatCompletionsOptions.Messages.Add(assistantMessage);
        }
    }
}
