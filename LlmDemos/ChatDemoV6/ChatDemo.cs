﻿using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using AutoGen.SemanticKernel;
using AutoGen.SemanticKernel.Extension;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Shared;

namespace ChatDemoV6;
internal class ChatDemo
{
    public static async Task Run(OpenAiOptions openAiOptions)
    {
        Console.WriteLine("DemoV6 shows how to use AutoGen to create a group chat with multiple agents. You can ask them to create a story and translate to various languages. Type 'Ok' if you are happy with the story.");

        var userAgent = CreateUserProxyAgent();
        var storytellerAgent = CreateStorytellerAgent(openAiOptions);
        var reviewerAgent = CreateReviewerAgent(openAiOptions);
        var translatorAgent = CreateTranslatorAgent(openAiOptions);
        var adminAgent = CreateAdminAgent(openAiOptions);


        var groupChat = new GroupChat(
            admin: adminAgent,
            members:
            [
                userAgent,
                storytellerAgent,
                reviewerAgent,
                translatorAgent,
            ]
        );

        userAgent.SendIntroduction("I will interact with the system", groupChat);
        storytellerAgent.SendIntroduction("I will create stories based on user's input", groupChat);
        reviewerAgent.SendIntroduction("I will review stories generated by storyteller", groupChat);
        translatorAgent.SendIntroduction("I will translate the story to various languages", groupChat);

        var topic = Console.ReadLine();
        var taskMessage = new TextMessage(Role.User, topic);

        await foreach (var message in groupChat.SendAsync([taskMessage], maxRound: 20))
        {
            // teminate chat if message is from runner and run successfully
            if (message.From == "user" && message.GetContent().Contains("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Thanks!");
                break;
            }
        }
    }

    private static MiddlewareAgent<UserProxyAgent> CreateUserProxyAgent()
    {
        return new UserProxyAgent(name: "user", humanInputMode: HumanInputMode.ALWAYS).RegisterPrintMessage();
    }

    private static MiddlewareStreamingAgent<SemanticKernelAgent> CreateStorytellerAgent(OpenAiOptions openAiOptions)
    {
        // Create a kernel builder
        var builder = Kernel.CreateBuilder();

        // Add the Azure OpenAI chat completions service
        builder.AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key);

        var kernel = builder.Build();

        OpenAIPromptExecutionSettings settings = new()
        {
            MaxTokens = 4096,
            Temperature = 0.8f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: "storyteller",
                systemMessage: """
                               You are an talented storyteller who can generate stories based on user's input. The story should be short and funny.
                               Once you receive the user input, you will generate a story based on the input.
                               Once you complete the story, ask the reviewer to review the story.
                               If the reviewer is satisfied with the story, ask the user what language they want then ask the translator to translate the story to that language.
                               """,
                settings: settings)
            .RegisterMessageConnector()
            .RegisterPrintMessage();
        return kernelAgent;
    }

    private static MiddlewareStreamingAgent<SemanticKernelAgent> CreateReviewerAgent(OpenAiOptions openAiOptions)
    {
        // Create a kernel builder
        var builder = Kernel.CreateBuilder();

        // Add the Azure OpenAI chat completions service
        builder.AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key);

        var kernel = builder.Build();

        OpenAIPromptExecutionSettings settings = new()
        {
            MaxTokens = 4096,
            Temperature = 0.8f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: "reviewer",
                systemMessage: """
                               You are an story reviewer who can provide feedback on stories generated by the storyteller.
                               You will provide feedback to the storyteller from a 5-6 years old kid's perspective.
                               Once you complete the review, ask the storyteller to make changes to the story.
                               If you are satisfied with the changes, ask the translator to translate the story.
                               """,
                settings: settings)
            .RegisterMessageConnector()
            .RegisterPrintMessage();
        return kernelAgent;
    }

    private static MiddlewareStreamingAgent<SemanticKernelAgent> CreateTranslatorAgent(OpenAiOptions openAiOptions)
    {
        // Create a kernel builder
        var builder = Kernel.CreateBuilder();

        // Add the Azure OpenAI chat completions service
        builder.AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key);

        var kernel = builder.Build();

        OpenAIPromptExecutionSettings settings = new()
        {
            MaxTokens = 4096,
            Temperature = 0.7f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: "translator",
                systemMessage: """
                               You are an translator who can translate text from one language to another.
                               You can translate multiple languages and provide the translated text to the user.
                               Once you translation is complete, ask the user if they are satisfied with the output.
                               """,
                settings: settings)
            .RegisterMessageConnector()
            .RegisterPrintMessage();
        return kernelAgent;
    }

    public static IAgent CreateAdminAgent(OpenAiOptions openAiOptions)
    {
        var openaiClient = new OpenAIClient(new Uri(openAiOptions.Endpoint), new Azure.AzureKeyCredential(openAiOptions.Key));
        var admin = new OpenAIChatAgent(
                openAIClient: openaiClient,
                modelName: openAiOptions.Model,
                name: "admin",
                temperature: 0)
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        return admin;
    }
}
