using System;
using CodeGeneratorDemo.SourceGeneratorDemo;

namespace CodeGeneratorDemo.Client.Core
{
    [AutoRegister]
    public class OrderService : IOrderService
    {
        public OrderService()
        {
            Console.WriteLine($"{this.GetType()} constructed.");
        }
    }
}
