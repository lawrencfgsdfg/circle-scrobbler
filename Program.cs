using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using static BeatmapUtils;

class Program {
    static readonly String VERSION = "1.0.3b";

    public static Config config;
    static BeatmapMetadata beatmapMetadata = new BeatmapMetadata();
    static DateTime startTime = DateTime.MinValue;

    static void Main(string[] args) {
        Console.WriteLine($"circle scrobbler v{VERSION}");

        // lil nasty one liner to grab osu directory
        String osuDirectory = "";
        try {
            osuDirectory = Path.GetDirectoryName(Process.GetProcessesByName("osu!")[0].MainModule.FileName);
        } catch(Exception e) {
            Console.WriteLine("osu! not found, is it open? - "+e.Message);
            Environment.Exit(1);
        }

        config = Config.loadFromFile();
        if (config.apiKey == "" || config.apiSecret == "") {
            WriteLineColor("apiKey and/or apiSecret are not defined in your config file!", ConsoleColor.Yellow);
            Environment.Exit(0);
        }

        int previousStatus = -1000;
        int previousRetries = -69;
        string previousFileName = "";
        bool scrobble = false;
        
        LastfmClient lastfm = Scrobbler.createScrobbler().Result;

        // nice lil greeting
        var username = lastfm.Auth.UserSession.Username;
        var playcount = lastfm.User.GetInfoAsync(username).Result.Content.Playcount;
        WriteLineColor(DateTime.Now, ConsoleColor.Cyan);
        WriteLineColor($"hello {username}, you have {playcount} scrobbles!\n", ConsoleColor.Cyan);
        //

        var reader = StructuredOsuMemoryReader.GetInstance(new("osu!"));
        var baseAddresses = new OsuBaseAddresses();

        // allow zh/jp to be printed correctly instead of "?????"
        Console.OutputEncoding = Encoding.Unicode;

        // avoid instant "can't read" message
        Thread.Sleep(100);

        var couldRead = false;
        while (true) {
            var canRead = reader.CanRead;
            if (canRead) {
                reader.TryRead(baseAddresses.GeneralData);
                int status = baseAddresses.GeneralData.RawStatus;
                int retries = baseAddresses.GeneralData.Retries;
                int audioTime = baseAddresses.GeneralData.AudioTime;
                double totalAudioTime = baseAddresses.GeneralData.TotalAudioTime;

                reader.TryRead(baseAddresses.Beatmap);
                string folderName = baseAddresses.Beatmap.FolderName;
                string fileName = baseAddresses.Beatmap.OsuFileName;

                // status has changed OR player has retried (executes on first run)
                if (previousStatus != status || previousRetries != retries) {
                    //Console.WriteLine("aqui "+scrobble);
                    // status updated FROM playing
                    if (previousStatus == (int)OsuMemoryStatus.Playing) {
                        if (scrobble) {
                            Scrobble s = makeScrobble();
                            lastfm.Scrobbler.ScrobbleAsync(s);
                            Console.WriteLine($"{ s.TimePlayed.DateTime} >  { s.Artist} - {s.Track}");
                        }
                        scrobble = false;
                    }
                    // status updated TO playing
                    if (status == (int)OsuMemoryStatus.Playing) {
                        // update beatmap metadata
                        try { // i .. don't even know anymore, i had a crash here and i don't know what it was. try catch
                            String path = osuDirectory + "/Songs/" + folderName + "/" + fileName;
                            path = path.Replace("\0", "");
                            beatmapMetadata = BeatmapUtils.ReadMetadata(path);
                        } catch(Exception e) {
                            WriteLineColor($"error reading metadata from {osuDirectory}/Songs/{folderName}/{fileName}: {e.Message}", ConsoleColor.Red);
                        }
                        startTime = DateTime.Now;
                    }
                }

                if (status == (int)OsuMemoryStatus.Playing) {
                    scrobble = (audioTime / totalAudioTime) > config.listenedPercent // song has passed listening percentage threshold
                                && totalAudioTime > config.minimumSongLength // song is long enough
                                ;
                }

                // filename changed, it's easier to listen to this instead of constantly decoding metadata from the current .osu
                if (fileName != previousFileName) {
                    //Console.WriteLine($"AQUI {folderName} , {fileName}");
                    try {
                        String path = osuDirectory + "/Songs/" + folderName + "/" + fileName;
                        path = path.Replace("\0", "");
                        beatmapMetadata = BeatmapUtils.ReadMetadata(path);
                    } catch (Exception e) {
                        WriteLineColor($"error reading metadata from {osuDirectory}/Songs/{folderName}/{fileName}: {e.Message}", ConsoleColor.Red);
                    }
                    if (config.updateNowPlaying) lastfm.Track.UpdateNowPlayingAsync(makeScrobble());
                }

                previousStatus = status;
                previousFileName = fileName;
                previousRetries = retries;
            }
            //else Console.WriteLine("osu! process not found");

            if(couldRead != canRead) {
                if (canRead) {
                    WriteLineColor("osu! process found, listening!", ConsoleColor.Green);
                } else {
                    WriteLineColor("osu! process not found", ConsoleColor.Red);
                }
                couldRead = canRead;
            }
            Thread.Sleep(config.pollingFrequency);
        }
    }
    static Scrobble makeScrobble() {
        return new Scrobble( // big ahh object
            Regex.Replace((
                config.preferUnicodeArtist
                    ? beatmapMetadata.ArtistUnicode ?? beatmapMetadata.Artist ?? ""
                    : beatmapMetadata.Artist ?? beatmapMetadata.ArtistUnicode ?? ""
                    ), config.artistRegex
            , ""),
            null, // album data
            Regex.Replace((
                config.preferUnicodeTitle
                    ? beatmapMetadata.TitleUnicode ?? beatmapMetadata.Title ?? "" // idk why VS complained about null here, but not the same for the artists
                    : beatmapMetadata.Title ?? beatmapMetadata.ArtistUnicode ?? ""
                    ), config.titleRegex
            , ""),
            startTime
        );
    }

    // i really thought this would exist already ...
    public static void WriteLineColor(Object obj, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.WriteLine(obj);
        Console.ResetColor();
        return;
    }

    // :3

}