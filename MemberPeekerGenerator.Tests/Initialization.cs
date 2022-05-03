using ExampleProject;
using Xunit.Abstractions;
using Xunit.Sdk;
using MemberPeekerCommon;

[assembly: Xunit.TestFramework("MemberPeekerGenerator.Tests.Initialization", "MemberPeekerGenerator.Tests")]
namespace MemberPeekerGenerator.Tests;

/// <summary>
/// This is called at assembly initialization. 
/// </summary>
public class Initialization : XunitTestFramework
{
    public Initialization(IMessageSink messageSink) : base(messageSink)
    {
        // Includes the external files marked by the attribute [CanPeeK] in the Compilation
        CompilationAggregator.AddToCompilation(typeof(ExampleClass).Assembly);
    }
}



