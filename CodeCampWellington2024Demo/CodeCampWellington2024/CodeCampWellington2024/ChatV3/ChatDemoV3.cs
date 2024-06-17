#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.Text;

namespace CodeCampWellington2024.ChatV3;

/// <summary>
/// This chat demo uses Semantic Kernel Bing Search plugin to search the web.
/// </summary>
internal class ChatDemoV3
{
    public static async Task Run(OpenAiOptions openAiOptions, BingSearchOptions bingSearchOptions)
    {
        Console.WriteLine("Hello, World! You can ask questions or press q to exit.");

        ILoggerFactory myLoggerFactory = NullLoggerFactory.Instance;

        // Create the kernel
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(myLoggerFactory);
        builder.Services.AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key);

        // Add a native plugin to call native code
        builder.Plugins.AddFromType<TimePlugin>();

        var kernel = builder.Build();

        // Add a plugin to use Bing search
        var bingConnector = new BingConnector(bingSearchOptions.Key);
        kernel.ImportPluginFromObject(new WebSearchEnginePlugin(bingConnector), "bing");

        // Retrieve the chat completion service from the kernel
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Create the chat history
        var chatMessages = new ChatHistory("""
                                           You are a friendly assistant who likes to follow the rules. You will complete required steps
                                           and request approval before taking any consequential actions. If the user doesn't provide
                                           enough information for you to complete a task, you will keep asking questions until you have
                                           enough information to complete the task.
                                           """);

        // Start the conversation
        while (true)
        {
            // Get user input
            Console.Write("USER: ");
            var userMessageString = Console.ReadLine();
            if (userMessageString?.Trim().ToLower() == "q")
            {
                Console.WriteLine("Goodbye!");
                break;
            }
            chatMessages.AddUserMessage(userMessageString!);

            // Get the chat completions
            OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
            var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatMessages,
                executionSettings: openAiPromptExecutionSettings,
                kernel: kernel);

            // Stream the results
            var assistantMessageStringBuilder = new StringBuilder();
            await foreach (var content in response)
            {
                if (content is { Role: not null, Content: not null })
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
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
