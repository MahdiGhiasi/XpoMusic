/// <reference path="browserHistory.ts" />
/// <reference path="uiInjector.ts" />
/// <reference path="color.ts" />

namespace InitScript.Common.UiElementModifier {
    export function createPageTitle(): string {
        try {
            var body = <HTMLElement>document.getElementsByTagName('body')[0];
            var titleDiv = document.createElement('div');
            titleDiv.classList.add("xpotifyWindowTitle");
            titleDiv.innerText = isProVersion() ? "Xpotify Pro" : "Xpotify";
            body.appendChild(titleDiv);
        }
        catch (ex) {
            return "injectTitleFailed,";
        }
        return "";
    }

    export function createBackButton(): string {
        try {
            var backButtonDiv = document.createElement('div');
            backButtonDiv.classList.add("backButtonContainer");
            backButtonDiv.classList.add("backButtonContainer-disabled");
            backButtonDiv.innerHTML = "<a class='backbutton'><span>&#xE72B;</span></a>";
            backButtonDiv.onclick = BrowserHistory.goBack;
            UiInjector.injectBackButton(backButtonDiv);
        }
        catch (ex) {
            return "injectBackFailed,";
        }
        return "";
    }

    export function createNavBarButtons(): string {
        try {
            var pinToStartButton = document.createElement('div');
            pinToStartButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifypintostart-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifypintostart">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE718;</span></div>'
                + '<span class="navbar-link__text">Pin this page to Start</span></div></a></div>';
            var settingsButton = document.createElement('div');
            settingsButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifysettings">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE115;</span></div>'
                + '<span class="navbar-link__text">Settings</span></div></a></div>';
            var aboutButton = document.createElement('div');
            aboutButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifyabout">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE946;</span></div>'
                + '<span class="navbar-link__text">About</span></div></a></div>';
            var donateButton = document.createElement('div');
            donateButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifydonate">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE719;</span></div>'
                + '<span class="navbar-link__text">Donate</span></div></a></div>';
            UiInjector.injectNavbarDownButton(pinToStartButton);
            UiInjector.injectNavbarDownButton(settingsButton);
            //UiInjector.injectNavbarDownButton(aboutButton);
            if (!isProVersion())
                UiInjector.injectNavbarDownButton(donateButton);
        }
        catch (ex) {
            return "injectNavBarFooterFailed,";
        }
        return "";
    }

    export function createCompactOverlayButton(): string {
        // Inject compact overlay button to now playing bar
        try {
            var compactOverlayButton = document.createElement('div');
            compactOverlayButton.className = "CompactOverlayButton";
            compactOverlayButton.innerHTML = '<a style="border-bottom: 0px;" href="#xpotifycompactoverlay"><button title="Mini view" class="control-button">'
                + '<div style="font-family: Segoe MDL2 Assets; position:relative; cursor: default;">'
                + '<div style="left: 6px; top: -3px; font-size: 19px; position: absolute;">&#xE7FB;</div>'
                + '<div style="left: 12px; top: -6px; font-size: 9px; position: absolute;">&#xEB9F;</div>'
                + '</div></button></a>';
            UiInjector.injectNowPlayingRightButton(compactOverlayButton);
        }
        catch (ex) {
            return "injectCompactOverlayFailed,";
        }
        return "";
    }

    export function addBackgroundClass() {
        // Find and add necessary class to background div
        try {
            setTimeout(function () {
                Color.addXpotifyClassToBackground(12);
            }, 250);

            // Sometimes the PWA changes the background element on loading, causing the 
            // background class to be removed. We'll do this again after a few seconds 
            // to make sure that does not happen.
            setTimeout(function () {
                Color.addXpotifyClassToBackground(0);
            }, 4000);
        }
        catch (ex) {
            return "findBackgroundDivFailed,";
        }
        return "";
    }

    export function addIndeterminateProgressBar() {
        try {
            var progressBar = document.createElement('div');
            progressBar.classList.add("xpotify-indeterminateprogressbar");
            progressBar.classList.add("playbackBar-indeterminateprogressbar");
            progressBar.innerHTML = '<div class="xpotify-indeterminateprogressbar-inner">'
                + '<div class="xpotify-indeterminateprogressbar-dot-container"><span /></div>'
                + '<div class="xpotify-indeterminateprogressbar-dot-container"><span /></div>'
                + '<div class="xpotify-indeterminateprogressbar-dot-container"><span /></div>'
                + '<div class="xpotify-indeterminateprogressbar-dot-container"><span /></div>'
                + '<div class="xpotify-indeterminateprogressbar-dot-container"><span /></div>'
                + '</span></div>';

            var progressBarContainer = document.querySelectorAll(".Root__now-playing-bar .playback-bar .progress-bar")[0];
            progressBarContainer.append(progressBar);
        }
        catch (ex) {
            return "addIndeterminateProgressBarFailed,";
        }
        return "";

    }
}