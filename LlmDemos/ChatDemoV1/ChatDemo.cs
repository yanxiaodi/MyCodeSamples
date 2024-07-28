using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Shared;
using System.Text;

namespace ChatDemoV1;
internal class ChatDemo
{
    public static async Task Run(OpenAiOptions openAiOptions)
    {
        Console.WriteLine("Hello, World! You can ask questions or press q to exit.");
        var openAiClient = new AzureOpenAIClient(new Uri(openAiOptions.Endpoint), new AzureKeyCredential(openAiOptions.Key));
        var chatClient = openAiClient.GetChatClient(openAiOptions.Model);
        var systemMessage = """
                            You are a friendly assistant who helps users with their tasks.
                            You will complete required steps and request approval before taking any consequential actions.
                            If the user doesn't provide enough information for you to complete a task, you will keep asking questions until you have enough information to complete the task.
                            """;
        var chatCompletionsOptions = new ChatCompletionOptions()
        {
            Temperature = (float)0.7,
            MaxTokens = 1024,
            //TopP = 0, // Only adjust Temperature or TopP, not both
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };
        var messages = new List<ChatMessage>()
            {
                new SystemChatMessage(systemMessage)
            };

        while (true)
        {
            Console.Write("USER: ");
            var userMessageString = Console.ReadLine();
            if (string.Equals(userMessageString?.Trim(), "q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye!");
                break;
            }
            Console.Write("ASSISTANT: ");
            var userMessage = new UserChatMessage(userMessageString);
            messages.Add(userMessage);

            var assistantMessageStringBuilder = new StringBuilder();
            var response = chatClient.CompleteChatStreamingAsync(messages, chatCompletionsOptions);

            await foreach (var chatUpdate in response)
            {
                foreach (var chatMessageContentPart in chatUpdate.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(chatUpdate.ContentUpdate.ToString()))
                    {
                        assistantMessageStringBuilder.Append(chatMessageContentPart.Text);
                        Console.Write(chatMessageContentPart.Text);
                    }
                }
            }

            Console.WriteLine();
            var assistantMessage = new AssistantChatMessage(assistantMessageStringBuilder.ToString());
            messages.Add(assistantMessage);
        }
    }
}
