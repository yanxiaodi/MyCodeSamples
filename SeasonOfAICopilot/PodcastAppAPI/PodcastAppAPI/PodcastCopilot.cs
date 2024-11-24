using Azure.AI.OpenAI;
using Newtonsoft.Json.Linq;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;
using System.Web;

namespace PodcastAppAPI;

public class PodcastCopilot : IPodcastCopilot
{
    private readonly AzureOpenAIClient _azureOpenAiClient;
    private readonly string _bingSearchKey;
    private readonly string _bingSearchEndpoint;

    public PodcastCopilot(string openAiEndpoint, string openAiKey, string bingSearchEndpoint, string bingSearchKey)
    {
        if (!Uri.IsWellFormedUriString(openAiEndpoint, UriKind.Absolute))
        {
            throw new ArgumentException("Invalid URI for OpenAI endpoint", nameof(openAiEndpoint));
        }

        if (!Uri.IsWellFormedUriString(bingSearchEndpoint, UriKind.Absolute))
        {
            throw new ArgumentException("Invalid URI for Bing Search endpoint", nameof(bingSearchEndpoint));
        }

        _azureOpenAiClient = new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(openAiKey));
        _bingSearchKey = bingSearchKey;
        _bingSearchEndpoint = bingSearchEndpoint;
    }


    /// <summary>
    /// Get the transcription of a podcast
    /// </summary>
    /// <param name="podcastUrl"></param>
    /// <returns></returns>
    private async Task<string> GetTranscription(string podcastUrl)
    {
        var decodedUrl = HttpUtility.UrlDecode(podcastUrl);
        // DO NOT use HttpClient in this way in production code. This is for demonstration purposes only.
        HttpClient httpClient = new HttpClient();
        Stream audioStreamFromBlob = await httpClient.GetStreamAsync(decodedUrl);

        AudioClient client = _azureOpenAiClient.GetAudioClient("whisper");
        AudioTranscription audioTranscription = await client.TranscribeAudioAsync(audioStreamFromBlob, "file.mp3");

        return audioTranscription.Text;
    }

    /// <summary>
    /// Get the guest name from a podcast transcription
    /// </summary>
    /// <param name="transcription"></param>
    /// <returns></returns>
    private async Task<string> GetGuestName(string transcription)
    {
        ChatClient client = _azureOpenAiClient.GetChatClient("gpt-4");

        ChatCompletion chatCompletion = await client.CompleteChatAsync(
        [
            new SystemChatMessage("Extract only the guest name on the Beyond the Tech podcast from the following transcript. Beyond the Tech is hosted by Kevin Scott, so Kevin Scott will never be the guest."),
            new UserChatMessage(transcription)
        ]);

        return chatCompletion.Content.First().Text;
    }

    /// <summary>
    /// Get the bio of a guest
    /// </summary>
    /// <param name="guestName"></param>
    /// <returns></returns>
    private async Task<string> GetGuestBio(string guestName)
    {
        // HttpClient should be injected via DI. Do not use the following code in your projects.
        var client = new HttpClient();

        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _bingSearchKey);

        HttpResponseMessage response = await client.GetAsync($"{_bingSearchEndpoint}/v7.0/search?q={guestName}");

        string responseBody = await response.Content.ReadAsStringAsync();

        // Parse responseBody as JSON and extract the bio.
        JObject searchResults = JObject.Parse(responseBody);
        var bio = searchResults["webPages"]["value"][0]["snippet"].ToString();

        return bio;
    }

    /// <summary>
    /// Get a social media blurb for a podcast episode
    /// </summary>
    /// <param name="transcription"></param>
    /// <param name="bio"></param>
    /// <returns></returns>
    private async Task<string> GetSocialMediaBlurb(string transcription, string bio)
    {
        ChatClient client = _azureOpenAiClient.GetChatClient("gpt-4");

        ChatCompletion chatCompletion = await client.CompleteChatAsync(
        [
            new SystemChatMessage("You are a helpful large language model that can create a LinkedIn promo blurb for episodes of the podcast Behind the Tech, when given transcripts of the podcasts. The Behind the Tech podcast is hosted by Kevin Scott."),
            new UserChatMessage("Create a short summary of this podcast episode that would be appropriate to post on LinkedIn to promote the podcast episode. The post should be from the first-person perspective of Kevin Scott, who hosts the podcast. \n" +
                                $"Here is the transcript of the podcast episode: {transcription} \n" +
                                $"Here is the bio of the guest: {bio}")
        ]);

        return chatCompletion.Content.First().Text;
    }

    /// <summary>
    /// Get a DALL-E prompt for a social media blurb
    /// </summary>
    /// <param name="socialBlurb"></param>
    /// <returns></returns>
    private async Task<string> GetDallEPrompt(string socialBlurb)
    {
        ChatClient client = _azureOpenAiClient.GetChatClient("gpt-4");

        ChatCompletion chatCompletion = await client.CompleteChatAsync(
        [
            new SystemChatMessage("You are a helpful large language model that generates DALL-E prompts, that when given to the DALL-E model can generate beautiful high-quality images to use in social media posts about a podcast on technology. Good DALL-E prompts will contain mention of related objects, and will not contain people, faces, or words. Good DALL-E prompts should include a reference to podcasting along with items from the domain of the podcast guest."),
            new UserChatMessage($"Create a DALL-E prompt to create an image to post along with this social media text: {socialBlurb}")

        ]);

        return chatCompletion.Content.First().Text;
    }

    private async Task<string> GetImage(string prompt)
    {
        ImageClient client = _azureOpenAiClient.GetImageClient("dall-e-3");

        ImageGenerationOptions options = new()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
            Style = GeneratedImageStyle.Vivid,
            ResponseFormat = GeneratedImageFormat.Uri,
        };

        GeneratedImage image = await client.GenerateImageAsync(prompt + ", high-quality digital art", options);

        return image.ImageUri.ToString();
    }

    public async Task<SocialMediaPost> GenerateSocialMediaPost(string podcastUrl)
    {
        var transcription = await GetTranscription(podcastUrl);
        var guestName = await GetGuestName(transcription);
        var guestBio = await GetGuestBio(guestName);
        var generatedBlurb = await GetSocialMediaBlurb(transcription, guestBio);
        var dallePrompt = await GetDallEPrompt(generatedBlurb);
        var generatedImage = await GetImage(dallePrompt);

        var socialMediaPost = new SocialMediaPost()
        {
            ImageUrl = generatedImage,
            Blurb = generatedBlurb
        };

        return socialMediaPost;
    }
}
