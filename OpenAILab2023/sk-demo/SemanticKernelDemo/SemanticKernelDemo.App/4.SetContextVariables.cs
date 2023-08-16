using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;

namespace SemanticKernelDemo.App;
internal static partial class GettingStarted
{
    public static async Task SetContextVariables(this IKernel kernel)
    {
        Console.WriteLine("-------------------------------------");
        Console.WriteLine("Setting context variables...");

        // Create the semantic function for the chat agent
        const string skPrompt = @"
            ChatBot can have a conversation with you about any topic.
            It can give explicit instructions or say 'I don't know' if it does not have an answer.
            
            {{$history}}
            User: {{$userInput}}
            ChatBot:";

        var promptConfig = new PromptTemplateConfig
        {
            Completion =
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            }
        };

        var promptTemplate = new PromptTemplate(skPrompt, promptConfig, kernel);
        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
        var chatFunction = kernel.RegisterSemanticFunction("ChatBot", "Chat", functionConfig);

        // Initialize the context
        var context = kernel.CreateNewContext();
        var history = "";
        Func<string, Task> Chat = async input =>
        {
            // Save new message in the context variables
            context.Variables["userInput"] = input;

            // Process the user message and get an answer
            var answer = await chatFunction.InvokeAsync(context);

            // Append the new interaction to the chat history
            history += $"\nUser: {input}\nMelody: {answer}\n";
            context.Variables["history"] = history;

            // Show the response
            Console.WriteLine(context);
        };

        Console.WriteLine("Type 'exit' to quit.");

        var userInput = Console.ReadLine();
        while (userInput.ToLower() != "exit")
        {
            await Chat(userInput);
            userInput = Console.ReadLine();
        }

        Console.WriteLine("-------------------------------------");
        Console.WriteLine();
    }
}
