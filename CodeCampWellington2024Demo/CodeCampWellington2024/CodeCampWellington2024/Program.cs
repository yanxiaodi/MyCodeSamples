// See https://aka.ms/new-console-template for more information

using CodeCampWellington2024;
using CodeCampWellington2024.ChatV1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var openAiOptions = builder.Configuration.GetSection(nameof(OpenAiOptions)).Get<OpenAiOptions>();
if (openAiOptions is null)
{
    throw new InvalidOperationException("ConfigOptions is null");
}

using IHost host = builder.Build();

// Application code should start here.
Console.WriteLine($"Model: {openAiOptions.Model}");
Console.WriteLine($"Endpoint: {openAiOptions.Endpoint}");
Console.WriteLine($"Key: {openAiOptions.Key}");

await ChatDemoV1.Run(openAiOptions);

await host.RunAsync();