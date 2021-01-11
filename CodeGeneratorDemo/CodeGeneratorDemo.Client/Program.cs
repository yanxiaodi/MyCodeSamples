using CodeGeneratorDemo.ReflectionDemo;
using System;
using System.Reflection;

namespace CodeGeneratorDemo.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var availableSpeakers = ReflectionHelper.AvailableSpeakers();
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
        }
    }
}
