using MemberPeekerGenerator.MemberPeekerGenerator.Dtos;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MemberPeekerGenerator.MemberPeekerGenerator;

public static class ClassEditor
{
    public static string GenerateExtensionClass(Compilation compilation, ClassToGenerate classToGenerate)
    {
        var root = classToGenerate.Tree?.GetRoot();

        if (root == null)
        {
            return "//something went wrong";
        }

        if (!compilation.ContainsSyntaxTree(root.SyntaxTree))
        {
            compilation = compilation.AddSyntaxTrees(root.SyntaxTree);
        }

        var semanticModel = compilation.GetSemanticModel(root.SyntaxTree);
        var descendants = root.DescendantNodes().ToArray();

        var leadingWhitespaceTrivia = SyntaxTriviaList.Create(SyntaxTrivia(SyntaxKind.WhitespaceTrivia, "    "));
        var trailingWhitespace = SyntaxTriviaList.Create(SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));
        var token = Token(leadingWhitespaceTrivia, SyntaxKind.PublicKeyword, trailingWhitespace);
        var tokenList = TokenList(token);
        var type = descendants.OfType<TypeDeclarationSyntax>().First();
        root = root.ReplaceNodes(descendants,
            (_, y) =>
            {
                return y switch
                {
                    TypeDeclarationSyntax classDeclarationSyntax
                        => GetModifiedClassDeclaration(tokenList, classDeclarationSyntax, classToGenerate.Symbol, type),

                    ConstructorDeclarationSyntax constructorDeclarationSyntax
                        => constructorDeclarationSyntax.WithIdentifier(Identifier(classToGenerate.Name + "_Exposed")),

                    ParameterSyntax parameterSyntax
                            => GetQualifiedNameToParameter(parameterSyntax, semanticModel),

                    MethodDeclarationSyntax methodDeclarationSyntax
                        => GetQualifiedNameAndAccessorToMethod(methodDeclarationSyntax, tokenList, classToGenerate.Symbol),

                    ObjectCreationExpressionSyntax objectCreationExpressionSyntax
                        => GetQualifiedNameForObjectCreation(objectCreationExpressionSyntax, semanticModel, descendants),
                    FieldDeclarationSyntax fieldDeclarationSyntax
                        => GetQualifiedNameToVariable(fieldDeclarationSyntax, tokenList, semanticModel),

                    BaseNamespaceDeclarationSyntax baseNamespaceDeclaration
                        => baseNamespaceDeclaration.WithName(IdentifierName("MemberPeeker")),

                    MemberDeclarationSyntax memberDeclaration
                        => GetMemberAsPublic(memberDeclaration, tokenList),

                    _ => y
                };
            });


        return $"{root}".Replace("[CanPeek]", "");
    }

    private static MemberDeclarationSyntax GetMemberAsPublic(MemberDeclarationSyntax memberDeclaration, SyntaxTokenList tokenList)
    {
        if (memberDeclaration is EnumMemberDeclarationSyntax)
        {
            return memberDeclaration;
        }
        return memberDeclaration.WithModifiers(tokenList);
    }

    private static ObjectCreationExpressionSyntax GetQualifiedNameForObjectCreation(ObjectCreationExpressionSyntax syntax, SemanticModel semanticModel, SyntaxNode[] tree)
    {
        var typeSymbol = semanticModel.GetTypeInfo(syntax).Type;
        return tree.OfType<TypeDeclarationSyntax>().Any(x => x.Identifier.ValueText == typeSymbol?.Name)
            ? syntax
            : syntax.WithType(ParseTypeName($"{typeSymbol}"));
    }

    public static TypeDeclarationSyntax GetModifiedClassDeclaration(SyntaxTokenList syntaxTokenList,
        TypeDeclarationSyntax syntax, INamedTypeSymbol type, TypeDeclarationSyntax original)
    {
        if (syntax.Identifier.ValueText.Equals(original.Identifier.ValueText))
        {
            return syntax.WithIdentifier(Identifier($"{syntax.Identifier.ValueText}_Exposed<T> where T : {type}\r"));
        }
        return syntax.WithModifiers(syntaxTokenList);
    }

    private static MethodDeclarationSyntax GetQualifiedNameAndAccessorToMethod(MethodDeclarationSyntax syntax,
        SyntaxTokenList syntaxTokenList, INamedTypeSymbol type)
    {
        // Convoluted check to say "this is the same signature".
        if (type.GetMembers().FirstOrDefault(x =>
                GeneratorHelpers.IsAMethod(x)
                && x.Name == syntax.Identifier.ValueText
                && ((IMethodSymbol)x).Parameters.Select(y => y.Name)
                                     .SequenceEqual(syntax.ParameterList.Parameters
                                     .Select(y => y.Identifier.ValueText))) is not IMethodSymbol method
                                     || method.ReturnsVoid)
        {
            return syntax.WithModifiers(syntaxTokenList);
        }

        return syntax.WithModifiers(syntaxTokenList).WithReturnType(ParseTypeName($"{method.ReturnType} "));
    }

    private static ParameterSyntax GetQualifiedNameToParameter(ParameterSyntax syntax, SemanticModel semanticModel)
    {
        if (syntax.Type == null)
        {
            return syntax;
        }

        return semanticModel.GetTypeInfo(syntax.Type).Type is not INamedTypeSymbol type
                ? syntax
                : syntax.WithType(ParseTypeName($"{type} "));
    }

    private static FieldDeclarationSyntax GetQualifiedNameToVariable(FieldDeclarationSyntax syntax,
      SyntaxTokenList syntaxTokenList, SemanticModel semanticModel)
    {
        if (semanticModel.GetTypeInfo(syntax.Declaration.Type).Type is not INamedTypeSymbol type)
        {
            return syntax;
        }

        return syntax.WithDeclaration(VariableDeclaration(ParseTypeName($"{type} "), syntax.Declaration.Variables)).WithModifiers(syntaxTokenList);
    }
}