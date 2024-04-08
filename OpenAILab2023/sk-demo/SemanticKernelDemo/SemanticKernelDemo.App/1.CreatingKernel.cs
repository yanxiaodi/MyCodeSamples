using Microsoft.SemanticKernel;
using Kernel = Microsoft.SemanticKernel.Kernel;

namespace SemanticKernelDemo.App;
internal static partial class GettingStarted
{
    public static Kernel InstantiateKernel()
    {
        var builder = new Kernel.CreateBuilder();
        var (model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

        builder.WithAzureChatCompletionService(model, azureEndpoint, apiKey);
        var kernel = builder.Build();

        Console.WriteLine("Hello, Azure OpenAI!");
        return kernel;
    }
}
