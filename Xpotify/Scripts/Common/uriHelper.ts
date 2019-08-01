namespace XpoMusicScript.Common.UriHelper {

    export function getPwaUri(uri) {
        if (uri === undefined || uri.trim() === "") {
            return "";
        }

        uri = uri.replace('http://', 'https://');
        var uriLowerCase = uri.toLowerCase();

        if (uriLowerCase.startsWith("https://open.spotify.com"))
            return uri;

        if (uriLowerCase.startsWith("spotify:")) {
            if (uriLowerCase.indexOf("spotify:artist:") >= 0) {
                var idx = uriLowerCase.indexOf("spotify:artist:") + "spotify:artist:".length;
                return "https://open.spotify.com/artist/" + uri.substring(idx);
            }
            else if (uriLowerCase.indexOf("spotify:album:") >= 0) {
                var idx = uriLowerCase.indexOf("spotify:album:") + "spotify:album:".length;
                return "https://open.spotify.com/album/" + uri.substring(idx);
            }
            else if (uriLowerCase.indexOf("spotify:playlist:") >= 0) {
                var idx = uriLowerCase.indexOf("spotify:playlist:") + "spotify:playlist:".length;
                return "https://open.spotify.com/playlist/" + uri.substring(idx);
            }
            else if (uriLowerCase.indexOf("spotify:track:") >= 0) {
                var idx = uriLowerCase.indexOf("spotify:track:") + "spotify:track:".length;
                return "https://open.spotify.com/track/" + uri.substring(idx);
            }
        }

        return "";
    }
}