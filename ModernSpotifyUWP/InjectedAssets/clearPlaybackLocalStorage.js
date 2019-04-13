
// This script works around a bug of Spotify PWA in Microsoft Edge,
// where volume is reset to the initial value in LocalStorage every few
// minutes, by clearing the LocalStorage.playback before opening the PWA

function clearPlaybackLocalStorage() {
    var value = window.localStorage.playback;
    window.localStorage.removeItem('playback');

    return value;
}


clearPlaybackLocalStorage();
