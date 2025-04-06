using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Shared;
using System.Text;

namespace Demo3;
internal class ChatDemo
{
    public static async Task Run(OpenAiOptions openAiOptions)
    {
        Console.WriteLine("This demo shows how to use Semantic Memory to retrieve information from files. You can ask questions or press q to exit.");

        // Create a kernel builder
        var builder = Kernel.CreateBuilder();

        // Add the Azure OpenAI chat completions service
        builder.AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key);

        // Add logs
        // builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        // Build the kernel
        var kernel = builder.Build();

        // Retrieve the chat completions service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Create the chat history
        var chatHistory = new ChatHistory("""
                                           You are a friendly assistant who helps users with their tasks.
                                           You will complete required steps and request approval before taking any consequential actions.
                                           If the user doesn't provide enough information for you to complete a task, you will keep asking questions until you have enough information to complete the task.
                                           """);

        // Enable the AutoInvokeKernelFunctions tool call behavior
        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            MaxTokens = 10000,
            Temperature = 0.7f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        // Configure Kernel Memory
        var memory = new KernelMemoryBuilder()
            .WithAzureOpenAITextGeneration(new AzureOpenAIConfig()
            {
                APIKey = openAiOptions.Key,
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                Endpoint = openAiOptions.Endpoint,
                Deployment = openAiOptions.Model,
            })
            .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig()
            {
                APIKey = openAiOptions.Key,
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                Endpoint = openAiOptions.Endpoint,
                Deployment = "text-embedding-3-large"
            })
            .WithSimpleVectorDb()
            .Build<MemoryServerless>();

        // Import a document into memory
        await memory.ImportDocumentAsync(Path.Combine(Directory.GetCurrentDirectory(), "Resources/DocSample.pdf"), "docSample");
        var memoryPlugin = new MemoryPlugin(memory, waitForIngestionToComplete: true);
        kernel.ImportPluginFromObject(memoryPlugin, "memory");

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

            chatHistory.AddUserMessage(userMessageString!);

            // Get the chat completions
            var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory: chatHistory,
                executionSettings: settings,
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
            chatHistory.AddAssistantMessage(assistantMessageStringBuilder.ToString());
        }
    }
}
