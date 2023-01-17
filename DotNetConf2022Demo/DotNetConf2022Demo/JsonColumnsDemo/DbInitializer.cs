namespace JsonColumnsDemo;

public static class DbInitializer
{
    public static async Task InitializeAsync(JsonColumnsDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (context.Authors.Any())
        {
            return;
        }

        // Create some authors
        var authors = new[]
        {
                new Author
                {
                    Name = "John Doe",
                    Contact = new ContactDetails
                    {
                        Address = new Address("123 Main Street", "London", "SW1 1AA", "UK"),
                        Phone = "01234 567890"
                    }
                },
                new Author
                {
                    Name = "Jane Doe",
                    Contact = new ContactDetails
                    {
                        Address = new Address("456 High Street", "Bristol", "BS1 1AA", "UK"),
                        Phone = "01234 567891"
                    }
                },
                new Author
                {
                    Name = "John Smith",
                    Contact = new ContactDetails
                    {
                        Address = new Address("789 Low Street", "Birmingham", "B1 1AA", "UK"),
                        Phone = "01234 567892"
                    }
                },
                new Author
                {
                    Name = "Jane Smith",
                    Contact = new ContactDetails
                    {
                        Address = new Address("101 High Street", "Manchester", "M1 1AA", "UK"),
                        Phone = "01234 567893"
                    }
                },
                new Author
                {
                    Name = "John Jones",
                    Contact = new ContactDetails
                    {
                        Address = new Address("102 High Street", "London", "M1 1AA", "UK"),
                        Phone = "01234 567894"
                    }
                },
            };
        await context.Authors.AddRangeAsync(authors);
        await context.SaveChangesAsync();
    }
}
