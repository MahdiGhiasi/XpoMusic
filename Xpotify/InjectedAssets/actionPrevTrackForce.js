
function getCurrentPlaying() {
    var nowPlayingBox = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left');

    if (nowPlayingBox.length > 0) {
        return nowPlayingBox[0].innerText;
    } else {
        return "";
    }
}

function actionPrevTrackForce() {
    var prevButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16");

    if (prevButton.length === 1) {
        var currentPlaying = getCurrentPlaying();

        prevButton[0].click();

        setTimeout(function () {
            var newCurrentPlaying = getCurrentPlaying();
            if (currentPlaying === newCurrentPlaying) {
                prevButton[0].click();
            }
        }, 1000);

        return "1";
    }

    return "0";
}

actionPrevTrackForce();
