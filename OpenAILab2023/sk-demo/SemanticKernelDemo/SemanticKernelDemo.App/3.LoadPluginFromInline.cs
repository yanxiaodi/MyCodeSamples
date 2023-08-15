using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;

namespace SemanticKernelDemo.App;
internal static partial class GettingStarted
{
    public static async Task LoadPluginFromInline(this IKernel kernel)
    {
        Console.WriteLine("-------------------------------------");
        Console.WriteLine("Loading plugin from inline code...");
        // Create the text for the prompt template
        string skPrompt = """
            {{$input}}

            Summarize the content above.
            """;

        // Set the config
        var promptConfig = new PromptTemplateConfig
        {
            Completion =
            {
                MaxTokens = 200,
                Temperature = 0.2,
                TopP = 0.5,
            }
        };

        // Create the prompt template
        var promptTemplate = new PromptTemplate(
            skPrompt,                        // Prompt template defined in natural language
            promptConfig,                    // Prompt configuration
            kernel                           // SK instance
        );

        // Register the plugin
        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

        var summaryFunction = kernel.RegisterSemanticFunction("MySkill", "Summary", functionConfig);

        // Invoke the plugin
        var input = """
Demo (ancient Greek poet)
From Wikipedia, the free encyclopedia
Demo or Damo (Greek: Δεμώ, Δαμώ; fl. c. AD 200) was a Greek woman of the Roman period, known for a single epigram, engraved upon the Colossus of Memnon, which bears her name. She speaks of herself therein as a lyric poetess dedicated to the Muses, but nothing is known of her life.[1]
Identity
Demo was evidently Greek, as her name, a traditional epithet of Demeter, signifies. The name was relatively common in the Hellenistic world, in Egypt and elsewhere, and she cannot be further identified. The date of her visit to the Colossus of Memnon cannot be established with certainty, but internal evidence on the left leg suggests her poem was inscribed there at some point in or after AD 196.[2]
Epigram
There are a number of graffiti inscriptions on the Colossus of Memnon. Following three epigrams by Julia Balbilla, a fourth epigram, in elegiac couplets, entitled and presumably authored by "Demo" or "Damo" (the Greek inscription is difficult to read), is a dedication to the Muses.[2] The poem is traditionally published with the works of Balbilla, though the internal evidence suggests a different author.[1]
In the poem, Demo explains that Memnon has shown her special respect. In return, Demo offers the gift for poetry, as a gift to the hero. At the end of this epigram, she addresses Memnon, highlighting his divine status by recalling his strength and holiness.[2]
Demo, like Julia Balbilla, writes in the artificial and poetic Aeolic dialect. The language indicates she was knowledgeable in Homeric poetry—'bearing a pleasant gift', for example, alludes to the use of that phrase throughout the Iliad and Odyssey.[a][2] 
""";

        var summary = await summaryFunction.InvokeAsync(input);

        Console.WriteLine(summary);
        Console.WriteLine("-------------------------------------");
        Console.WriteLine();
    }
}
