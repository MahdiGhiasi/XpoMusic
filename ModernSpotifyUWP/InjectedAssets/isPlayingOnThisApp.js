
function isPlayingOnThisApp() {
    return (document.querySelectorAll(".Root__now-playing-bar .ConnectBar").length === 0) ? "1" : "0";
}

isPlayingOnThisApp();
