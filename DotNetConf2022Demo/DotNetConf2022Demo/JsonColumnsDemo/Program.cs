// See https://aka.ms/new-console-template for more information

using JsonColumnsDemo;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Hello, World!");

await using var dbContext = new JsonColumnsDbContext();
// Initialize the database
await DbInitializer.InitializeAsync(dbContext);

// Query
var authorsInLondon = await dbContext.Authors.Where(x => x.Contact.Address.City == "London").ToListAsync();
Console.WriteLine($"There are {authorsInLondon.Count} authors in London");
// Output all the authors in London
foreach (var author in authorsInLondon)
{
    Console.WriteLine($"{author.Name} lives at {author.Contact.Address.Street}, {author.Contact.Address.City}, {author.Contact.Address.Postcode}, {author.Contact.Address.Country}");
}
