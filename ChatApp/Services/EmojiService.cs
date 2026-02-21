using Microsoft.FluentUI.AspNetCore.Components;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ChatApp.Services;

public interface IEmojiService
{
    List<Emoji> AllEmojis { get; }
    Emoji? GetEmoji(string shortcode);
    string GetUnicode(string emojiName);
    string GetShortcode(Emoji emoji);
}

public class EmojiService : IEmojiService
{
    private readonly List<Emoji> _allEmojis = new();
    private readonly Dictionary<string, Emoji> _emojiMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _unicodeMap = new(StringComparer.OrdinalIgnoreCase);

    public List<Emoji> AllEmojis => _allEmojis;

    public EmojiService(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        LoadUnicodeMap(env.WebRootPath);
        LoadAllEmojis();
    }

    private void LoadUnicodeMap(string webRootPath)
    {
        try
        {
            string jsonPath = Path.Combine(webRootPath, "emoji_data.json");
            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("emoji", out var emojiProp) &&
                        element.TryGetProperty("aliases", out var aliasesProp))
                    {
                        string unicode = emojiProp.GetString() ?? "";
                        foreach (var alias in aliasesProp.EnumerateArray())
                        {
                            string aliasStr = alias.GetString() ?? "";
                            if (!string.IsNullOrEmpty(aliasStr))
                            {
                                // Store as grinning_face
                                _unicodeMap.TryAdd(aliasStr.Replace("-", "_"), unicode);
                            }
                        }

                        // Also try to map the description (e.g. "grinning face" -> "grinning_face")
                        if (element.TryGetProperty("description", out var descProp))
                        {
                            string desc = descProp.GetString()?.Replace(" ", "_").Replace("-", "_") ?? "";
                            if (!string.IsNullOrEmpty(desc))
                            {
                                _unicodeMap.TryAdd(desc, unicode);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load emoji.json: {ex.Message}");
        }
    }

    private void LoadAllEmojis()
    {
        string[] assemblies = {
            "SmileysEmotion", "PeopleBody", "AnimalsNature",
            "FoodDrink", "TravelPlaces", "Activities", "Objects", "Symbols"
        };

        foreach (var assemblyName in assemblies)
        {
            try
            {
                var asm = Assembly.Load("Microsoft.FluentUI.AspNetCore.Components.Emojis." + assemblyName);
                if (asm == null) continue;

                foreach (var type in asm.GetExportedTypes())
                {
                    if (type.FullName != null && type.FullName.Contains("Color.Default") && !type.IsAbstract)
                    {
                        if (Activator.CreateInstance(type) is Emoji emoji)
                        {
                            _allEmojis.Add(emoji);

                            // Example shortcode: :grinning_face: from GrinningFace
                            string shortcode = GenerateShortcode(emoji.Name);
                            _emojiMap.TryAdd(shortcode, emoji);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load emojis from {assemblyName}: {ex.Message}");
            }
        }
    }

    private string GenerateShortcode(string emojiName)
    {
        if (string.IsNullOrEmpty(emojiName)) return "::";

        // Convert "GrinningFace" -> "grinning_face" and replace spaces
        string snakeCase = Regex.Replace(emojiName, "([a-z])([A-Z])", "$1_$2").Replace(" ", "_").ToLower();
        return $":{snakeCase}:";
    }

    public string GetShortcode(Emoji emoji)
    {
        var match = _emojiMap.FirstOrDefault(kvp => kvp.Value.Name == emoji.Name);
        return match.Key ?? $":{emoji.Name.Replace(" ", "_").ToLower()}:";
    }

    public Emoji? GetEmoji(string shortcode)
    {
        if (_emojiMap.TryGetValue(shortcode, out var emoji))
        {
            return emoji;
        }
        return null;
    }

    public string GetUnicode(string emojiName)
    {
        // emojiName comes in as "GrinningFace", convert to "grinning_face"
        string snakeCase = Regex.Replace(emojiName, "([a-z])([A-Z])", "$1_$2").ToLower();

        if (_unicodeMap.TryGetValue(snakeCase, out string? unicode))
        {
            return unicode;
        }
        return ""; // Fallback if no native unicode emoji is found
    }
}
