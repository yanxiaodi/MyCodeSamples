using System;

namespace CodeGeneratorDemo.ReflectionDemo.Core
{
    public class EnglishSpeaker : ISpeaker
    {
        public string Name => this.GetType().ToString();

        public void SayHello()
        {
            Console.WriteLine("Hello!");
        }
    }
}
