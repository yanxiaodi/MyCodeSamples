using CodeGeneratorDemo.ReflectionDemo;
using System;
using System.Reflection;
using CodeGeneratorDemo.T4TemplateDemo.DesignTimeTextTemplateDemo;

namespace CodeGeneratorDemo.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ReflectionSample();
            RunTimeT4TemplateSample();
            DesignTimeT4TemplateSample();
        }

        private static void ReflectionSample()
        {
            Console.WriteLine("Here is the Reflection sample:");
            // Find all the speakers in the current domain
            var availableSpeakers = ReflectionHelper.GetAvailableSpeakers();
            foreach (var availableSpeaker in availableSpeakers)
            {
                // Create the instance of the type
                var speaker = Activator.CreateInstance(availableSpeaker);
                // Get the property info of the given property name
                PropertyInfo namePropertyInfo = availableSpeaker.GetProperty("Name");
                // Then you can get the value of the property
                var name = namePropertyInfo?.GetValue(speaker)?.ToString();
                Console.WriteLine($"I am {name}");
                // Invoke the method of the instance
                Console.WriteLine(availableSpeaker.InvokeMember("SayHello", BindingFlags.InvokeMethod, null, speaker, null));
            }

            Console.WriteLine();
        }

        private static void RunTimeT4TemplateSample()
        {
            Console.WriteLine("Here is the Run-Time T4 template sample:");
            var page = new RunTimeTextTemplateDemo();
            Console.WriteLine(page.TransformText());
            Console.WriteLine();
        }

        private static void DesignTimeT4TemplateSample()
        {
            Console.WriteLine("Here is the Design-Time T4 template sample:");
            var service = new ProductService();
            var product = service.GetProduct(Guid.NewGuid()).GetAwaiter().GetResult();
            Console.WriteLine($"{product.GetType()}: {product.Id}");
            Console.WriteLine();
        }
    }
}
