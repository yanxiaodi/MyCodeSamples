using System;
using CodeGeneratorDemo.SourceGeneratorDemo;

namespace CodeGeneratorDemo.SourceGeneratorDemo.Core
{
    [AutoRegister]
    public class ProductService
    {
        public ProductService()
        {
            Console.WriteLine($"{this.GetType()} constructed.");
        }
    }
}
