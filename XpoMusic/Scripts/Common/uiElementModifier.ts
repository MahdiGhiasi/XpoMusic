﻿/// <reference path="action.ts" />
/// <reference path="browserHistory.ts" />
/// <reference path="uiInjector.ts" />
/// <reference path="color.ts" />

namespace XpoMusicScript.Common.UiElementModifier {

    export function createBackButton(): string {
        try {
            var body = <HTMLElement>document.getElementsByTagName('body')[0];
            var backButtonDiv = document.createElement('div');
            backButtonDiv.classList.add("backButtonContainer");
            backButtonDiv.classList.add("backButtonContainer-disabled");
            backButtonDiv.innerHTML = "<span>&#xE72B;</span>";
            backButtonDiv.onclick = BrowserHistory.goBack;
            body.appendChild(backButtonDiv);
        }
        catch (ex) {
            window.XpoMusic.Log("injectBackFailed");
            window.XpoMusic.Log(ex.toString());
            return "injectBackFailed,";
        }
        return "";
    }

    export function createNavBarButtons(): string {
        try {
            var pinToStartButton = document.createElement('div');
            pinToStartButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifypintostart-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE718;</span></div>'
                + '<span class="navbar-link__text">Pin this page to Start</span></div></a></div>';
            pinToStartButton.querySelector('a').onclick = function () {
                window.XpoMusic.PinToStart();
                return false;
            };

            var settingsButton = document.createElement('div');
            settingsButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE115;</span></div>'
                + '<span class="navbar-link__text">Settings</span></div></a></div>';
            settingsButton.querySelector('a').onclick = function () {
                window.XpoMusic.OpenSettings();
                return false;
            };

            //var accountButton = document.createElement('div');
            //accountButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
            //    + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE8D4;</span></div>'
            //    + '<span class="navbar-link__text">Account</span></div></a></div>';
            //accountButton.querySelector('a').onclick = function () {
            //    Action.navigateToPage('/settings/account');
            //    return false;
            //};

            //var logOutButton = document.createElement('div');
            //logOutButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
            //    + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xF3B1;</span></div>'
            //    + '<span class="navbar-link__text">Log out</span></div></a></div>';
            //logOutButton.querySelector('a').onclick = function () {
            //    (<HTMLElement>(document.querySelectorAll(".Root__top-bar a[role=button]")[0])).click();
            //    document.body.style.opacity = '0';
            //    return false;
            //};

            var aboutButton = document.createElement('div');
            aboutButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE946;</span></div>'
                + '<span class="navbar-link__text">About</span></div></a></div>';
            aboutButton.querySelector('a').onclick = function () {
                window.XpoMusic.OpenAbout();
                return false;
            };

            var donateButton = document.createElement('div');
            donateButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE719;</span></div>'
                + '<span class="navbar-link__text">Donate</span></div></a></div>';
            donateButton.querySelector('a').onclick = function () {
                window.XpoMusic.OpenDonate();
                return false;
            };

            // UiInjector.injectNavbarDownButton(logOutButton);
            UiInjector.injectNavbarDownButton(settingsButton);
            UiInjector.injectNavbarDownButton(pinToStartButton);
            //UiInjector.injectNavbarDownButton(aboutButton);
            //if (!Common.isProVersion())
            //    UiInjector.injectNavbarDownButton(donateButton);
        }
        catch (ex) {
            window.XpoMusic.Log("injectNavBarFooterFailed");
            window.XpoMusic.Log(ex.toString());
            return "injectNavBarFooterFailed,";
        }
        return "";
    }

    export function createCompactOverlayButton(): string {
        // Inject compact overlay button to now playing bar
        try {
            var compactOverlayButton = document.createElement('div');
            compactOverlayButton.classList.add("CompactOverlayButton");
            compactOverlayButton.classList.add("CompactOverlayButton-disabled");
            compactOverlayButton.innerHTML = '<a style="border-bottom: 0px;" href="#"><button title="Mini view" class="control-button">'
                + '<div style="font-family: Segoe MDL2 Assets; position:relative; cursor: default;">'
                + '<div style="left: 6px; top: -3px; font-size: 19px; position: absolute;">&#xE7FB;</div>'
                + '<div style="left: 12px; top: -6px; font-size: 9px; position: absolute;">&#xEB9F;</div>'
                + '</div></button></a>';
            compactOverlayButton.querySelector('a').onclick = function () {
                window.XpoMusic.OpenMiniView();
                return false;
            };
            UiInjector.injectNowPlayingRightButton(compactOverlayButton);
        }
        catch (ex) {
            window.XpoMusic.Log("injectCompactOverlayFailed");
            window.XpoMusic.Log(ex.toString());
            return "injectCompactOverlayFailed,";
        }
        return "";
    }

    export function addNowPlayingButton() {
        try {
            var nowPlayingButton = document.createElement('div');
            nowPlayingButton.innerHTML = '<li class="navBar-group"><div class="GlueDropTarget">'
                + '<div class="navBar-item navBar-item--with-icon-left nowPlaying-navBar-item nowPlaying-navBar-item-disabled">'
                + '<a class="link-subtle navBar-link ellipsis-one-line" aria-label="Now Playing" href="#"><div class="navBar-link-text-with-icon-wrapper">'
                + '<div class="icon NavBar__icon nowPlaying-icon"></div>'
                + '<span class="navbar-link__text">Now Playing</span></div></a></div></div></li>';
            nowPlayingButton.querySelector('a').onclick = function () {
                window.XpoMusic.OpenNowPlaying();
                return false;
            };
            UiInjector.injectNowPlayingNavBarButton(nowPlayingButton);
        }
        catch (ex) {
            window.XpoMusic.Log("addNowPlayingButtonFailed");
            window.XpoMusic.Log(ex.toString());
            return "addNowPlayingButtonFailed,";
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
            window.XpoMusic.Log("findBackgroundDivFailed");
            window.XpoMusic.Log(ex.toString());
            return "findBackgroundDivFailed,";
        }
        return "";
    }

    export function createTrackListAddRemoveButtons(): boolean {
        try {
            var count = 0;
            var needsRefresh = false;
            var tracks = document.querySelectorAll(".tracklist .tracklist-row");
            for (var i = 0; i < tracks.length; i++) {
                if (tracks[i].getAttribute("data-xpotify-addremovebuttonsadded") !== null) {
                    if (!tracks[i].classList.contains("tracklistSongExistsInLibrary") && !tracks[i].classList.contains("tracklistSongNotExistsInLibrary") && tracks[i].getAttribute("data-trackid") !== null && tracks[i].querySelectorAll('.more').length > 0) {
                        needsRefresh = true;
                    }
                    continue;
                }

                var addSongDiv = document.createElement('button');
                addSongDiv.classList.add("tracklist-middle-align");
                addSongDiv.classList.add("control-button");
                addSongDiv.classList.add("spoticon-add-16");
                addSongDiv.classList.add("trackListAddRemoveSongButton");
                addSongDiv.classList.add("trackListAddSongButton");
                addSongDiv.setAttribute("title", "Add to your Liked Songs");
                addSongDiv.addEventListener('click', async function (e) {
                    var row = (<HTMLElement>e.target).closest('.tracklist-row');
                    var trackId = row.getAttribute("data-trackid");

                    row.classList.add('tracklistSongExistsInLibrary');
                    row.classList.remove('tracklistSongNotExistsInLibrary');

                    var libraryApi = new SpotifyApi.Library();
                    var result = await libraryApi.saveTrack(trackId);

                    if (!result) {
                        row.classList.add('tracklistSongNotExistsInLibrary');
                        row.classList.remove('tracklistSongExistsInLibrary');
                    }
                });

                var removeSongDiv = document.createElement('button');
                removeSongDiv.classList.add("tracklist-middle-align");
                removeSongDiv.classList.add("control-button");
                removeSongDiv.classList.add("spoticon-added-16");
                removeSongDiv.classList.add("trackListAddRemoveSongButton");
                removeSongDiv.classList.add("trackListRemoveSongButton");
                removeSongDiv.setAttribute("title", "Remove from your Liked Songs");
                removeSongDiv.addEventListener('click', async function (e) {
                    var row = (<HTMLElement>e.target).closest('.tracklist-row');
                    var trackId = row.getAttribute("data-trackid");

                    row.classList.add('tracklistSongNotExistsInLibrary');
                    row.classList.remove('tracklistSongExistsInLibrary');

                    var libraryApi = new SpotifyApi.Library();
                    var result = await libraryApi.removeTrack(trackId);

                    if (!result) {
                        row.classList.add('tracklistSongExistsInLibrary');
                        row.classList.remove('tracklistSongNotExistsInLibrary');
                    }
                });

                var destContainer = tracks[i].querySelectorAll('.more')[0];

                if (destContainer !== undefined) {
                    destContainer.appendChild(addSongDiv);
                    destContainer.appendChild(removeSongDiv);
                    count++;
                }

                tracks[i].setAttribute("data-xpotify-addremovebuttonsadded", "1");
            }

            return (count > 0) || needsRefresh;
        } catch (ex) {
            window.XpoMusic.Log(ex.toString());
            return false;
        }
    }
}