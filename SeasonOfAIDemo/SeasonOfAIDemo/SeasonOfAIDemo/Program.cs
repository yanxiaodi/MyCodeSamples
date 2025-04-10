﻿// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SeasonOfAIDemo;
using SeasonOfAIDemo.ChatV1;

Console.WriteLine("Hello, Wellington .NET User Group! Welcome to join Season of AI!");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var (openAiOptions, bingSearchOptions, azureAiSearchOptions) = Helpers.GetConfigurations(builder);

using IHost host = builder.Build();

// Application code should start here.

await ChatDemoV1.Run(openAiOptions);
//await ChatDemoV2.Run(openAiOptions);
//await ChatDemoV3.Run(openAiOptions, bingSearchOptions);
//await ChatDemoV4.Run(openAiOptions, azureAiSearchOptions);

await host.RunAsync();

