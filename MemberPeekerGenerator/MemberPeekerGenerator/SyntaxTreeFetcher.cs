using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MemberPeekerGenerator.MemberPeekerGenerator;

public static class SyntaxTreeFetcher
{

    public static SyntaxTree? GetSyntaxTree(INamedTypeSymbol parameterSymbol)
    {
        SyntaxTree? tree;
        if (parameterSymbol.Locations.First().Kind == LocationKind.MetadataFile)
        {
            var attributes = parameterSymbol.GetAttributes();
            var attribute = attributes.FirstOrDefault(x => x.AttributeClass?.Name == "CanPeekAttribute");
            var filePath = (string)(attribute?.ConstructorArguments[0].Value ?? string.Empty);
            tree = GetTreeFromMetadata(filePath);
        }
        else
        {
            var location = parameterSymbol.Locations.First();
            tree = location.SourceTree;
        }

        return tree;
    }

    public static SyntaxTree GetTreeFromMetadata(string name)
    {
        var code = File.ReadAllText(name);
        return CSharpSyntaxTree.ParseText(code).WithChangedText(SourceText.From(code, Encoding.UTF8));
    }
}