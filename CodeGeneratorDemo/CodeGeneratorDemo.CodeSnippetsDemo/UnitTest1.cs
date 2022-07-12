
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace CodeGeneratorDemo.CodeSnippetsDemo
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }

        [Theory]
        [InlineData(1)]
        public void Test2(int value)
        {

        }
    }
}