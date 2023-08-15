using Microsoft.SemanticKernel;

namespace SemanticKernelDemo.App;
internal static partial class GettingStarted
{
    public static IKernel InstantiateKernel()
    {
        var builder = new KernelBuilder();
        var (model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

        builder.WithAzureChatCompletionService(model, azureEndpoint, apiKey);
        var kernel = builder.Build();

        Console.WriteLine("Hello, Azure OpenAI!");
        return kernel;
    }
}
