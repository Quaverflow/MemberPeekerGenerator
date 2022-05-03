using ExampleProject;
using ExampleProject.Injectables;
using Moq;
using Xunit;

namespace MemberPeekerGenerator.Tests
{
    public class BasicTest
    {
        [Fact]
        public void Test1()
        {
            var injectableMock = new Mock<InterfaceExample1>(MockBehavior.Strict);
            var injectable2Mock = new Mock<InterfaceExample2>(MockBehavior.Strict);
            var injectableClass = new Mock<AbstractExample1>().Object;

            var exposedClassInstance = new MemberPeeker.ExampleClass_Exposed<ExampleClass>(injectableClass, injectableMock.Object, injectable2Mock.Object);

            var result = exposedClassInstance.HelloPrivate();
            Assert.Equal(13, result);
        }
    }
}