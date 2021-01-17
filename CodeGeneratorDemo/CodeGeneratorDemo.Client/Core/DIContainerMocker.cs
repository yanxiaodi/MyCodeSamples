using System;

namespace CodeGeneratorDemo.Client.Core
{
    public static class DiContainerMocker
    {
        public static void RegisterService<TInterface, TImplementation>(TImplementation service)
        {
            Console.WriteLine($"{service.GetType()} has been registered for {typeof(TInterface)}.");
        }
    }
}
