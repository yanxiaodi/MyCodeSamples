#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Shared;
using System.Text;

namespace ChatDemoV4;
internal class ChatDemo
{
    public static async Task Run(OpenAiOptions openAiOptions, BingSearchOptions bingSearchOptions)
    {
        Console.WriteLine("Hello, World! You can ask questions or press q to exit.");

        // Create a kernel builder
        var builder = Kernel.CreateBuilder();

        // Add the Azure OpenAI chat completions service
        builder.AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key);

        // Add logs
        // builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        // Build the kernel
        var kernel = builder.Build();

        // Add a plugin to use Bing search
        var bingConnector = new BingConnector(bingSearchOptions.Key);
        kernel.ImportPluginFromObject(new WebSearchEnginePlugin(bingConnector), "bing");

        // Retrieve the chat completions service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Enable the AutoInvokeKernelFunctions tool call behavior
        OpenAIPromptExecutionSettings executionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Create the chat history
        var chatMessages = new ChatHistory("""
                                           You are a friendly assistant who helps users with their tasks.
                                           You will complete required steps and request approval before taking any consequential actions.
                                           If the user doesn't provide enough information for you to complete a task, you will keep asking questions until you have enough information to complete the task.
                                           """);

        while (true)
        {
            // Get user input
            Console.Write("USER: ");
            var userMessageString = Console.ReadLine();
            if (string.Equals(userMessageString?.Trim(), "q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            chatMessages.AddUserMessage(userMessageString!);

            // Get the chat completions
            var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatMessages,
                executionSettings: executionSettings,
                kernel: kernel);

            // Stream the results
            var assistantMessageStringBuilder = new StringBuilder();
            Console.Write("ASSISTANT: ");
            await foreach (var content in response)
            {
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
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
