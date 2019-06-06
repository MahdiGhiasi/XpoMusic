
function getCurrentSongPlayTime() {
    var time = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .playback-bar .playback-bar__progress-time");

    if (time.length === 2) {
        return time[0].innerText;
    }

    return "";
}

getCurrentSongPlayTime();
