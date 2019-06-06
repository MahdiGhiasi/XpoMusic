# Xpotify
A modern Spotify experience for Windows 10

<a href='//www.microsoft.com/store/apps/9N1N68MC7FXR?cid=storebadge&ocid=badge'><img src='https://assets.windowsphone.com/85864462-9c82-451e-9355-a3d5f874397a/English_get-it-from-MS_InvariantCulture_Default.png' alt='English badge' width="142" height="52"/></a>

## How to build

Create a file named `Secrets.cs` in the `ModernSpotifyUWP/` directory, and put the following content into it:

    namespace ModernSpotifyUWP
    {
        internal static class Secrets
        {
            internal static readonly string SpotifyClientId = "";
            internal static readonly string SpotifyClientSecret = "";
            internal static readonly string GoogleAnalyticsTrackerId = "";
        }
    }

Sign up on [Spotify Developer website](https://developer.spotify.com/) and get an API key for yourself. Put Id and Secret that you get from Spotify into the `SpotifyClientId` and `SpotifyClientSecret` fields. You can leave the `GoogleAnalyticsTrackerId` empty.


