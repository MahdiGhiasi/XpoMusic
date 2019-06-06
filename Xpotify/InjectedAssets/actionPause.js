
function actionPause() {
    var pauseButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-pause-16");

    if (pauseButton.length === 1) {
        pauseButton[0].click();
        return "1";
    }

    return "0";
}

actionPause();
