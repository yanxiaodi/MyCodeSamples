using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDemo.Config;
using System.Text;

namespace SemanticKernelDemo.ChatbotV2;
internal class ChatbotDemoV2
{
    public static async Task Run()
    {
        ILoggerFactory myLoggerFactory = NullLoggerFactory.Instance;

        // Create the kernel
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(myLoggerFactory);
        var (_, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();
        builder.Services.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);
        Kernel kernel = builder.Build();

        // Retrieve the chat completion service from the kernel
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Create the chat history
        ChatHistory chatMessages = new ChatHistory("""
                                           You are a friendly assistant who likes to follow the rules. You will complete required steps
                                           and request approval before taking any consequential actions. If the user doesn't provide
                                           enough information for you to complete a task, you will keep asking questions until you have
                                           enough information to complete the task.
                                           """);

        // Start the conversation
        while (true)
        {
            // Get user input
            Console.Write("User > ");
            var userMessageString = Console.ReadLine();
            if (userMessageString?.Trim().ToLower() == "q")
            {
                Console.WriteLine("Goodbye!");
                break;
            }
            chatMessages.AddUserMessage(userMessageString!);

            // Get the chat completions
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatMessages,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            // Stream the results
            var assistantMessageStringBuilder = new StringBuilder();
            await foreach (var content in result)
            {
                if (content.Role.HasValue)
                {
                    Console.Write($"{content.Role.Value.ToString().ToUpperInvariant()}: ");
                }
                if (!string.IsNullOrEmpty(content.Content))
                {
                    assistantMessageStringBuilder.Append(content.Content);
                    Console.Write(content.Content);
                }
            }
            Console.WriteLine();

            // Add the message from the agent to the chat history
            chatMessages.AddAssistantMessage(assistantMessageStringBuilder.ToString());
        }
    }
}
