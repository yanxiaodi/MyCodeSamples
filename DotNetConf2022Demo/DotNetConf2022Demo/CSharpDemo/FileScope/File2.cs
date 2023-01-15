namespace CSharpDemo.FileScope
{
    // In File2.cs:
    // Doesn't conflict with HiddenWidget
    // declared in File1.cs
    public class HiddenWidget
    {
        public void RunTask()
        {
            Console.WriteLine("Hello from HiddenWidget in File2.cs");
        }
    }
}
