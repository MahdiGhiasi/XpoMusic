
function actionNextTrack() {
    var nextButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-forward-16");

    if (nextButton.length === 1) {
        nextButton[0].click();
        return "1";
    }

    return "0";
}

actionNextTrack();
