﻿namespace CodeCampWellington2024;
internal sealed class OpenAiOptions
{
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

internal sealed class BingSearchOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}