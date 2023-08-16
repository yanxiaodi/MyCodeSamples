using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System.Text.Json;

namespace SemanticKernelDemo.App;
internal static partial class GettingStarted
{
    public static async Task UsePlanner(this IKernel kernel)
    {
        Console.WriteLine("-------------------------------------");
        Console.WriteLine("Using the planner...");

        // Load native skill into the kernel registry, sharing its functions with prompt templates
        var planner = new SequentialPlanner(kernel);

        // Provide plugins to the planner
        var skillsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins");
        kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "SummarizeSkill");
        kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "WriterSkill");

        // Make a request to the planner
        var ask = "Tomorrow is Valentine's day. I need to come up with a few date ideas and e-mail them to my significant other.";
        var originalPlan = await planner.CreatePlanAsync(ask);

        // Print the plan
        Console.WriteLine("Original plan:\n");
        Console.WriteLine(JsonSerializer.Serialize(originalPlan, new JsonSerializerOptions { WriteIndented = true }));

        // Execute the plan
        var originalPlanResult = await originalPlan.InvokeAsync();

        Console.WriteLine("Original Plan results:\n");
        Console.WriteLine(Utils.WordWrap(originalPlanResult.Result, 100));

        Console.WriteLine("-------------------------------------");
        Console.WriteLine();
    }

    public static async Task UsePlannerWithAdditionalFunction(this IKernel kernel)
    {
        Console.WriteLine("-------------------------------------");
        Console.WriteLine("Using the planner with an additional function...");

        // Load native skill into the kernel registry, sharing its functions with prompt templates
        var planner = new SequentialPlanner(kernel);

        // Provide plugins to the planner
        var skillsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins");
        kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "SummarizeSkill");
        kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "WriterSkill");

        // Add a function to the planner
        string skPrompt = """
            {{$input}}
            
            Rewrite the above in the style of Shakespeare.
            """;
        var shakespeareFunction = kernel.CreateSemanticFunction(skPrompt, "shakespeare", "ShakespeareSkill", maxTokens: 2000, temperature: 0.2, topP: 0.5);

        // Make a request to the planner
        var ask = @"Tomorrow is Valentine's day. I need to come up with a few date ideas.
            She likes Shakespeare so write using his style. E-mail these ideas to my significant other";
        var newPlan = await planner.CreatePlanAsync(ask);

        // Print the plan
        Console.WriteLine("Updated plan:\n");
        Console.WriteLine(JsonSerializer.Serialize(newPlan, new JsonSerializerOptions { WriteIndented = true }));

        // Execute the plan
        var newPlanResult = await kernel.RunAsync(newPlan);
        Console.WriteLine("New Plan results:\n");
        Console.WriteLine(newPlanResult.Result);

        Console.WriteLine("-------------------------------------");
        Console.WriteLine();
    }
}
