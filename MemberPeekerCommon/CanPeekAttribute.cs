using System.Runtime.CompilerServices;

namespace MemberPeekerCommon;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
public class CanPeekAttribute : Attribute
{
    public readonly string FilePath;

    public CanPeekAttribute([CallerFilePath] string filePath = "")
    {
        FilePath = filePath;
    }
}
