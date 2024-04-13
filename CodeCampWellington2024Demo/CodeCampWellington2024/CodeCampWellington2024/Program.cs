// See https://aka.ms/new-console-template for more information

using CodeCampWellington2024;
using CodeCampWellington2024.ChatV2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, Code Camp Wellington 2024!");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var (openAiOptions, bingSearchOptions, azureAiSearchOptions) = Helpers.GetConfigurations(builder);

using IHost host = builder.Build();

// Application code should start here.

//await ChatDemoV1.Run(openAiOptions);
await ChatDemoV2.Run(openAiOptions);
//await ChatDemoV3.Run(openAiOptions, bingSearchOptions);
//await ChatDemoV4.Run(openAiOptions, azureAiSearchOptions);

await host.RunAsync();

