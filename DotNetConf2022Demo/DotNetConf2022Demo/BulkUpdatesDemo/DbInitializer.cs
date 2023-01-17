namespace BulkUpdatesDemo;

public static class DbInitializer
{
    public static async Task InitializeAsync(BulkUpdatesDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (context.Blogs.Any())
        {
            return;
        }

        var tagEntityFramework = new Tag("TagEF", "Entity Framework");
        var tagDotNet = new Tag("TagNet", ".NET");
        var tagDotNetMaui = new Tag("TagMaui", ".NET MAUI");
        var tagAspDotNet = new Tag("TagAsp", "ASP.NET");
        var tagAspDotNetCore = new Tag("TagAspC", "ASP.NET Core");
        var tagDotNetCore = new Tag("TagC", ".NET Core");
        var tagHacking = new Tag("TagHx", "Hacking");
        var tagLinux = new Tag("TagLin", "Linux");
        var tagSqlite = new Tag("TagLite", "SQLite");
        var tagVisualStudio = new Tag("TagVS", "Visual Studio");
        var tagGraphQl = new Tag("TagQL", "GraphQL");
        var tagCosmosDb = new Tag("TagCos", "CosmosDB");
        var tagBlazor = new Tag("TagBl", "Blazor");

        var maddy = new Author("Maddy Montaquila")
        {
            Contact = new() { Address = new("1 Main St", "Camberwick Green", "CW1 5ZH", "UK"), Phone = "01632 12345" }
        };
        var jeremy = new Author("Jeremy Likness")
        {
            Contact = new() { Address = new("2 Main St", "Chigley", "CW1 5ZH", "UK"), Phone = "01632 12346" }
        };
        var dan = new Author("Daniel Roth")
        {
            Contact = new() { Address = new("3 Main St", "Camberwick Green", "CW1 5ZH", "UK"), Phone = "01632 12347" }
        };
        var arthur = new Author("Arthur Vickers")
        {
            Contact = new() { Address = new("15a Main St", "Chigley", "CW1 5ZH", "UK"), Phone = "01632 12348" }
        };
        var brice = new Author("Brice Lambson")
        {
            Contact = new() { Address = new("4 Main St", "Chigley", "CW1 5ZH", "UK"), Phone = "01632 12349" }
        };

        var blogs = new List<Blog>
        {
            new(".NET Blog")
            {
                Posts =
                {
                    new Post(
                        "Productivity comes to .NET MAUI in Visual Studio 2022",
                        "Visual Studio 2022 17.3 is now available and...",
                        new DateTime(2022, 8, 9)) { Tags = { tagDotNetMaui, tagDotNet }, Author = maddy },
                    new Post(
                        "Announcing .NET 7 Preview 7", ".NET 7 Preview 7 is now available with improvements to System.LINQ, Unix...",
                        new DateTime(2022, 8, 9)) { Tags = { tagDotNet }, Author = jeremy },
                    new Post(
                        "ASP.NET Core updates in .NET 7 Preview 7", ".NET 7 Preview 7 is now available! Check out what's new in...",
                        new DateTime(2022, 8, 9))
                    {
                        Tags = { tagDotNet, tagAspDotNet, tagAspDotNetCore }, Author = dan
                    },
                    new Post(
                        "Announcing Entity Framework 7 Preview 7: Interceptors!",
                        "Announcing EF7 Preview 7 with new and improved interceptors, and...",
                        new DateTime(2022, 8, 9))
                    {
                        Tags = { tagEntityFramework, tagDotNet, tagDotNetCore }, Author = arthur
                    }
                },
            },
            new("1unicorn2")
            {
                Posts =
                {
                    new Post(
                        "Hacking my Sixth Form College network in 1991",
                        "Back in 1991 I was a student at Franklin Sixth Form College...",
                        new DateTime(2020, 4, 10)) { Tags = { tagHacking }, Author = arthur },
                    new Post(
                        "All your versions are belong to us",
                        "Totally made up conversations about choosing Entity Framework version numbers...",
                        new DateTime(2020, 3, 26)) { Tags = { tagEntityFramework }, Author = arthur },
                    new Post(
                        "Moving to Linux", "A few weeks ago, I decided to move from Windows to Linux as...",
                        new DateTime(2020, 3, 7)) { Tags = { tagLinux }, Author = arthur },
                    new Post(
                        "Welcome to One Unicorn 2.0!", "I created my first blog back in 2011..",
                        new DateTime(2020, 2, 29)) { Tags = { tagEntityFramework }, Author = arthur }
                }
            },
            new("Brice's Blog")
            {
                Posts =
                {
                    new Post(
                        "SQLite in Visual Studio 2022", "A couple of years ago, I was thinking of ways...",
                        new DateTime(2022, 7, 26))
                    {
                        Tags = { tagSqlite, tagVisualStudio }, Author = brice
                    },
                    new Post(
                        "On .NET - Entity Framework Migrations Explained",
                        "This week, @JamesMontemagno invited me onto the On .NET show...",
                        new DateTime(2022, 5, 4))
                    {
                        Tags = { tagEntityFramework, tagDotNet }, Author = brice
                    },
                    new Post(
                        "Dear DBA: A silly idea", "We have fun on the Entity Framework team...",
                        new DateTime(2022, 3, 31)) { Tags = { tagEntityFramework }, Author = brice },
                    new Post(
                        "Microsoft.Data.Sqlite 6", "It’s that time of year again. Microsoft.Data.Sqlite version...",
                        new DateTime(2021, 11, 8)) { Tags = { tagSqlite, tagDotNet }, Author = brice }
                }
            },
            new("Developer for Life")
            {
                Posts =
                {
                    new Post(
                        "GraphQL for .NET Developers", "A comprehensive overview of GraphQL as...",
                        new DateTime(2021, 7, 1))
                    {
                        Tags = { tagDotNet, tagGraphQl, tagAspDotNetCore }, Author = jeremy
                    },
                    new Post(
                        "Azure Cosmos DB With EF Core on Blazor Server",
                        "Learn how to build Azure Cosmos DB apps using Entity Framework Core...",
                        new DateTime(2021, 5, 16))
                    {
                        Tags =
                        {
                            tagDotNet,
                            tagEntityFramework,
                            tagAspDotNetCore,
                            tagCosmosDb,
                            tagBlazor
                        },
                        Author = jeremy
                    },
                    new Post(
                        "Multi-tenancy with EF Core in Blazor Server Apps",
                        "Learn several ways to implement multi-tenant databases in Blazor Server apps...",
                        new DateTime(2021, 4, 29))
                    {
                        Tags = { tagDotNet, tagEntityFramework, tagAspDotNetCore, tagBlazor },
                        Author = jeremy
                    },
                    new Post(
                        "An Easier Blazor Debounce", "Where I propose a simple method to debounce input without...",
                        new DateTime(2021, 4, 12))
                    {
                        Tags = { tagDotNet, tagAspDotNetCore, tagBlazor }, Author = jeremy
                    }
                }
            }
        };

        await context.AddRangeAsync(blogs);
        await context.SaveChangesAsync();

        await context.SaveChangesAsync();
    }
}
