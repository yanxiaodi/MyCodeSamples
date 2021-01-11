namespace CodeGeneratorDemo.Core
{
    public class ChineseSpeaker : Speaker, ISayHello
    {
        public string SayHello()
        {
            return "Nihao";
        }
    }
}
