namespace XpoMusicScript.Common.UiInjector {
    export function injectNavbarDownButton(button) {
        var sessionInfo = document.querySelectorAll(".Root__nav-bar nav > div:last-child");
        if (sessionInfo.length === 0) {
            setTimeout(function () {
                injectNavbarDownButton(button);
            }, 500);
        } else {
            var navbar = sessionInfo[0].parentElement;
            navbar.insertBefore(button, sessionInfo[0]);

            (<HTMLElement>(sessionInfo[0])).style.display = 'none';
        }
    }

    export function injectNowPlayingRightButton(button) {
        var extraControlsBar = document.querySelectorAll('.Root__now-playing-bar .ExtraControls');
        if (extraControlsBar.length === 0) {
            setTimeout(function () {
                injectNowPlayingRightButton(button);
            }, 500);
        } else {
            extraControlsBar[0].prepend(button);
        }
    }

    export function injectNowPlayingNavBarButton(button) {
        var extraControlsBar = document.querySelectorAll('.Root__top-container .Root__nav-bar nav ul');
        if (extraControlsBar.length === 0) {
            setTimeout(function () {
                injectNowPlayingNavBarButton(button);
            }, 500);
        } else {
            extraControlsBar[0].append(button);
        }
    }
}