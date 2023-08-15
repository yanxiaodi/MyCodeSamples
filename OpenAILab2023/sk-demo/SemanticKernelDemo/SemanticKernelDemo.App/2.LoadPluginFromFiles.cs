using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;

namespace SemanticKernelDemo.App;
internal static partial class GettingStarted
{
    public static async Task LoadPluginFromFiles(this IKernel kernel)
    {
        // Import a plugin from a directory
        Console.WriteLine("-------------------------------------");
        Console.WriteLine("Loading plugin from directory...");
        var skillDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        var funSkillFunctions = kernel.ImportSemanticSkillFromDirectory(skillDirectory, "FunSkill");
        var jokeFunction = funSkillFunctions.FirstOrDefault(x => x.Key == "Joke").Value;
        // Set the context variables
        var context = new ContextVariables();
        // You can change the style and input to get different results
        context.Set("style", "warm");
        context.Set("input", "computer");
        var result = await jokeFunction.InvokeAsync(context);
        Console.WriteLine(result);
        Console.WriteLine("-------------------------------------");
        Console.WriteLine();
    }
}
