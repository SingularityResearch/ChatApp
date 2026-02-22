using Microsoft.FluentUI.AspNetCore.Components;

namespace ChatApp.Services;

public static class EmojiConstants
{
    public static readonly Dictionary<string, Emoji> EmojiMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ":grinning:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.GrinningFace() },
        { ":)", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.GrinningFace() },
        { ":smile:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.SmilingFaceWithSmilingEyes() },
        { ":+1:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.PeopleBody.Color.Default.ThumbsUp() },
        { ":thumbsup:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.PeopleBody.Color.Default.ThumbsUp() },
        { "<3", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.RedHeart() },
        { ":heart:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.RedHeart() },
        { ":joy:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.FaceWithTearsOfJoy() },
        { ":rofl:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.RollingOnTheFloorLaughing() },
        { ":sad:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.FrowningFace() },
        { ":(", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.FrowningFace() },
        { ":cry:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.CryingFace() },
        { ":sob:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.LoudlyCryingFace() },
        { ":angry:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.AngryFace() },
        { ">(", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.AngryFace() },
        { ":wink:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.WinkingFace() },
        { ";)", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.WinkingFace() },
        { ":sunglasses:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.SmilingFaceWithSunglasses() },
        { ":fire:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.TravelPlaces.Color.Default.Fire() },
        { ":star:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.TravelPlaces.Color.Default.Star() },
        { ":rocket:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.TravelPlaces.Color.Default.Rocket() },
        { ":check:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.Symbols.Color.Default.CheckMarkButton() },
        { ":x:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.Symbols.Color.Default.CrossMark() },
        { ":o:", new Microsoft.FluentUI.AspNetCore.Components.Emojis.SmileysEmotion.Color.Default.FaceWithOpenMouth() }
    };
}
