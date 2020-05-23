﻿/// <reference path="../SpotifyApi/player.ts" />

namespace XpoMusicScript.Common.StatusReport {

    function getTextContent(element, index) {
        var e = document.querySelectorAll(element);

        if (e.length > index) {
            return (e[index]).innerText;
        } else {
            throw "Couldn't find element.";
        }
    }

    export function getTrackFingerprint() {
        return getTextContent('.Root__now-playing-bar .now-playing-bar__left', 0);
    }

    export function getTrackName() {
        return getTextContent('.Root__now-playing-bar .now-playing-bar__left .ellipsis-one-line', 1);
    }

    export function getTrackArtist() {
        return getTextContent('.Root__now-playing-bar .now-playing-bar__left .ellipsis-one-line', 2);
    }

    export function getTrackAlbumId() {
        var artistUri = (<HTMLLinkElement>((document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__left a")[1]))).href;
        return artistUri.substring(artistUri.lastIndexOf('/') + 1);
    }

    export function getTrackArtistId() {
        var artistUri = (<HTMLLinkElement>((document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__left a")[2]))).href;
        return artistUri.substring(artistUri.lastIndexOf('/') + 1);
    }

    export function timeStringToMilliseconds(time) {
        if (time.match('^[0-9]+\:[0-9][0-9]?$') === null)
            throw "Invalid time format";

        var parts = time.split(':');

        return (Number.parseInt(parts[0]) * 60 + Number.parseInt(parts[1])) * 1000;
    }

    export function getElapsedTime() {
        var time = getTextContent('.Root__now-playing-bar .now-playing-bar__center .playback-bar .playback-bar__progress-time', 0);
        return timeStringToMilliseconds(time);
    }

    export function getTotalTime() {
        var time = getTextContent('.Root__now-playing-bar .now-playing-bar__center .playback-bar .playback-bar__progress-time', 1);
        return timeStringToMilliseconds(time);
    }

    export function getIsSavedToLibrary() {
        var e = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left .spoticon-heart-active-16');
        return (e.length > 0);

        // This is not fatal, so we don't throw exception if this fails.
    }

    export function getIsPlaying() {
        var pauseButton = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__center .spoticon-pause-16');
        if (pauseButton.length > 0) {
            return true;
        }

        var playButton = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__center .spoticon-play-16');
        if (playButton.length > 0) {
            return false;
        }

        throw "Can't find play/pause buttons";
    }

    export function getIsPrevTrackAvailable() {
        var prevButton = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16');
        if (prevButton.length === 0) {
            throw "Can't find prevTrackButton";
        }
        if (((prevButton[0])).classList.contains("control-button--disabled")) {
            return false;
        }
        return true;
    }

    export function getIsNextTrackAvailable() {
        var nextButton = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__center .spoticon-skip-forward-16');
        if (nextButton.length === 0) {
            throw "Can't find nextTrackButton";
        }
        if (((nextButton[0])).classList.contains("control-button--disabled")) {
            return false;
        }
        return true;
    }

    export async function getTrackIdViaApi() {
        var fingerprint = getTrackFingerprint() + "\n" + (getIsPlaying() ? "playing" : "paused");
        // @ts-ignore
        if (window.xpotify_prevFingerprint === fingerprint) {
            // @ts-ignore
            return window.xpotify_prevTrackId;
        }
        // @ts-ignore
        window.xpotify_prevTrackId = ""; // Return empty at least until we get a response from api.
        // @ts-ignore
        window.xpotify_prevFingerprint = fingerprint;

        window.XpoMusic.Log("getTrackIdViaApi(): getting track id... local fingerprint: " + fingerprint);

        var playerApi = new SpotifyApi.Player();
        var result = await playerApi.getCurrentlyPlaying();

        var trackName = result.item.name;
        var trackId = result.item.id;
        window.XpoMusic.Log("getTrackIdViaApi() result: track name = '" + trackName + "'; id = '" + trackId + "'");

        if (trackName == getTrackName()) {
            window.XpoMusic.Log("getTrackIdViaApi(): retrieved name matches local name. Will return its id.");

            // @ts-ignore
            window.xpotify_prevTrackId = trackId;
            return trackId;
        }

        return "";
    }

    export async function getTrackId() {

        return await getTrackIdViaApi(); // right clicking on current playing song is broken in PWA!

        
        var tracks = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left .ellipsis-one-line a');
        if (tracks.length === 0)
            return "";

        var element = tracks[0];

        var menus = document.querySelectorAll("nav[role=menu]");
        for (var i = 0; i < menus.length; i++) {
            (<HTMLElement>menus[i]).style.display = 'none';
        }

        // Open context menu on track name on now playing bar
        var e = element.ownerDocument.createEvent('MouseEvents');
        e.initMouseEvent('contextmenu', true, true,
            element.ownerDocument.defaultView, 1, 0, 0, 0, 0, false,
            false, false, false, 2, null);
        element.dispatchEvent(e);

        var trackId = "";

        var tas = document.querySelectorAll("nav[role=menu] textarea");
        for (var j = 0; j < tas.length; j++) {
            if (tas[j].textContent.toLowerCase().startsWith("https://open.spotify.com/track/")) {
                trackId = tas[j].textContent.substr("https://open.spotify.com/track/".length);
            }
        }

        // Close context menu
        var clickX = 1;
        var clickY = window.innerHeight - 1;
        var e2 = document.createEvent('MouseEvents');
        e2.initMouseEvent(
            'mousedown', true, true, window, 0,
            0, 0, clickX, clickY, false, false,
            false, false, 0, null
        );
        document.elementFromPoint(clickX, clickY).dispatchEvent(e2);
        e2 = document.createEvent('MouseEvents');
        e2.initMouseEvent(
            'mouseup', true, true, window, 0,
            0, 0, clickX, clickY, false, false,
            false, false, 0, null
        );
        document.elementFromPoint(clickX, clickY).dispatchEvent(e2);

        setTimeout(function () {
            for (var i = 0; i < menus.length; i++) {
                (<HTMLElement>menus[i]).style.opacity = '0';
                (<HTMLElement>menus[i]).style.display = 'unset';
            }
        }, 500);

        // @ts-ignore
        window.xpotify_prevFingerprint = fingerprint;
        // @ts-ignore
        window.xpotify_prevTrackId = trackId;

        // We will ignore (not throw exception) if we can't find trackId, as this is not fatal anyway.

        return trackId;
    }

    export function getVolume() {
        try {
            var element = <HTMLElement>(document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__right .volume-bar .progress-bar .progress-bar__fg')[0]);
            return (100 - Number.parseFloat(element.style.transform.replace(/[^\d.]/g, ''))) / 100.0;
        }
        catch (ex) {
            return 1.0;
            // This is not critical, will ignore if fails.
        }
    }

    async function getNowPlaying() {
        var trackName, trackId, albumId, artistName, artistId, elapsedTime, totalTime, trackFingerprint;
        var isPrevTrackAvailable, isNextTrackAvailable, isPlaying, isSavedToLibrary, volume;
        var success = true;

        try {
            trackName = getTrackName();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get trackName.");
            success = false;
        }
        try {
            trackId = await getTrackId();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get trackId.");
            trackId = "";
            success = false;
        }
        try {
            albumId = getTrackAlbumId();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get albumId.");
            success = false;
        }
        try {
            artistName = getTrackArtist();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get artistName.");
            success = false;
        }
        try {
            artistId = getTrackArtistId();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get artistId.");
            success = false;
        }
        try {
            elapsedTime = getElapsedTime();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get elapsedTime.");
            success = false;
        }
        try {
            totalTime = getTotalTime();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get totalTime.");
            success = false;
        }
        try {
            trackFingerprint = getTrackFingerprint();
        } catch (ex) {
            console.log("Failed to get trackFingerprint.");
            success = false;
        }
        try {
            isPrevTrackAvailable = getIsPrevTrackAvailable();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get isPrevTrackAvailable.");
            success = false;
        }
        try {
            isNextTrackAvailable = getIsNextTrackAvailable();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get isNextTrackAvailable.");
            success = false;
        }
        try {
            isPlaying = getIsPlaying();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get isPlaying.");
            success = false;
        }
        try {
            isSavedToLibrary = getIsSavedToLibrary();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get isSavedToLibrary.");
            success = false;
        }
        try {
            volume = getVolume();
        } catch (ex) {
            window.XpoMusic.Log("Failed to get volume.");
            success = false;
        }

        var data = {
            TrackName: trackName,
            TrackId: trackId,
            AlbumId: albumId,
            ArtistName: artistName,
            ArtistId: artistId,
            ElapsedTime: elapsedTime,
            TotalTime: totalTime,
            TrackFingerprint: trackFingerprint,
            IsPrevTrackAvailable: isPrevTrackAvailable,
            IsNextTrackAvailable: isNextTrackAvailable,
            IsPlaying: isPlaying,
            IsTrackSavedToLibrary: isSavedToLibrary,
            Volume: volume,
            Success: success,
        };

        // window.XpoMusic.Log("getNowPlaying(): sending data: " + JSON.stringify(data));

        return data;
    }

    export function isBackPossible() {
        var backButtonDiv = document.querySelectorAll(".backButtonContainer");

        if (backButtonDiv.length === 0) {
            return true;
        } else if (backButtonDiv[0].classList.contains("backButtonContainer-disabled")) {
            return false;
        } else {
            return true;
        }
    }

    async function sendStatusReport() {
        var data = {
            BackButtonEnabled: isBackPossible(),
            NowPlaying: await getNowPlaying(),
        };

        if (data.NowPlaying.TrackId.toString().trim().length > 0
            || data.NowPlaying.TrackName.toString().trim().length > 0) {
            Common.Action.enableNowPlaying();
        }

        window.XpoMusic.StatusReport(data);
    }

    export function initRegularStatusReport() {
        setInterval(sendStatusReport, 1000);
    }
}