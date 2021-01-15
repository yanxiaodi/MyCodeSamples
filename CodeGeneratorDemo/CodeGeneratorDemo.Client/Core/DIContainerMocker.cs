using System;

namespace CodeGeneratorDemo.SourceGeneratorDemo.Core
{
    public static class DiContainerMocker
    {
        public static void RegisterService<T>(T service)
        {
            Console.WriteLine($"{service.GetType()} has been registered.");
        }
    }
}
