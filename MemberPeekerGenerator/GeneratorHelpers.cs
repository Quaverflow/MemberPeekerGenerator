using Microsoft.CodeAnalysis;

namespace MemberPeekerGenerator;

public static class GeneratorHelpers
{
    /// <summary>
    /// Checks if symbol is a method
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public static bool IsAMethod(ISymbol member) =>
        member.DeclaredAccessibility == Accessibility.Public
        && member.Name != ".ctor"
        && member is IMethodSymbol
        && !member.Name.StartsWith("get");

    /// <summary>
    /// Generates collections if type is determined to be IEnumerable
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string? GetCollectionIfBaseIsIEnumerable(INamedTypeSymbol type)
    {
        if (type.AllInterfaces.Any(x => x.Name is "IEnumerable"))
        {
            return type.Name.StartsWith("IDictionary")
                ? $"new System.Collections.Generic.Dictionary<{string.Join(",", type.TypeArguments)}>()"
                : $"new System.Collections.Generic.List<{string.Join(",", type.TypeArguments)}>()";
        }
        return null;
    }

    /// <summary>
    /// Generates default value for primitives
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string? GetDefaultIfPrimitive(INamedTypeSymbol type)
    {
        var symbol = type.ToString();
        return symbol switch
        {
            "double" or "float" or "decimal" or "byte" or "sbyte" or "short"
                or "ushort" or "int" or "uint" or "long" or "ulong" => "0",
            "string" => "string.Empty",
            "bool" => "false",
            "char" => "char.MinValue",
            _ => type.CreateObjectInstanceFromSymbol()
        };
    }

    /// <summary>
    /// Generates a type and its required constructor types recursively
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string? CreateObjectInstanceFromSymbol(this INamedTypeSymbol type)
    {
        var constructors = type.InstanceConstructors;
        var constructor = constructors.OrderBy(x => x.Parameters.Length)
                                                   .FirstOrDefault(x => x.DeclaredAccessibility is Accessibility.Public);
        if (constructor is null)
        {
            //todo handle non public constructor
            return null;
        }

        var parameters = string.Empty;
        foreach (var parameter in constructor.Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                parameters += GenerateTypes((INamedTypeSymbol)parameter.Type);
            }
            else
            {
                parameters += ", " + GenerateTypes((INamedTypeSymbol)parameter.Type);
            }
        }

        return $"new {type}({parameters})";


        static string GenerateTypes(INamedTypeSymbol returnType)
        {
            var returnValue = GetDefaultIfPrimitive(returnType);
            if (returnValue is null)
            {
                var newMock = $"new Mock<{returnType}>().Object";
                var ifClass = returnType.IsAbstract ? newMock : returnType.CreateObjectInstanceFromSymbol();

                returnValue = returnType.TypeKind switch
                {
                    TypeKind.Class => $"{ifClass}",
                    TypeKind.Struct => $"{returnType.CreateObjectInstanceFromSymbol()}",
                    TypeKind.Interface => GetCollectionIfBaseIsIEnumerable(returnType) ?? newMock,
                    _ => "default"
                };
            }

            return returnValue;
        }
    }

}