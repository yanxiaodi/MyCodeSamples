#pragma warning disable SKEXP0054
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDemo.Config;
using System.Text;

namespace SemanticKernelDemo.EmailWriter;
internal class ChatbotDemoV4
{
    public static async Task Run()
    {
        ILoggerFactory myLoggerFactory = NullLoggerFactory.Instance;

        // Create the kernel
        var builder = Kernel.CreateBuilder();
        //builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
        builder.Services.AddSingleton(myLoggerFactory);
        var (_, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();
        builder.Services.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);

        // Add plugins
        builder.Plugins.AddFromType<AuthorEmailPlanner>();
        builder.Plugins.AddFromType<EmailPlugin>();
        Kernel kernel = builder.Build();

        //var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ChatbotV3", "plugins");

        //var bingKey = "c12873c6816a47f09c80731a83ad75c0";
        //var bingConnector = new BingConnector(bingKey);
        //kernel.ImportPluginFromObject(new WebSearchEnginePlugin(bingConnector), "bing");

        //kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "SummarizePlugin"));
        //kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginsDirectory, "WriterPlugin"));

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
            chatMessages.AddUserMessage(Console.ReadLine()!);

            // Get the chat completions
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                //MaxTokens = 8000
                //Temperature = 0.75,
                //TopP = 1,
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
                    Console.Write("Assistant > ");
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
