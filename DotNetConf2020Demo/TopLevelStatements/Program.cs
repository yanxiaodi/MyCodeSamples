//using System;

//namespace TopLevelStatements
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Hello World!");
//        }
//    }
//}

//using static System.Console;
System.Console.WriteLine("Hello World!");

var person = new Person { FirstName = "Jack" };
System.Console.WriteLine(person.FirstName);
class Person
{
    public string FirstName { get; set; }
}