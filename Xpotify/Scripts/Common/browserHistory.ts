namespace XpoMusicScript.Common.BrowserHistory {
    export function canGoBack() {
        return window.location.hash !== "#xpotifyInitialPage";
    }

    export function goBack() {
        if (canGoBack()) {
            window.history.go(-1);
        }
    }

}