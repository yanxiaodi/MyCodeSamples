// See https://aka.ms/new-console-template for more information

using AutoGenDemo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, Wellington .NET User Group! Welcome to join Season of AI!");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var (openAiOptions, bingSearchOptions, azureAiSearchOptions) = Helpers.GetConfigurations(builder);

using IHost host = builder.Build();

//await ChatWithSKAgent.Run(openAiOptions);
await ChatWithAssistantAgent.Run(openAiOptions);

await host.RunAsync();