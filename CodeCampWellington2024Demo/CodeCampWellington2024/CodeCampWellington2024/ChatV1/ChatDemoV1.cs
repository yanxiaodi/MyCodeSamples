using Azure;
using Azure.AI.OpenAI;
using System.Text;

namespace CodeCampWellington2024.ChatV1;

/// <summary>
/// This is a simple chat demo that demonstrates how to use the OpenAI Chat API to create a chatbot.
/// </summary>
internal class ChatDemoV1
{
    public static async Task Run(OpenAiOptions openAiOptions)
    {
        Console.WriteLine("Hello, World! Ask a question. Press q to exit.");
        var openAiClient = new OpenAIClient(new Uri(openAiOptions.Endpoint), new AzureKeyCredential(openAiOptions.Key));
        var systemMessage = """
                            You are a friendly assistant who likes to follow the rules. You will complete required steps
                            and request approval before taking any consequential actions. If the user doesn't provide
                            enough information for you to complete a task, you will keep asking questions until you have
                            enough information to complete the task.
                            """;
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = openAiOptions.Model,
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
            Console.Write("USER: ");
            var userMessageString = Console.ReadLine();
            if (userMessageString?.Trim().ToLower() == "q")
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            var userMessage = new ChatRequestUserMessage(userMessageString);
            chatCompletionsOptions.Messages.Add(userMessage);

            var assistantMessageStringBuilder = new StringBuilder();
            var response = await openAiClient.GetChatCompletionsStreamingAsync(chatCompletionsOptions);

            await foreach (var chatUpdate in response)
            {
                if (chatUpdate.Role.HasValue && chatUpdate.ContentUpdate != null)
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
