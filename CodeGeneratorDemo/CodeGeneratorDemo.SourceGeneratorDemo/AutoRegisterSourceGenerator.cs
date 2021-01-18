using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CodeGeneratorDemo.SourceGeneratorDemo
{
    [Generator]
    public class AutoRegisterSourceGenerator : ISourceGenerator
    {
        // The attribute that allows decorating methods with [CompileTimeExecutor] can be added to the compilation
        private const string AttributeText = @"
using System;
namespace CodeGeneratorDemo.SourceGeneratorDemo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class AutoRegisterAttribute : Attribute
    {
        public AutoRegisterAttribute()
        {
        }
    }
}";
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            // If you want to debug the Source Generator, please uncomment the below code.
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("AutoRegisterAttribute", SourceText.From(AttributeText, Encoding.UTF8));
            if (!(context.SyntaxReceiver is MySyntaxReceiver receiver))
            {
                return;
            }
            CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            SyntaxTree attributeSyntaxTree =
                CSharpSyntaxTree.ParseText(SourceText.From(AttributeText, Encoding.UTF8), options);
            Compilation compilation = context.Compilation.AddSyntaxTrees(attributeSyntaxTree);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(@"
using System;
using CodeGeneratorDemo.Client.Core;
namespace CodeGeneratorDemo.SourceGeneratorDemo
{
    public class RegisterHelper
    {
        public static void RegisterServices()
        {
");
            // Get all the classes with the AutoRegisterAttribute
            INamedTypeSymbol attributeSymbol =
                compilation.GetTypeByMetadataName("CodeGeneratorDemo.SourceGeneratorDemo.AutoRegisterAttribute");
            foreach (var candidateClass in receiver.CandidateClasses)
            {
                SemanticModel model = compilation.GetSemanticModel(candidateClass.SyntaxTree);
                if (model.GetDeclaredSymbol(candidateClass) is ITypeSymbol typeSymbol &&
                    typeSymbol.GetAttributes().Any(x =>
                        x.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                {
                    stringBuilder.Append($@"
            DiContainerMocker.RegisterService<I{candidateClass.Identifier.Text}, {candidateClass.Identifier.Text}>(new {candidateClass.Identifier.Text}());");
                }
            }
            stringBuilder.Append(@"
        }
    }
}");
            context.AddSource("RegisterServiceHelper", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
        }
    }

    public class MySyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any method with at least one attribute is a candidate for property generation
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.AttributeLists.Count >= 0)
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }

}
