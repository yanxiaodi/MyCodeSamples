namespace CSharpDemo.FileScope
{
    // In File1.cs:
    file interface IWidget
    {
        void ProvideAnswer();
    }

    file class HiddenWidget
    {
        public int Work() => 42;
    }

    public class Widget : IWidget
    {
        public void ProvideAnswer()
        {
            var worker = new HiddenWidget();
            var result = worker.Work();
            Console.WriteLine($"The answer is {result} from Widget in File1.cs");
        }
    }
}
