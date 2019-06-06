
function actionPrevTrack() {
    var prevButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16");

    if (prevButton.length === 1) {
        prevButton[0].click();
        return "1";
    }

    return "0";
}

actionPrevTrack();
