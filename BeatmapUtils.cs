using System.IO;
using System.Text.RegularExpressions;

public static class BeatmapUtils
{
    public class BeatmapMetadata {
        public string Title { get; set; } = "";
        public string TitleUnicode { get; set; } = ""; // zh/jp support
        public string Artist { get; set; } = "";
        public string ArtistUnicode { get; set; } = "";
    }

    public static BeatmapMetadata ReadMetadata(string filePath) {
        var metadata = new BeatmapMetadata();
        var txt = File.ReadAllText(filePath);

        // regex magic
        // or horror
        var dict = Regex.Matches(txt, @"(?m)^(Title(?:Unicode)?|Artist(?:Unicode)?):(.+)$")
                        .Cast<Match>()
                        .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value.Trim());

        // hopefully doesn't break ... it shouldn't, probably ...
        metadata.Title = dict.GetValueOrDefault("Title");
        metadata.TitleUnicode = dict.GetValueOrDefault("TitleUnicode");
        metadata.Artist = dict.GetValueOrDefault("Artist");
        metadata.ArtistUnicode = dict.GetValueOrDefault("ArtistUnicode");

        return metadata;
    }
}