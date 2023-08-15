// See https://aka.ms/new-console-template for more information
using SemanticKernelDemo.App;

Console.WriteLine("Hello, World!");

// Instantiate a kernel
var kernel = GettingStarted.InstantiateKernel();

// Load a plugin from a directory
//await kernel.LoadPluginFromFiles();

// Load a plugin from inline code
//await kernel.LoadPluginFromInline();

// Set context variables
//await kernel.SetContextVariables();

// Chain functions
await kernel.RunFunctionsSequentially();

