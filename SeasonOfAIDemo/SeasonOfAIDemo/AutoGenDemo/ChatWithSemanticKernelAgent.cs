#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using AutoGen;
using AutoGen.Core;
using AutoGen.SemanticKernel;
using AutoGen.SemanticKernel.Extension;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace AutoGenDemo;
internal class ChatWithSemanticKernelAgent
{
    public static async Task Run(OpenAiOptions openAiOptions, BingSearchOptions bingSearchOptions)
    {
        var userProxyAgent = new UserProxyAgent(
                name: "user",
                humanInputMode: HumanInputMode.ALWAYS)
            .RegisterPrintMessage();

        var bingSearchAgent = CreateBingSearchAgent(openAiOptions, bingSearchOptions);
        var summarizerAgent = CreateSummarizerAgent(openAiOptions);

        var groupChat = new RoundRobinGroupChat(
            agents: [userProxyAgent, bingSearchAgent, summarizerAgent]);

        var groupChatAgent = new GroupChatManager(groupChat);

        var history = await userProxyAgent.InitiateChatAsync(
            receiver: groupChatAgent,
            message: "How to deploy an openai resource on azure",
            maxRound: 10);
    }

    private static MiddlewareStreamingAgent<SemanticKernelAgent> CreateBingSearchAgent(OpenAiOptions openAiOptions, BingSearchOptions bingSearchOptions)
    {
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key); ;


        // Add a plugin to use Bing search
        var bingConnector = new BingConnector(bingSearchOptions.Key);
        var webSearchPlugin = new WebSearchEnginePlugin(bingConnector);
        builder.Plugins.AddFromObject(webSearchPlugin);
        var kernel = builder.Build();

        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: "bing-search",
                systemMessage: """
                               You search results from Bing and return it as-is.
                               You put the original search result between ```bing and ```

                               e.g.
                               ```bing
                               xxx
                               ```
                               """)
            .RegisterMessageConnector()
            .RegisterPrintMessage(); // pretty print the message
        return kernelAgent;
    }


    private static MiddlewareStreamingAgent<SemanticKernelAgent> CreateSummarizerAgent(OpenAiOptions openAiOptions)
    {
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(openAiOptions.Model, openAiOptions.Endpoint, openAiOptions.Key); ;

        // Add a plugin to use Bing search
        //var bingConnector = new BingConnector(bingSearchOptions.Key);
        //var webSearchPlugin = new WebSearchEnginePlugin(bingConnector);
        //builder.Plugins.AddFromObject(webSearchPlugin);
        var kernel = builder.Build();

        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: "summarize",
                systemMessage: """
                               You summarize search result from bing in a short and concise manner
                               """)
            .RegisterMessageConnector()
            .RegisterPrintMessage(); // pretty print the message
        return kernelAgent;
    }
}
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
