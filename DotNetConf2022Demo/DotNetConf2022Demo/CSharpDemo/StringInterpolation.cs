namespace CSharpDemo
{
    public class StringInterpolation
    {
        public static void ContainNewLine()
        {
            // The interpolated expressions can now include newlines.
            // This feature makes it easier to read string interpolations that use longer C# expressions,
            // like pattern matching switch expressions, or LINQ queries.
            var city = "Wellington";
            var message =
                $"{city} is a great city. {city switch
                {
                    "Wellington" => "It's the capital of New Zealand",
                    "Auckland" => "It's the largest city in New Zealand",
                    "Christchurch" => "It's the most largest city in the South Island",
                    _ => "But I don't know anything about that city"
                }}";

            Console.WriteLine(message);
        }

        public static void RawStringLiteral()
        {
            var x = 2;
            var y = 3;
            // Raw string literals can contain arbitrary text, including whitespace,
            // new lines, embedded quotes, and other special characters
            // without requiring escape sequences. 
            var pointMessage = $"""The point "{x}, {y}" is {Math.Sqrt(x * x + y * y)} from the origin""";
            Console.WriteLine(pointMessage);
        }

        public static void RawStringLiteralMultipleDenotes()
        {
            var x = 2;
            var y = 3;
            // Multiple $ characters denote how many
            // consecutive braces start and end the interpolation:
            var pointMessage = $"""The point {x}, {y} is {Math.Sqrt(x * x + y * y)} from the origin""";
            // The preceding example specifies that two braces start and end an interpolation.
            // The third repeated opening and closing brace are included in the output string.
            // Note: The compiler issues an error if the sequence of brace characters is equal to or greater
            // than double the length of the sequence of $ characters.
            // Question: What if we want to output {{2, 3}}?
            Console.WriteLine(pointMessage);
        }

        public static void RawStringLiteralMultipleLines()
        {
            // The raw string literal syntax is now available. It can contain any characters, including
            // newlines, embedded quotes, and other special characters without escaping.
            var longMessage = """
                This is a long message.
                It has several lines.
                    Some are indented
                            more than others.
                Some should start at the first column.
                Some have "quoted text" in them. 
                """;
            // Any whitespace to the left of the closing double quotes
            // will be removed from the string literal.
            Console.WriteLine(longMessage);

            var jsonString = """ 
                {
                    "name": "John Smith",
                    "age": 35,
                    "address": {
                        "street": "123 Main St",
                        "city": "Anytown",
                        "state": "CA",
                        "zip": "12345"
                    },
                    "phoneNumbers": [
                        {
                            "type": "home",
                            "number": "555-555-5555"
                        },
                        {
                            "type": "work",
                            "number": "555-555-5556"
                        }
                    ],
                    "email": "john.smith@example.com"
                } 
                """;
            Console.WriteLine(jsonString);
        }
    }
}
