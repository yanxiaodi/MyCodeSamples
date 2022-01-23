using System;
using System.Runtime.InteropServices;

namespace RecordDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var normalPersonA = new NormalPerson { FirstName = "Scott", LastName = "Hunter" };
            var normalPersonB = new NormalPerson { FirstName = "Scott", LastName = "Hunter" };
            Console.WriteLine("Class sample:");
            Console.WriteLine($"Equals: {Equals(normalPersonA, normalPersonB)}");
            Console.WriteLine($"ReferenceEquals: {ReferenceEquals(normalPersonA, normalPersonB)}");
            Console.WriteLine($"ToString: {normalPersonA}");

            Console.WriteLine();
            var personA = new Person("Scott", "Hunter");
            var personB = new Person("Scott", "Hunter");
            Console.WriteLine("Record sample:");
            Console.WriteLine($"Equals: {Equals(personA, personB)}");
            Console.WriteLine($"ReferenceEquals: {ReferenceEquals(personA, personB)}");
            Console.WriteLine($"ToString: {personA}");

            var personC = personA with { LastName = "Hanselman" };
            Console.WriteLine($"ToString: {personC}");

            Console.WriteLine();
            Console.WriteLine("Init only setters sample:");
            var student = new Student { FirstName = "Scott", LastName = "Hunter" };
            // Error! You can not change its value
            //student.FirstName = "Jack";
            Console.WriteLine(student);

        }
    }

    class NormalPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public override bool Equals(object? obj)
        {
           if (obj == null)
           {
               return false;
           }

           if (obj is NormalPerson target)
           {
               return FirstName == target.FirstName && LastName == target.LastName;
           }
           return false;
        }
    }

    record Person
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }

        public Person(string first, string last) => (FirstName, LastName) = (first, last);
    }

    public class Student
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }

        public override string ToString()
        {
            return $"FirstName: {FirstName}, LastName: {LastName}";
        }
    }

}
