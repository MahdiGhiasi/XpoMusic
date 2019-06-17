
function enableNowPlaying() {
    var nowPlayingButton = document.querySelectorAll('.nowPlaying-navBar-item');
    if (nowPlayingButton.length > 0) {
        nowPlayingButton[0].classList.remove('nowPlaying-navBar-item-disabled');
    } else {
        setTimeout(enableNowPlaying, 1000);
        return;
    }

    var compactOverlayButton = document.querySelectorAll('.CompactOverlayButton');
    if (compactOverlayButton.length > 0) {
        compactOverlayButton[0].classList.remove('CompactOverlayButton-disabled');
    } else {
        setTimeout(enableNowPlaying, 1000);
        return;
    }
}

enableNowPlaying();
