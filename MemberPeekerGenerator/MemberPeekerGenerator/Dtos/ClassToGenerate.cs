using Microsoft.CodeAnalysis;

namespace MemberPeekerGenerator.MemberPeekerGenerator.Dtos;

public struct ClassToGenerate
{
    public readonly string Name;
    public readonly INamedTypeSymbol Symbol;
    public readonly SyntaxTree? Tree;

    public ClassToGenerate(string className, SyntaxTree? tree, INamedTypeSymbol symbol)
    {
        Name = className;
        Tree = tree;
        Symbol = symbol;
    }
}