﻿// See https://aka.ms/new-console-template for more information

using Demo4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shared;

Console.WriteLine("Hello, World!");
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var openAiOptions = Helpers.GetConfigurations(builder);

using IHost host = builder.Build();

await ChatDemo.Run(openAiOptions);