
function tryPlay(remainingCount) {
    if (remainingCount <= 0)
        return;

    console.log('r' + remainingCount);

    setTimeout(function () {
        var modalPlayButton = document.querySelectorAll(".autoplay-modal-container .btn-green");
        if (modalPlayButton.length > 0)
            modalPlayButton[0].click();
        else
            tryPlay(remainingCount - 1);
    }, 500);
}

tryPlay(20);

