using System;

namespace CodeGeneratorDemo.ReflectionDemo.Core
{
    public class ChineseSpeaker : ISpeaker
    {
        public string Name => this.GetType().ToString();

        public void SayHello()
        {
            Console.WriteLine("Nihao!");
        }
    }
}
