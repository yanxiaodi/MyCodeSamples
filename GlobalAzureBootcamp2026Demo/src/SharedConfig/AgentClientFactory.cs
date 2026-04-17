using System.Reflection;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace SharedConfig;

/// <summary>
/// Reads Azure OpenAI settings from user secrets and provides a configured client.
/// </summary>
public static class AgentClientFactory
{
    /// <summary>
    /// The shared UserSecretsId used by every project in this solution.
    /// Each demo project's .csproj declares this same value as &lt;UserSecretsId&gt;,
    /// so <c>dotnet user-secrets set</c> only needs to be run once.
    /// </summary>
    public const string SharedUserSecretsId = "6e990080-ffbf-4bed-8b2d-abb21b747384";

    /// <summary>
    /// Reads Azure OpenAI settings from the shared user secrets store and returns
    /// a ready-to-use <see cref="AzureOpenAIClient"/> together with the deployment name.
    /// <para>
    /// If <c>AzureOpenAI:ApiKey</c> is set in user secrets, <see cref="AzureKeyCredential"/>
    /// is used (no <c>az login</c> required). Otherwise falls back to
    /// <see cref="DefaultAzureCredential"/>.
    /// </para>
    /// </summary>
    public static (AzureOpenAIClient Client, string DeploymentName) Create()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var config = new ConfigurationBuilder()
            .AddUserSecrets(assembly, optional: false)
            .Build();

        var settings = new AzureOpenAISettings
        {
            Endpoint       = config[$"{AzureOpenAISettings.SectionName}:Endpoint"]       ?? string.Empty,
            DeploymentName = config[$"{AzureOpenAISettings.SectionName}:DeploymentName"] ?? string.Empty,
            ApiKey         = config[$"{AzureOpenAISettings.SectionName}:ApiKey"],
        };

        settings.Validate();

        var uri = new Uri(settings.Endpoint);
        AzureOpenAIClient client = string.IsNullOrWhiteSpace(settings.ApiKey)
            ? new AzureOpenAIClient(uri, new DefaultAzureCredential())   // az login / managed identity
            : new AzureOpenAIClient(uri, new AzureKeyCredential(settings.ApiKey)); // API key

        return (client, settings.DeploymentName);
    }
}
