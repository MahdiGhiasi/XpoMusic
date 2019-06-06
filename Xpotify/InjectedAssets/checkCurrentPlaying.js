
function getCurrentPlaying() {
    var nowPlayingBox = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left');

    if (nowPlayingBox.length > 0) {
        return nowPlayingBox[0].innerText;
    } else {
        return "";
    }
}


getCurrentPlaying();
