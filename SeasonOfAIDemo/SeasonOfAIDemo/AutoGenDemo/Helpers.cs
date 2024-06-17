using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AutoGenDemo;
internal static class Helpers
{
    internal static (OpenAiOptions, BingSearchOptions, AzureAiSearchOptions) GetConfigurations(HostApplicationBuilder hostApplicationBuilder)
    {
        var openAiOptions = hostApplicationBuilder.Configuration.GetSection(nameof(OpenAiOptions)).Get<OpenAiOptions>();
        if (openAiOptions is null)
        {
            throw new InvalidOperationException("OpenAiOptions is null");
        }
        var bingSearchOptions = hostApplicationBuilder.Configuration.GetSection(nameof(BingSearchOptions)).Get<BingSearchOptions>();
        if (bingSearchOptions is null)
        {
            throw new InvalidOperationException("BingSearchOptions is null");
        }
        var azureAiSearchOptions = hostApplicationBuilder.Configuration.GetSection(nameof(AzureAiSearchOptions)).Get<AzureAiSearchOptions>();
        if (azureAiSearchOptions is null)
        {
            throw new InvalidOperationException("AzureAiSearchOptions is null");
        }

        return (openAiOptions, bingSearchOptions, azureAiSearchOptions);
    }
}
