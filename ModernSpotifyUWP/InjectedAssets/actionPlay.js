
function actionPlay() {
    var playButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-play-16");

    if (playButton.length === 1) {
        playButton[0].click();
        return "1";
    }

    return "0";
}

actionPlay();
