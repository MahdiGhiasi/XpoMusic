namespace XpotifyScript.Common.Action {

    export function nextTrack() {
        var nextButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-forward-16");

        if (nextButton.length === 1) {
            (<HTMLElement>nextButton[0]).click();
            return "1";
        }

        return "0";
    }

    export function pause() {
        var pauseButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-pause-16");

        if (pauseButton.length === 1) {
            (<HTMLElement>pauseButton[0]).click();
            return "1";
        }

        return "0";
    }

    export function play() {
        var playButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-play-16");

        if (playButton.length === 1) {
            (<HTMLElement>playButton[0]).click();
            return "1";
        }

        return "0";
    }

    export function playPause() {
        if (play() === '0') {
            pause();
        }
    }


    export function prevTrack() {
        var prevButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16");

        if (prevButton.length === 1) {
            (<HTMLElement>prevButton[0]).click();
            return "1";
        }

        return "0";
    }

    export function prevTrackForce() {
        var prevButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16");

        if (prevButton.length === 1) {
            var currentPlaying = StatusReport.getTrackFingerprint();

            (<HTMLElement>prevButton[0]).click();

            var delay = 300;
            if (document.querySelectorAll(".ConnectBar").length > 0)
                delay = 1250;

            setTimeout(function () {
                var newCurrentPlaying = StatusReport.getTrackFingerprint();
                if (currentPlaying === newCurrentPlaying) {
                    (<HTMLElement>prevButton[0]).click();
                }
            }, delay);

            return "1";
        }

        return "0";
    }

    function tryAutoPlayTrack(remainingCount) {
        if (remainingCount <= 0)
            return;

        //console.log('r' + remainingCount);

        setTimeout(function () {
            var modalPlayButton = document.querySelectorAll(".autoplay-modal-container .btn-green");
            if (modalPlayButton.length > 0)
                (<HTMLElement>modalPlayButton[0]).click();
            else
                tryAutoPlayTrack(remainingCount - 1);
        }, 500);
    }

    export function autoPlayTrack() {
        tryAutoPlayTrack(26);
    }

    function tryAutoPlayPlaylist(remainingCount) {
        if (remainingCount <= 0)
            return;

        //console.log('r' + remainingCount);

        setTimeout(function () {
            var modalPlayButton = document.querySelectorAll(".autoplay-modal-container .btn-green");
            var tracklistPlayButton = document.querySelectorAll(".TrackListHeader .btn-green");

            if (modalPlayButton.length > 0)
                (<HTMLElement>modalPlayButton[0]).click();
            else if (tracklistPlayButton.length > 0)
                (<HTMLElement>tracklistPlayButton[0]).click();
            else
                tryAutoPlayPlaylist(remainingCount - 1);
        }, 500);
    }

    export function autoPlayPlaylist() {
        tryAutoPlayPlaylist(26);
    }

    export function enableNowPlaying() {
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

    export function goBackIfPossible() {
        if (BrowserHistory.canGoBack()) {
            window.history.go(-1);
            return "1";
        }
        return "0";
    }

    export function goForwardIfPossible() {
        window.history.go(1);
    }

    export function isPlayingOnThisApp() {
        return (document.querySelectorAll(".Root__now-playing-bar .ConnectBar").length === 0) ? "1" : "0";
    }

    export function navigateToPage(url) {
        history.pushState({}, null, url);
        history.pushState({}, null, url + "#navigatingToPagePleaseIgnore");
        history.back();
    }

    export function seekPlayback(percentage) {
        var progressBar = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar .playback-bar .progress-bar")[0];

        var rect = progressBar.getBoundingClientRect();
        var x = rect.left + 1 + (rect.width - 2) * percentage;
        var y = rect.top + rect.height / 2;

        var mouseDownEvent = document.createEvent('MouseEvents');
        mouseDownEvent.initMouseEvent(
            'mousedown', true, true, window, 0,
            0, 0, x, y, false, false,
            false, false, 0, null
        );
        document.elementFromPoint(x, y).dispatchEvent(mouseDownEvent);

        var mouseUpEvent = document.createEvent('MouseEvents');
        mouseUpEvent.initMouseEvent(
            'mouseup', true, true, window, 0,
            0, 0, x, y, false, false,
            false, false, 0, null
        );
        document.elementFromPoint(x, y).dispatchEvent(mouseUpEvent);
    }

    export function seekVolume(percentage) {
        var progressBar = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar .volume-bar .progress-bar")[0];

        var rect = progressBar.getBoundingClientRect();
        var x = rect.left + (rect.width - 1) * percentage;
        var y = rect.top + rect.height / 2;

        var mouseDownEvent = document.createEvent('MouseEvents');
        mouseDownEvent.initMouseEvent(
            'mousedown', true, true, window, 0,
            0, 0, x, y, false, false,
            false, false, 0, null
        );
        document.elementFromPoint(x, y).dispatchEvent(mouseDownEvent);

        var mouseUpEvent = document.createEvent('MouseEvents');
        mouseUpEvent.initMouseEvent(
            'mouseup', true, true, window, 0,
            0, 0, x, y, false, false,
            false, false, 0, null
        );
        document.elementFromPoint(x, y).dispatchEvent(mouseUpEvent);
    }
}