using System;
using CodeGeneratorDemo.SourceGeneratorDemo;

namespace CodeGeneratorDemo.SourceGeneratorDemo.Core
{
    [AutoRegister]
    public class OrderService 
    {
        public OrderService()
        {
            Console.WriteLine($"{this.GetType()} constructed.");
        }
    }
}
