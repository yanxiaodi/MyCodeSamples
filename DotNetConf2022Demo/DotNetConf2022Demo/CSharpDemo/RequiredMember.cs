namespace CSharpDemo
{
    public class RequiredMember
    {
        public static void CreatePerson()
        {
            // Without the required member, we can create an instance using the default constructor:
            var person = new Person();
            //var person = new Person("Jack");
            //var person = new Person() { FirstName = "Jack" };
            Console.WriteLine($"The person's name is {person.FirstName}");
        }
    }

    public class Person
    {
        public Person() { }

        // SetsRequiredMembers attribute Specifies that this constructor sets all required members for the current type
        //[SetsRequiredMembers]
        public Person(string firstName) => FirstName = firstName;

        public string FirstName { get; init; }

        //public required string FirstName { get; init; }

        // Omitted for brevity.
    }
}
