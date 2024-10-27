namespace PodcastAppAPI;

public interface IPodcastCopilot
{
    Task<SocialMediaPost> GenerateSocialMediaPost(string podcastUrl);
}
