namespace InitScript.Common.UiInjector {
    export function injectBackButton(backButtonDiv) {
        var navbarHeader = document.getElementsByClassName('navBar-header');
        if (navbarHeader.length === 0) {
            setTimeout(function () {
                injectBackButton(backButtonDiv);
            }, 500);
        } else {
            navbarHeader[0].prepend(backButtonDiv);
        }
    }

    export function injectNavbarDownButton(button) {
        var navbar = document.querySelectorAll(".NavBarFooter");
        var sessionInfo = document.querySelectorAll(".sessionInfo");
        if (navbar.length === 0 || sessionInfo.length === 0) {
            setTimeout(function () {
                injectNavbarDownButton(button);
            }, 500);
        } else {
            navbar[0].insertBefore(button, sessionInfo[0]);
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