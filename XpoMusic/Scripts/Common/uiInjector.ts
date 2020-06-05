namespace XpoMusicScript.Common.UiInjector {
    export function injectNavbarDownButton(button) {
        var sessionInfo = document.querySelectorAll(".navBar-item.NavBar__download-item");
        if (sessionInfo.length === 0) {
            setTimeout(function () {
                injectNavbarDownButton(button);
            }, 500);
        } else {
            var navbar = sessionInfo[0].parentElement;
            navbar.insertBefore(button, sessionInfo[0]);
        }
    }

    export function injectNowPlayingRightButton(button) {
        var extraControlsBar = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__right__inner .ExtraControls');
        if (extraControlsBar.length === 0) {
            setTimeout(function () {
                injectNowPlayingRightButton(button);
            }, 500);
        } else {
            extraControlsBar[0].prepend(button);
        }
    }

    export function injectNowPlayingNavBarButton(button) {
        var extraControlsBar = document.querySelectorAll('.Root__top-container .navBar ul');
        if (extraControlsBar.length === 0) {
            setTimeout(function () {
                injectNowPlayingNavBarButton(button);
            }, 500);
        } else {
            extraControlsBar[0].append(button);
        }
    }
}