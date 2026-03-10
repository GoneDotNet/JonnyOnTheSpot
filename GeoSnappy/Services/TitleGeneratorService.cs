using Microsoft.Extensions.AI;

namespace GeoSnappy.Services;

public class TitleGeneratorService
{
    private readonly IChatClient? _chatClient;

    public TitleGeneratorService(IChatClient? chatClient = null)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GenerateFunnyTitleAsync(double latitude, double longitude, string? photoFilePath = null)
    {
        if (_chatClient is null)
            return GetFallbackTitle();

        try
        {
            // Try multimodal (image + text) first, fall back to text-only
            if (photoFilePath is not null && File.Exists(photoFilePath))
            {
                try
                {
                    return await GenerateWithImageAsync(photoFilePath, latitude, longitude);
                }
                catch (ArgumentException)
                {
                    // Image content not supported by this IChatClient — fall through to text-only
                }
            }

            return await GenerateTextOnlyAsync(latitude, longitude);
        }
        catch
        {
            return GetFallbackTitle();
        }
    }

    private async Task<string> GenerateWithImageAsync(string photoFilePath, double latitude, double longitude)
    {
        var imageBytes = await File.ReadAllBytesAsync(photoFilePath);
        var imageContent = new DataContent(imageBytes, "image/jpeg");

        var message = new ChatMessage(ChatRole.User,
        [
            new TextContent($"""
                Look at this photo taken at coordinates ({latitude:F4}, {longitude:F4}).
                Generate a short, funny, creative caption that describes what you see in the photo.
                Keep it under 10 words. Be witty, playful, and use a pun or wordplay if possible.
                Return only the caption text, nothing else.
                """),
            imageContent
        ]);

        var response = await _chatClient!.GetResponseAsync([message]);
        var title = response.Text?.Trim();
        return string.IsNullOrWhiteSpace(title) ? GetFallbackTitle() : title;
    }

    private async Task<string> GenerateTextOnlyAsync(double latitude, double longitude)
    {
        var prompt = $"""
            Generate a short, funny, creative caption for a photo taken at coordinates 
            ({latitude:F4}, {longitude:F4}). Keep it under 10 words. Be witty, playful, 
            and use a pun or wordplay if possible. Return only the caption text, nothing else.
            """;

        var response = await _chatClient!.GetResponseAsync(prompt);
        var title = response.Text?.Trim();
        return string.IsNullOrWhiteSpace(title) ? GetFallbackTitle() : title;
    }

    private static string GetFallbackTitle()
    {
        var titles = new[]
        {
            "📸 Caught in the wild!",
            "🌍 I was here, deal with it",
            "🗺️ Lost but photogenic",
            "📍 Pin-worthy moment",
            "🎯 Nailed the shot!",
            "🌅 Another day, another snap",
            "🏞️ Scenic and I know it",
            "📷 Say cheese... or don't",
            "🗻 Peak photography right here",
            "🎨 Art happens everywhere",
            "🌈 Somewhere over the GPS",
            "🚀 To infinity and this spot",
        };

        return titles[Random.Shared.Next(titles.Length)];
    }
}
