﻿namespace XpoMusicScript.Common.TracklistExtended {

    export function injectTrackIdsToTrackList() {

        // Hide menus
        var menus = document.querySelectorAll("nav[role=menu]");
        for (var i = 0; i < menus.length; i++) {
            (<HTMLElement>menus[i]).style.display = 'none';
        }

        var tracks = document.querySelectorAll(".Root__main-view .tracklist-container .tracklist-row")

        var counter = 0;

        // Find track url for each tracklist-row and add it to data-trackid
        for (var i = 0; i < tracks.length; i++) {

            var element = tracks[i];

            if (element.getAttribute('data-trackid') !== null)
                continue;

            counter++;

            var e = element.ownerDocument.createEvent('MouseEvents');
            e.initMouseEvent('contextmenu', true, true,
                element.ownerDocument.defaultView, 1, 0, 0, 0, 0, false,
                false, false, false, 2, null);
            element.dispatchEvent(e);

            var tas = document.querySelectorAll("nav[role=menu] textarea");
            for (var j = 0; j < tas.length; j++) {
                if (tas[j].textContent.toLowerCase().startsWith("https://open.spotify.com/track/")) {
                    element.setAttribute('data-trackid', tas[j].textContent.substr("https://open.spotify.com/track/".length));
                }
            }
        }

        window.XpoMusic.Log(counter.toString() + " trackIds extracted.");

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


        // Don't let menu to be visible
        setTimeout(function () {
            for (var i = 0; i < menus.length; i++) {
                (<HTMLElement>menus[i]).style.opacity = '0';
                (<HTMLElement>menus[i]).style.display = 'unset';
            }
        }, 1000);
    }

    export async function setAddRemoveButtons() {
        var tracklist = document.querySelectorAll(".Root__main-view .tracklist");
        var rows = tracklist[0].querySelectorAll('.tracklist-row');
        var trackIds = [];
        for (var i = 0; i < rows.length; i++) {
            if (rows[i].classList.contains("tracklistSongExistsInLibrary") || rows[i].classList.contains("tracklistSongNotExistsInLibrary")) {
                // Don't ask server for the items we already know the status
                continue;
            }

            var attr = rows[i].getAttribute('data-trackid');
            if (attr !== undefined && attr.length > 0) {
                if (trackIds.indexOf(attr) < 0) { 
                    // Add each track id once
                    trackIds.push(attr);
                }
            }
        }

        var api = new SpotifyApi.Library();
        var result = await api.isTracksSaved(trackIds);

        result.forEach(function (item: boolean, index: number) {
            var element = document.querySelectorAll('.tracklist-row[data-trackid="' + trackIds[index] + '"]');
            if (element.length < 1)
                return;

            for (var i = 0; i < element.length; i++) {
                if (item) {
                    (<HTMLElement>element[i]).classList.remove('tracklistSongNotExistsInLibrary');
                    (<HTMLElement>element[i]).classList.add('tracklistSongExistsInLibrary');
                } else {
                    (<HTMLElement>element[i]).classList.remove('tracklistSongExistsInLibrary');
                    (<HTMLElement>element[i]).classList.add('tracklistSongNotExistsInLibrary');
                }
            }
        });
    }

    export async function initTracklistMod() {
        var tracklist = document.querySelectorAll(".Root__main-view .tracklist");

        if (tracklist.length === 0)
            return; // No tracklist found

        if (window.location.href.startsWith("https://open.spotify.com/collection/tracks"))
            return; // No tracklist add remove button for Liked Songs page (they're all liked by definition!)

        if (tracklist[0].querySelectorAll(".tracklist-row").length === 0)
            return; // List not loaded yet

        if (!UiElementModifier.createTrackListAddRemoveButtons())
            return; // No new elements present

        window.XpoMusic.Log('Some new track list elements found. will try to find track ids.');

        await Common.sleep(500); // Make sure animations and UI is completely loaded (avoid lag)
        injectTrackIdsToTrackList();
        await setAddRemoveButtons(); 
    }

}
