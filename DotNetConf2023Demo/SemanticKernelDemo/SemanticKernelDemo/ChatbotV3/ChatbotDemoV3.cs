#pragma warning disable SKEXP0054
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using SemanticKernelDemo.Config;
using System.Text;

namespace SemanticKernelDemo.ChatbotV3;
internal class ChatbotDemoV3
{
    public static async Task Run()
    {
        ILoggerFactory myLoggerFactory = NullLoggerFactory.Instance;

        // Create the kernel
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(myLoggerFactory);
        var (_, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();
        builder.Services.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);

        //builder.Plugins.AddFromType<EmailPlugin>();

        Kernel kernel = builder.Build();

        // Add plugins
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ChatbotV3", "plugins");

        var bingKey = "c12873c6816a47f09c80731a83ad75c0";
        var bingConnector = new BingConnector(bingKey);
        kernel.ImportPluginFromObject(new WebSearchEnginePlugin(bingConnector), "bing");

        kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "SummarizePlugin"));
        kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "WriterPlugin"));

        // Retrieve the chat completion service from the kernel
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Create the chat history
        ChatHistory chatMessages = new ChatHistory("""
                                           You are a friendly assistant who likes to follow the rules. You will complete required steps
                                           and request approval before taking any consequential actions.
                                           If you don't know what the user is asking, you can return "Unknown".
                                           If the user doesn't provide enough information for you to complete a task, you will keep asking questions until you have
                                           enough information to complete the task.
                                           """);

        // Start the conversation
        while (true)
        {
            // Get user input
            Console.Write("User > ");
            var userMessageString = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(userMessageString))
            {
                Console.Write("User > ");
                userMessageString = Console.ReadLine();
            }
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

            if (assistantMessageStringBuilder.ToString().ToLower().Contains("unknown"))
            {
                Console.WriteLine("Searching Bing...");
                var function = kernel.Plugins["bing"]["search"];
                var bingResult = await kernel.InvokeAsync(function, new() { ["query"] = userMessageString });

                // Summarize the result
                var summaryFunction = kernel.Plugins["SummarizePlugin"]["summarize"];
                var summaryResult = await kernel.InvokeAsync(summaryFunction, new() { ["text"] = bingResult });
                Console.WriteLine(summaryResult);
                assistantMessageStringBuilder.Append(summaryResult);
            }
            Console.WriteLine();

            // Add the message from the agent to the chat history
            chatMessages.AddAssistantMessage(assistantMessageStringBuilder.ToString());
        }
    }
}
