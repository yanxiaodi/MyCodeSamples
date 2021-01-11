namespace CodeGeneratorDemo.Core
{
    public class EnglishSpeaker : Speaker, ISayHello
    {
        public string SayHello()
        {
            return "Hello!";
        }
    }
}
