namespace CodeGeneratorDemo.ReflectionDemo.Core
{
    public class ChineseSpeaker : ISpeaker
    {
        public string Name => this.GetType().ToString();

        public string SayHello()
        {
            return "Nihao";
        }
    }
}
