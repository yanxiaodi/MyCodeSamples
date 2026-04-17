namespace SharedConfig;

/// <summary>
/// Strongly-typed settings for an Azure OpenAI deployment.
/// Populate via user secrets (key prefix: AzureOpenAI:).
/// </summary>
public sealed class AzureOpenAISettings
{
    public const string SectionName = "AzureOpenAI";

    /// <summary>Azure OpenAI resource endpoint, e.g. https://&lt;name&gt;.openai.azure.com/</summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>Model deployment name, e.g. gpt-4o</summary>
    public string DeploymentName { get; init; } = string.Empty;

    /// <summary>
    /// Optional API key. When set, <see cref="AzureKeyCredential"/> is used instead of
    /// <see cref="Azure.Identity.DefaultAzureCredential"/> — no <c>az login</c> required.
    /// </summary>
    public string? ApiKey { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new InvalidOperationException(
                $"Azure OpenAI endpoint is not configured. " +
                $"Set '{SectionName}:Endpoint' in user secrets.");

        if (string.IsNullOrWhiteSpace(DeploymentName))
            throw new InvalidOperationException(
                $"Azure OpenAI deployment name is not configured. " +
                $"Set '{SectionName}:DeploymentName' in user secrets.");
    }
}
