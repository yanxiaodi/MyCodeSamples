namespace AutoGenDemo;
internal sealed class OpenAiOptions
{
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

internal sealed class BingSearchOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

internal sealed class AzureAiSearchOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Index { get; set; } = string.Empty;
}