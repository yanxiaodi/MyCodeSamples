using System;
using CodeGeneratorDemo.SourceGeneratorDemo;

namespace CodeGeneratorDemo.Client.Core
{
    [AutoRegister]
    public class ProductService : IProductService
    {
        public ProductService()
        {
            Console.WriteLine($"{this.GetType()} constructed.");
        }
    }
}
