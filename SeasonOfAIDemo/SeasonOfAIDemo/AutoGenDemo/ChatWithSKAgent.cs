using AutoGen.Core;
using AutoGen.SemanticKernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AutoGenDemo;
internal class ChatWithSKAgent
{
    public static async Task Run(OpenAiOptions openAiOptions)
    {
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key);
        var kernel = builder.Build();

        // create a semantic kernel agent
        var semanticKernelAgent = new SemanticKernelAgent(
            kernel: kernel,
            name: "assistant",
            systemMessage: "You are an assistant that help user to do some tasks.")
            .RegisterPrintMessage();

        // SemanticKernelAgent supports the following message types:
        // - IMessage<ChatMessageContent> where ChatMessageContent is from Azure.AI.OpenAI

        var helloMessage = new ChatMessageContent(AuthorRole.User, "Hello");

        // Use MessageEnvelope.Create to create an IMessage<ChatRequestMessage>
        var chatMessageContent = MessageEnvelope.Create(helloMessage);
        var reply = await semanticKernelAgent.SendAsync(chatMessageContent);

        // The type of reply is MessageEnvelope<ChatResponseMessage> where ChatResponseMessage is from Azure.AI.OpenAI
        //reply.Should().BeOfType<MessageEnvelope<ChatMessageContent>>();

        // You can un-envelop the reply to get the ChatResponseMessage
        //ChatMessageContent response = (reply as MessageEnvelope<ChatMessageContent>).Content;
        //response.Role.Should().Be(AuthorRole.Assistant);
    }
}
