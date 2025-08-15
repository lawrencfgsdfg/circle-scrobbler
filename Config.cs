using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class Config {
    public string apiKey { get; set; } = "";
    public string apiSecret { get; set; } = "";
    public int pollingFrequency { get; set; } = 100;
    public double listenedPercent { get; set; } = 0.5;
    public double minimumSongLength { get; set; } = 30000;
    public string titleRegex { get; set; } = "";
    public string artistRegex { get; set; } = "";
    public bool preferUnicodeTitle { get; set; } = true;
    public bool preferUnicodeArtist { get; set; } = true;
    public bool updateNowPlaying { get; set; } = true;

    static readonly string configPath = Path.Combine(AppContext.BaseDirectory, "config.yaml");

    public static Config loadFromFile() {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        if (!File.Exists(configPath)) {
            var defaultConfig = new Config();
            File.WriteAllText(configPath, serializer.Serialize(defaultConfig));
            return defaultConfig;
        }

        var yaml = File.ReadAllText(configPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // in case any new options are added later on, the user will not have trouble.
        // potential additions will be written back to config file
        Config c = deserializer.Deserialize<Config>(yaml) ?? new Config();
        File.WriteAllText(configPath, serializer.Serialize(c));
        return c;
    }
}
