using System;
using System.Diagnostics;
using System.Net;
using System.Xml;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;

/*
    i was planning to just use YAML but lastfm library needs Newtonsoft JSON
    so user config is in YAML (includes apikey and secret), and session specifically is JSON
    slightly strange but we ball
*/

public class Scrobbler {
    static readonly string sessionFile = Path.Combine(AppContext.BaseDirectory, "lastfm.session");

    // returns LastfmClient
    // mostly yoinked from my muse dash scrobbler with minimal changes
    public static async Task<LastfmClient> createScrobbler() {
        LastAuth auth;

        var apiKey = Program.config.apiKey; // eh.. is this ugly ..
        var apiSecret = Program.config.apiSecret;

        if (File.Exists(sessionFile)) {
            // create auth from session
            auth = new LastAuth(apiKey, apiSecret);
            LastUserSession session = JsonConvert.DeserializeObject<LastUserSession>(File.ReadAllText(sessionFile));
            auth.LoadSession(session);

            if (auth.Authenticated) {
                Program.WriteLineColor("last.fm authentication from session file successful!\n", ConsoleColor.Green);
                return (new LastfmClient(auth));
            } else {
                Program.WriteLineColor("last.fm authentication from session file unsuccessful!", ConsoleColor.Red);
            } 
        }
        // not returned at this point means auth has failed - not existing/expired

        int port = 54321;
        string doneUrl = $"http://localhost:{port}";

        // create http listener
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        // url that user must open to authenticate
        string url = $"http://www.last.fm/api/auth/?api_key={apiKey}&cb={doneUrl}/";
        Console.WriteLine($"if not automatically opened, please open: \n{url}\n");

        // open in browser
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });

        // wait for the page to open
        var context = await listener.GetContextAsync();

        // obtain token, which is appended at the end of the localhost URL
        // <callback_url>/?token=xxxxxxx
        var request = context.Request;
        string token = request.QueryString["token"];

        // wrap up localhost stuff
        var r = context.Response;
        var buffer = System.Text.Encoding.UTF8.GetBytes("you can close this tab now :3");
        r.ContentLength64 = buffer.Length;
        r.OutputStream.Write(buffer);
        r.OutputStream.Close();

        listener.Stop();

        // create auth
        auth = new LastAuth(apiKey, apiSecret);
        var resp = await auth.GetSessionTokenAsync(token);
        Console.WriteLine($"lastfm auth {(auth.Authenticated?"un":"")}successful! ");

        var lastfm = new LastfmClient(auth);

        // save the session to file
        var json = JsonConvert.SerializeObject(auth.UserSession, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(sessionFile, json);

        return lastfm;
    }

}
