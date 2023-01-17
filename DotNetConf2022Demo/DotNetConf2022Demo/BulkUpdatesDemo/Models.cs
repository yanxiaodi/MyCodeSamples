namespace BulkUpdatesDemo;

public class Blog
{
    public Blog(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }
    public string Name { get; set; }
    public List<Post> Posts { get; } = new();
}

public class Post
{
    public Post(string title, string content, DateTime publishedOn)
    {
        Title = title;
        Content = content;
        PublishedOn = publishedOn;
    }

    public int Id { get; private set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishedOn { get; set; }
    public Blog Blog { get; set; } = null!;
    public List<Tag> Tags { get; } = new();
    public Author? Author { get; set; }
}

public class Tag
{
    public Tag(string id, string text)
    {
        Id = id;
        Text = text;
    }

    public string Id { get; private set; }
    public string Text { get; set; }
    public List<Post> Posts { get; } = new();
}

public class Author
{
    public Author(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }
    public string Name { get; set; }
    public ContactDetails Contact { get; set; } = null!;
    public List<Post> Posts { get; } = new();
}

#region ContactDetailsAggregate
public class ContactDetails
{
    public Address Address { get; set; } = null!;
    public string? Phone { get; set; }
}

public class Address
{
    public Address(string street, string city, string postcode, string country)
    {
        Street = street;
        City = city;
        Postcode = postcode;
        Country = country;
    }

    public string Street { get; set; }
    public string City { get; set; }
    public string Postcode { get; set; }
    public string Country { get; set; }
}
#endregion