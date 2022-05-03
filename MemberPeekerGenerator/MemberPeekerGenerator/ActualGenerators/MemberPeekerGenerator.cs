using MemberPeekerGenerator.MemberPeekerGenerator.Dtos;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace MemberPeekerGenerator.MemberPeekerGenerator.ActualGenerators;

[Generator]
public class MemberPeekerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
               static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<SyntaxNode> classes, SourceProductionContext context)
    {
        try
        {
            if (classes.IsDefaultOrEmpty)
            {
                return;
            }

            var classesToGenerate = classes.Distinct();

            var convertedClassesToGenerate = GetTypesToGenerate(compilation, classesToGenerate, context.CancellationToken);

            foreach (var classTree in convertedClassesToGenerate)
            {
                var result = ClassEditor.GenerateExtensionClass(compilation, classTree);
                context.AddSource($"{classTree.Name}_Exposed.cs", SourceText.From(result, Encoding.UTF8));
            }
        }
        catch (Exception e)
        {
            context.AddSource($"error.cs", $"/*\nSomething went wrong, please report this bug. \n {e.Message} \n {e.StackTrace}\n*/");
        }
    }

    private static List<ClassToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<SyntaxNode?> classes, CancellationToken ct)
    {
        var classesToGenerate = new List<ClassToGenerate>();

        foreach (var parameterDeclaration in classes)
        {
            ct.ThrowIfCancellationRequested();

            if (parameterDeclaration == null)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(parameterDeclaration.SyntaxTree);
            var symbol = semanticModel?.GetTypeInfo(parameterDeclaration).Type;
            if (symbol is not INamedTypeSymbol parameterSymbol)
            {
                continue;
            }

            var tree = SyntaxTreeFetcher.GetSyntaxTree(parameterSymbol);
            if (tree != null)
            {
                classesToGenerate.Add(new ClassToGenerate(parameterSymbol.Name, tree, parameterSymbol));
            }
        }
        return classesToGenerate;

    }
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is ObjectCreationExpressionSyntax
            {
                Type: QualifiedNameSyntax
                {

                    Left: IdentifierNameSyntax nameSpace,
                    Right: GenericNameSyntax
                }
            })
        {
            if (nameSpace.Identifier.ValueText.StartsWith("MemberPeeker"))
            {
                return true;
            }
        }

        return false;
    }

    private static SyntaxNode GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var methodDeclarationSyntax = (ObjectCreationExpressionSyntax)context.Node;
        var name = (QualifiedNameSyntax)methodDeclarationSyntax.Type;
        return ((GenericNameSyntax)name.Right).TypeArgumentList.Arguments[0];
    }

}