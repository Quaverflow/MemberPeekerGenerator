using ExampleProject.Injectables;
using MemberPeekerCommon;

namespace ExampleProject;

[CanPeek]
public class ExampleClass
{
    readonly AbstractExample1 _injectableClass;
    private readonly InterfaceExample1 _injectable;
    private readonly InterfaceExample2 _injectable2;

    public ExampleClass(AbstractExample1 injectableClass, InterfaceExample1 injectable, InterfaceExample2 injectable2)
    {
        _injectableClass = injectableClass;
        _injectable = injectable;
        _injectable2 = injectable2;
    }

    private int HelloPrivate()
    {
        var list = new List<int>(); 
        var structNested = new AmASneakyStruct();
        var classNested = new IAmASneakyClass();
        var enumNested = IAmASneakyEnum.Sneaky;
        var returnValueForTestingPurposes = 13;
        return returnValueForTestingPurposes;
    }
    private struct AmASneakyStruct
    {
        public bool SneakyBool { get; set; }
    }

    private class IAmASneakyClass
    {
        public bool SneakyBool { get; set; }
    }

    private enum IAmASneakyEnum
    {
        Sneaky
    }
}
