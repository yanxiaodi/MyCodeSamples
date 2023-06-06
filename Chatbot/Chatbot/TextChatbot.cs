using Azure.AI.OpenAI;
using System.Text;

namespace Chatbot;
internal class TextChatbot
{
    public static async Task Chat(OpenAIClient openAiClient)
    {
        Console.WriteLine("Hello, World! Ask a question. Press q to exit.");
        var chatMessages = new Queue<ChatMessage>();
        var systemMessage = new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information.");
        while (true)
        {
            Console.WriteLine("User: ");
            var userMessageString = Console.ReadLine();
            if (userMessageString?.Trim().ToLower() == "q")
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            var userMessage = new ChatMessage(ChatRole.User, userMessageString);
            chatMessages.Enqueue(userMessage);
            while (chatMessages.Count > 20)
            {
                chatMessages.Dequeue();
            }
            var options = new ChatCompletionsOptions
            {
                Temperature = (float)0.7,
                MaxTokens = 800,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };
            options.Messages.Add(systemMessage);
            foreach (var chatMessage in chatMessages)
            {
                options.Messages.Add(chatMessage);
            }

            var response = await openAiClient.GetChatCompletionsStreamingAsync("gpt-35-turbo", options);
            //var response = await openAiClient.GetChatCompletionsAsync("gpt-35-turbo", options);
            using var streamingChatCompletions = response.Value;
            var assistantMessageStringBuilder = new StringBuilder();
            Console.WriteLine("Assistant: ");
            await foreach (var choice in streamingChatCompletions.GetChoicesStreaming())
            {
                await foreach (var item in choice.GetMessageStreaming())
                {
                    assistantMessageStringBuilder.Append(item.Content);
                    Console.Write(item.Content);
                }
            }
            Console.WriteLine();
            var assistantMessage = new ChatMessage(ChatRole.Assistant, assistantMessageStringBuilder.ToString());
            chatMessages.Enqueue(assistantMessage);
        }
    }
}
