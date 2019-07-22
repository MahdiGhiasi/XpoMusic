# Xpotify
A modern Spotify experience for Windows 10

<a href='//www.microsoft.com/store/apps/9N1N68MC7FXR?cid=storebadge&ocid=badge'><img src='https://assets.windowsphone.com/85864462-9c82-451e-9355-a3d5f874397a/English_get-it-from-MS_InvariantCulture_Default.png' alt='English badge' width="142" height="52"/></a>

## Building from source

#### Requirements

You need to have these components installed in order to build Xpotify:

* Visual Studio 2017
* Windows 10 SDK v1809 (17763)
* TypeScript
* [Web Compiler extension for Visual Studio](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebCompiler)

#### Configuration

Create a file named `Secrets.cs` in the `Xpotify/` directory, and put the following content into it:

    namespace Xpotify
    {
        internal static class Secrets
        {
            internal static readonly string SpotifyClientId = "";
            internal static readonly string SpotifyClientSecret = "";
            internal static readonly string GoogleAnalyticsTrackerId = "";
        }
    }

Sign up on [Spotify Developer website](https://developer.spotify.com/) and get an API key for yourself. Put Id and Secret that you get from Spotify into the `SpotifyClientId` and `SpotifyClientSecret` fields. You can leave the `GoogleAnalyticsTrackerId` empty.

Also, you will need to add `https://xpomusic.ghiasi.net/login/redirect` as the redirect URI on Spotify developer dashboard for the app entry you created. Alternatively, you can choose a different redirect URI and then modify [this line of code](https://github.com/MahdiGhiasi/Xpotify/blob/7e003b9879104a5b8b771f48475feca92155de8a/Xpotify/SpotifyApi/Authorization.cs#L18) accordingly.

## Contributing

If you want to work on a bug or an enhancement that is already present and approved in the Issues, please leave a comment under that issue stating that you're going to work on it (so we can avoid doing duplicate work).

Also, if you want to work on a new feature you have in mind for Xpotify, please create an issue first so we can discuss it.

## License

Xpotify is available under [GNU General Public License v3.0](https://github.com/MahdiGhiasi/Xpotify/blob/master/LICENSE.md).
