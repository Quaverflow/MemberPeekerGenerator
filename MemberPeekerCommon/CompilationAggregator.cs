using System.Reflection;
using System.Text;

namespace MemberPeekerCommon;

public static class CompilationAggregator
{
    private static bool _done;

    /// <summary>
    /// Gets all the types in the referenced assembly and adds them to the compilation
    /// </summary>
    /// <param name="assembly"></param>
    public static void AddToCompilation(params Assembly[] assemblies)
    {
        if (_done)
        {
            return;
        }

        var paths = new List<string>();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(CanPeekAttribute), true).Length > 0);
            paths.AddRange(types.Select(x => (CanPeekAttribute)x.GetCustomAttributes(typeof(CanPeekAttribute), true)
                   .First())
               .Select(y => y.FilePath));
        }

        var projectPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName + "\\Directory.Build.Props";
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("<Project>");
        stringBuilder.AppendLine("  <ItemGroup>");
        foreach (var path in paths)
        {
            if (path == null)
            {
                continue;
            }

            var index = path.LastIndexOf("\\");
            if (index == -1)
            {
                continue;
            }

            var lastSlash = path.Substring(index);
            stringBuilder.AppendLine($"      <CSFile Include=\"{path}\"/>");
            stringBuilder.AppendLine($"      <FilesToMove Include=\"$(MSBuildProjectDirectory){lastSlash}\"/>");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("  </ItemGroup>");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("  <Target Name=\"Build\">");
        stringBuilder.AppendLine("      <Move SourceFiles=\"@(FilesToMove)\" DestinationFolder=\"$(MSBuildProjectDirectory)\\ImportedTypes\" />");
        stringBuilder.AppendLine("  </Target> ");
        stringBuilder.AppendLine("</Project>");

        File.WriteAllText(projectPath, stringBuilder.ToString());
        _done = true;
    }
}
