
function getTextContent(element, index) {
    var e = document.querySelectorAll(element);

    if (e.length > index) {
        return (e[index]).innerText;
    } else {
        throw "Couldn't find element.";
    }
}

function getTrackFingerprint() {
    return getTextContent('.Root__now-playing-bar .now-playing-bar__left', 0);
}

function getTrackName() {
    return getTextContent('.Root__now-playing-bar .now-playing-bar__left .track-info .track-info__name', 0);
}

function getTrackArtist() {
    return getTextContent('.Root__now-playing-bar .now-playing-bar__left .track-info .track-info__artists', 0);
}

function getTrackAlbumId() {
    var artistUri = ((document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__left .track-info .track-info__name a")[0])).href;
    return artistUri.substring(artistUri.lastIndexOf('/') + 1);
}

function getTrackArtistId() {
    var artistUri = ((document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__left .track-info .track-info__artists a")[0])).href;
    return artistUri.substring(artistUri.lastIndexOf('/') + 1);
}

function timeStringToMilliseconds(time) {
    if (time.match('^[0-9]+\:[0-9][0-9]?$') === null)
        throw "Invalid time format";

    var parts = time.split(':');

    return (Number.parseInt(parts[0]) * 60 + Number.parseInt(parts[1])) * 1000;
}

function getElapsedTime() {
    var time = getTextContent('.Root__now-playing-bar .now-playing-bar__center .playback-bar .playback-bar__progress-time', 0);
    return timeStringToMilliseconds(time);
}

function getTotalTime() {
    var time = getTextContent('.Root__now-playing-bar .now-playing-bar__center .playback-bar .playback-bar__progress-time', 1);
    return timeStringToMilliseconds(time);
}

function getIsSavedToLibrary() {
    var e = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left .spoticon-heart-active-16');
    return (e.length > 0);

    // This is not fatal, so we don't throw exception if this fails.
}

function getIsPlaying() {
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

function getIsPrevTrackAvailable() {
    var prevButton = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16');
    if (prevButton.length === 0) {
        throw "Can't find prevTrackButton";
    }
    if (((prevButton[0])).classList.contains("control-button--disabled")) {
        return false;
    }
    return true;
}

function getIsNextTrackAvailable() {
    var nextButton = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__center .spoticon-skip-forward-16');
    if (nextButton.length === 0) {
        throw "Can't find nextTrackButton";
    }
    if (((nextButton[0])).classList.contains("control-button--disabled")) {
        return false;
    }
    return true;
}

function getTrackId() {
    var fingerprint = getTrackFingerprint();

    if (window.xpotify_prevFingerprint === fingerprint) {
        return window.xpotify_prevTrackId;
    }

    var menus = document.querySelectorAll("nav[role=menu]");
    for (var i = 0; i < menus.length; i++) {
        (menus[i]).style.display = 'none';
    }

    var tracks = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left .track-info .track-info__name a');

    if (tracks.length === 0)
        return "";

    var element = tracks[0];

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
            (menus[i]).style.opacity = '0';
            (menus[i]).style.display = 'unset';
        }
    }, 500);

    window.xpotify_prevFingerprint = fingerprint;
    window.xpotify_prevTrackId = trackId;

    // We will ignore (not throw exception) if we can't find trackId, as this is not fatal anyway.

    return trackId;
}

function getVolume() {
    try {
        var element = (document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__right .volume-bar .progress-bar .progress-bar__fg')[0]);
        return (100 - Number.parseFloat(element.style.transform.replace(/[^\d.]/g, ''))) / 100.0;
    }
    catch (ex) {
        return 1.0;
        // This is not critical, will ignore if fails.
    }
}

function getNowPlaying() {
    var trackName, trackId, albumId, artistName, artistId, elapsedTime, totalTime, trackFingerprint;
    var isPrevTrackAvailable, isNextTrackAvailable, isPlaying, isSavedToLibrary, volume;
    var success = true;

    try {
        trackName = getTrackName();
    } catch (ex) {
        console.log("Failed to get trackName.");
        success = false;
    }
    try {
        trackId = getTrackId();
    } catch (ex) {
        console.log("Failed to get trackId.");
        trackId = ex.toString();
        success = false;
    }
    try {
        albumId = getTrackAlbumId();
    } catch (ex) {
        console.log("Failed to get albumId.");
        success = false;
    }
    try {
        artistName = getTrackArtist();
    } catch (ex) {
        console.log("Failed to get artistName.");
        success = false;
    }
    try {
        artistId = getTrackArtistId();
    } catch (ex) {
        console.log("Failed to get artistId.");
        success = false;
    }
    try {
        elapsedTime = getElapsedTime();
    } catch (ex) {
        console.log("Failed to get elapsedTime.");
        success = false;
    }
    try {
        totalTime = getTotalTime();
    } catch (ex) {
        console.log("Failed to get totalTime.");
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
        console.log("Failed to get isPrevTrackAvailable.");
        success = false;
    }
    try {
        isNextTrackAvailable = getIsNextTrackAvailable();
    } catch (ex) {
        console.log("Failed to get isNextTrackAvailable.");
        success = false;
    }
    try {
        isPlaying = getIsPlaying();
    } catch (ex) {
        console.log("Failed to get isPlaying.");
        success = false;
    }
    try {
        isSavedToLibrary = getIsSavedToLibrary();
    } catch (ex) {
        console.log("Failed to get isSavedToLibrary.");
        success = false;
    }
    try {
        volume = getVolume();
    } catch (ex) {
        console.log("Failed to get volume.");
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

    return data;
}

function isBackPossible() {
    var backButtonDiv = document.querySelectorAll(".backButtonContainer");

    if (backButtonDiv.length === 0) {
        return true;
    } else if (backButtonDiv[0].classList.contains("backButtonContainer-disabled")) {
        return false;
    } else {
        return true;
    }
}

function statusReport() {
    return JSON.stringify({
        BackButtonEnabled: isBackPossible(),
        NowPlaying: getNowPlaying(),
    });
}

statusReport();




