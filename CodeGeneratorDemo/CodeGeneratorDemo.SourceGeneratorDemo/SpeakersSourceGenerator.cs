using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace CodeGeneratorDemo.SourceGeneratorDemo
{
    [Generator]
    public class SpeakersSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder(@"
using System;
namespace CodeGeneratorDemo.SourceGeneratorDemo
{
    public static class SpeakerHelper
    {
        public static void SayHello() 
        {
            Console.WriteLine(""Hello from generated code!"");
");
            sourceBuilder.Append(@"
        }
    }
}");
            // inject the created source into the users compilation
            context.AddSource("SpeakersSourceGenerator", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

        }
    }
}
