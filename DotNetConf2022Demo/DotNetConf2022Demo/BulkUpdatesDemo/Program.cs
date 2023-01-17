// See https://aka.ms/new-console-template for more information

using BulkUpdatesDemo;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Hello, World!");

// Initialize the database
await using var dbContext = new BulkUpdatesDbContext();
dbContext.LoggingEnabled = false;
await DbInitializer.InitializeAsync(dbContext);

// Query all the posts
var posts = await dbContext.Posts.ToListAsync();
foreach (var post in posts)
{
    Console.WriteLine($"Post: {post.Title}");
}

//#region Old ways

//// Update the titles to all the posts
//// Previously, we need to query all the posts then update them one by one
//Console.WriteLine("Updating the titles...");
//dbContext.LoggingEnabled = true;
//foreach (var post in posts)
//{
//    post.Title += " *Featured!*";
//}
//await dbContext.SaveChangesAsync();
//dbContext.LoggingEnabled = false;

//// Query all the posts again
//posts = await dbContext.Posts.ToListAsync();
//foreach (var post in posts)
//{
//    Console.WriteLine($"Post: {post.Title}");
//}

//// Delete all the posts
//// Previously, we need to query all the posts then delete them one by one
//Console.WriteLine("Deleting all the posts...");
//dbContext.LoggingEnabled = true;
//foreach (var post in posts)
//{
//    dbContext.Remove(post);
//}
//await dbContext.SaveChangesAsync();
//dbContext.LoggingEnabled = false;

//#endregion


// We can use bulk update
Console.WriteLine("Bulk updating the titles...");
dbContext.LoggingEnabled = true;
await dbContext.Posts.ExecuteUpdateAsync(x => x.SetProperty(y => y.Title, y => y.Title + " *Featured!*"));
dbContext.LoggingEnabled = false;

// Query all the posts again
//posts.ForEach(x => dbContext.Entry(x).State = EntityState.Detached);
posts = await dbContext.Posts.ToListAsync();
foreach (var post in posts)
{
    Console.WriteLine($"Post: {post.Title}");
}

// We can use bulk delete
Console.WriteLine("Bulk deleting all the posts...");
dbContext.LoggingEnabled = true;
await dbContext.Posts.ExecuteDeleteAsync();
// No need to call SaveChangesAsync() because ExecuteDeleteAsync() immediately executes the SQL statement
dbContext.LoggingEnabled = false;
