
# circle scrobbler

A last.fm scrobbler for the famous circle clicking game, osu!

I decided to write this because although [this scrobbler](https://github.com/iMyon/OsuLastfmScrobbler) by [iMyon](https://github.com/iMyon) has served me well for many scrobbles, it's become inconsistent in whether it works or not.

## Main Features
- No plaintext passwords: OAuth is used to scrobble on your behalf.
- Very simple, but configurable: Does what it says, but has some nifty options (see config section).

## Setup

1. Download from the [latest release](https://github.com/lawrencfgsdfg/circle-scrobbler/releases).
2. On initial run, `config.yaml` will be created in the same directory as the program.
3. You will need a [last.fm API account](https://www.last.fm/api/account/create). Copy the revelevant details to the config file. 
4. Ensure that osu! is opened. Re-open the program, and an authorization page should open in your browser. Click "Allow Access".
5. Scrobbles should now be tracked while you play osu!

## config.yaml
|Setting|Description|
|---|---|
|apiKey|[Application API Key](https://www.last.fm/api/accounts)|
|apiSecret|[Application Shared Secret](https://www.last.fm/api/accounts)|
|pollingFrequency|How frequently, in milliseconds, to read osu!'s memory to track game activity.<br/>Default value is `100`.|
|listenedPercent|The amount of a song that must be listened to in order to be counted as a scrobble.<br/>Represented as a decimal between 0 and 1. The default value is `0.5`.|
|minimumSongLength|The minimum length of a song, in milliseconds, to be counted as a scrobble.<br/>Default value is `30000`.|
|titleRegex|Regex pattern to <b>remove</b> from song titles.<br>Example: `(?i)\([^)]*cut[^)]*\)` will target all parentheses that contain "cut", such as "(Cut Ver.)" or "(Sped Up & Cut Ver.)".<br>Note that you do <i>not</i> need to wrap your pattern in quotation marks.<br>This is empty by default.|
|artistRegex|Regex pattern to <b>remove</b> from artist names.<br>This is empty by default.|
|preferUnicodeTitle|Whether or not to prefer unicode over romanised song titles.<br>Default value is `true`.|
|preferUnicodeArtist|Whether or not to prefer unicode over romanised artist names.<br>Default value is `true`.|
|updateNowPlaying|Whether or not to update the "now scrobbling" on your profile.<br>When quickly switching in song select, last.fm may get confused and display an older track.<br>Default value is `true`.|

## Potential Future Plans
- Allow scrobbling outside of gameplay.
- Cross check metadata with Spotify or other platforms.
- Attempt to make "now scrobbling" more accurate.

## Developer Comments

Thanks to [Piotrekol](https://github.com/Piotrekol) for this [seriously amazing magic stuff](https://github.com/Piotrekol/ProcessMemoryDataFinder) that made this all possible.

Thanks to [kapral](https://github.com/kapral) for their [fork of this .NET last.fm library](https://github.com/kapral/lastfm/tree/next).

If you have questions, feedback, etc, don't hesitate to open an [issue](https://github.com/lawrencfgsdfg/circle-scrobbler/issues) or [pull request](https://github.com/lawrencfgsdfg/circle-scrobbler/pulls).

<b>:3</b>
