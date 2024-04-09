// See https://aka.ms/new-console-template for more information

using CodeCampWellington2024;
using CodeCampWellington2024.ChatV2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var openAiOptions = builder.Configuration.GetSection(nameof(OpenAiOptions)).Get<OpenAiOptions>();
if (openAiOptions is null)
{
    throw new InvalidOperationException("OpenAiOptions is null");
}
var bingSearchOptions = builder.Configuration.GetSection(nameof(BingSearchOptions)).Get<BingSearchOptions>();
if (bingSearchOptions is null)
{
    throw new InvalidOperationException("BingSearchOptions is null");
}

using IHost host = builder.Build();

// Application code should start here.

//await ChatDemoV1.Run(openAiOptions);
await ChatDemoV2.Run(openAiOptions);
//await ChatDemoV3.Run(openAiOptions, bingSearchOptions);


await host.RunAsync();