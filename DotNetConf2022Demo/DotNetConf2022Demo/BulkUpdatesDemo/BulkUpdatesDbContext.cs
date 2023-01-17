using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace BulkUpdatesDemo;

public class BulkUpdatesDbContext : DbContext
{
    public bool LoggingEnabled { get; set; }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Author> Authors => Set<Author>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => (optionsBuilder.UseSqlServer(@$"Server=(localdb)\mssqllocaldb;Database={GetType().Name}"))
            .EnableSensitiveDataLogging()
            .LogTo(
                s =>
                {
                    if (LoggingEnabled)
                    {
                        Console.WriteLine(s);
                    }
                }, LogLevel.Information);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Author>().ToTable("Authors");
        modelBuilder.Entity<Author>().OwnsOne(
            author => author.Contact, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
                ownedNavigationBuilder.OwnsOne(contactDetails => contactDetails.Address);
            });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<List<string>>().HaveConversion<StringListConverter>();

        base.ConfigureConventions(configurationBuilder);
    }

    private class StringListConverter : ValueConverter<List<string>, string>
    {
        public StringListConverter()
            : base(v => string.Join(", ", v!), v => v.Split(',', StringSplitOptions.TrimEntries).ToList())
        {
        }
    }
}

