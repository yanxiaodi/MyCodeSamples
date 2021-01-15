namespace CodeGeneratorDemo.ReflectionDemo.Core
{
    public class EnglishSpeaker : ISpeaker
    {
        public string Name => this.GetType().ToString();

        public string SayHello()
        {
            return "Hello!";
        }
    }
}
