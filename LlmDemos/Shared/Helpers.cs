using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Shared;
public static class Helpers
{
    public static OpenAiOptions GetConfigurations(HostApplicationBuilder hostApplicationBuilder)
    {
        var openAiOptions = hostApplicationBuilder.Configuration.GetSection(nameof(OpenAiOptions)).Get<OpenAiOptions>();
        if (openAiOptions is null)
        {
            throw new InvalidOperationException("OpenAiOptions is null");
        }

        return openAiOptions;
    }

    public static BingSearchOptions GetBingSearchOptions(HostApplicationBuilder hostApplicationBuilder)
    {
        var bingSearchOptions = hostApplicationBuilder.Configuration.GetSection(nameof(BingSearchOptions)).Get<BingSearchOptions>();
        if (bingSearchOptions is null)
        {
            throw new InvalidOperationException("BingSearchOptions is null");
        }

        return bingSearchOptions;
    }
}