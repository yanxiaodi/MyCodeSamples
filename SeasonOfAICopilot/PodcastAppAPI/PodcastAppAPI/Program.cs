using PodcastAppAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddUserSecrets<Program>();



var openAiOptions = builder.Configuration.GetSection(nameof(OpenAiOptions)).Get<OpenAiOptions>();
if (openAiOptions is null)
{
    throw new InvalidOperationException("OpenAI options are not configured.");
}
var bingSearchOptions = builder.Configuration.GetSection(nameof(BingSearchOptions)).Get<BingSearchOptions>();
if (bingSearchOptions is null)
{
    throw new InvalidOperationException("Bing Search options are not configured.");
}

// Register the PodcastCopilot service
builder.Services.AddSingleton<IPodcastCopilot, PodcastCopilot>(_ => new PodcastCopilot(openAiOptions.Endpoint, openAiOptions.Key, bingSearchOptions.Endpoint, bingSearchOptions.Key));

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/GenerateSocialMediaPost/{podcastUrl}", (string podcastUrl) =>
    {
        using var scope = app.Services.CreateScope();
        var podcastCopilot = scope.ServiceProvider.GetRequiredService<IPodcastCopilot>();
        return podcastCopilot.GenerateSocialMediaPost(podcastUrl);
    })
    .WithName("GetSocialMediaPost")
    .WithSummary("Generate Social Media Post")
    .WithDescription("Generates a blurb / social media post with an image for a podcast episode based on the podcast url provided.")
    .WithOpenApi();

app.Run();