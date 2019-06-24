/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />

namespace XpotifyScript.Common.KeyboardShortcutListener {

    declare var Xpotify: any;

    function copyToClipboard(str) {
        // From https://hackernoon.com/copying-text-to-clipboard-with-javascript-df4d4988697f

        const el = document.createElement('textarea');
        el.value = str;
        el.setAttribute('readonly', '');
        el.style.position = 'absolute';
        el.style.left = '-9999px';
        document.body.appendChild(el);
        const selected =
            document.getSelection().rangeCount > 0
                ? document.getSelection().getRangeAt(0)
                : false;
        el.select();
        document.execCommand('copy');
        document.body.removeChild(el);
        if (selected) {
            document.getSelection().removeAllRanges();
            document.getSelection().addRange(selected);
        }
    }

    function getKeyboardFocusableElements() {
        return document.querySelectorAll("a.mo-info-name, li[role=button], .Root__main-view nav ul li a, a.search-history-result, input[type=text]");
    }

    function getElementsPreferredForInitialFocus() {
        return document.querySelectorAll("a.mo-info-name, li[role=button], a.search-history-result");
    }

    function getScrollParent(element, includeHidden) {
        let style = getComputedStyle(element);
        const excludeStaticParent = style.position === 'absolute';
        const overflowRegex = includeHidden ? /(auto|scroll|hidden)/ : /(auto|scroll)/;

        if (style.position === 'fixed') {
            return document.body;
        }
        let parent = element.parentElement;
        while (parent) {
            style = getComputedStyle(parent);
            if (excludeStaticParent && style.position === 'static') {
                continue;
            }
            if (overflowRegex.test(style.overflow + style.overflowY + style.overflowX)) {
                return parent;
            }
            parent = parent.parentElement;
        }

        return document.body;
    }


    function scrollIntoViewEx(element: HTMLElement) {
        var bounds = element.getBoundingClientRect();
        if (bounds.top > (window.innerHeight - 200)) {
            element.scrollIntoView(false);

            bounds = element.getBoundingClientRect();
            if (bounds.top > (window.innerHeight - 300)) {
                var parent = <HTMLElement>getScrollParent(element, false);
                parent.scrollTop += 300;
            }
        } else if (bounds.top < 30) {
            element.scrollIntoView(true);

            bounds = element.getBoundingClientRect();
            if (bounds.top < 200) {
                var parent = <HTMLElement>getScrollParent(element, false);
                parent.scrollTop -= 200;
            }
        }
    }

    function keyboardSelect(offset: number) {
        var focusableElements = getKeyboardFocusableElements();
        var activeElement = document.activeElement;
        var idx = -1;

        for (var i = 0; i < focusableElements.length; i++) {
            if (activeElement == focusableElements[i]) {
                idx = i;
                break;
            }
        }

        if (idx == -1) {
            idx = 0;

            var preferredForFocus = getElementsPreferredForInitialFocus()[0];
            for (var i = 0; i < focusableElements.length; i++) {
                if (focusableElements[i] == preferredForFocus) {
                    idx = i;
                    break;
                }
            }
        } else {
            idx += offset;
            idx = Math.min(idx, focusableElements.length - 1);
            idx = Math.max(idx, 0);
        }

        var selectedElement = (<HTMLElement>focusableElements[idx]);

        selectedElement.focus();
        scrollIntoViewEx(selectedElement);
    }

    function onKeyDown(e: KeyboardEvent) {
        // Ignore if focus is on a textbox and the selected key is anything other than arrow up/down keys
        var isInputFocused = document.activeElement.tagName.toLowerCase() === "input";
        if (isInputFocused && e.which != 38 && e.which != 40 && !e.ctrlKey && !e.altKey)
            return;

        var shouldPreventDefault = true;

        // Spotify style shortcuts
        if (e.ctrlKey && e.which == 'N'.charCodeAt(0)) {
            // Ctrl+N -> New Playlist
            Action.navigateToPage("/collection/playlists");

            setTimeout(function () {
                (<HTMLElement>document.querySelectorAll('.Root__main-view .asideButton button')[0]).click();
            }, 250);

        } else if (e.ctrlKey && e.which == 'C'.charCodeAt(0) && !isInputFocused) {
            // Ctrl+C -> Copy
            copyToClipboard(window.location.href.replace("#xpotifyInitialPage", ""));

        } else if (e.ctrlKey && e.which == 'V'.charCodeAt(0) && !isInputFocused) {
            // Ctrl+V -> Paste
            Xpotify.navigateToClipboardUri();

        } else if (e.which == 46) {
            // Delete key -> Delete

        } else if (e.which == ' '.charCodeAt(0)) {
            // Space -> Play/pause
            if (StatusReport.getIsPlaying())
                Action.pause();
            else
                Action.play();

        } else if (e.ctrlKey && e.which == 39) {
            // Ctrl+ArrowRight -> Next Track
            Action.nextTrack();

        } else if (e.ctrlKey && e.which == 37) {
            // Ctrl+ArrowLeft -> Prev Track
            Action.prevTrack();

        } else if (e.ctrlKey && e.shiftKey && e.which == 38) {
            // Ctrl+Shift+ArrowUp -> Max volume
            Action.seekVolume(1);

        } else if (e.ctrlKey && e.shiftKey && e.which == 40) {
            // Ctrl+Shift+ArrowDown -> Mute
            Action.seekVolume(0);

        } else if (e.ctrlKey && e.which == 38) {
            // Ctrl+ArrowUp -> Volume up
            var volume = StatusReport.getVolume();
            var newVolume = Math.min(volume + 0.1, 1);
            Action.seekVolume(newVolume);

        } else if (e.ctrlKey && e.which == 40) {
            // Ctrl+ArrowDown -> Volume down
            var volume = StatusReport.getVolume();
            var newVolume = Math.max(volume - 0.1, 0);
            Action.seekVolume(newVolume);

        } else if (e.ctrlKey && e.which == 'L'.charCodeAt(0)) {
            // Ctrl+L -> Search
            Action.navigateToPage("/search");

        } else if (e.ctrlKey && e.which == 'F'.charCodeAt(0)) {
            // Ctrl+F -> Filter (in Songs and Playlists) -- not present at the moment
            shouldPreventDefault = false;

        } else if (e.altKey && e.which == 37) {
            // Alt+ArrowLeft -> Go back
            Action.goBackIfPossible();

        } else if (e.altKey && e.which == 39) {
            // Alt+ArrowRight -> Go forward
            Action.goForwardIfPossible();

            //} else if (e.which == 13) {
            // Enter -> Play selected row -- not present at the moment
            // Enter key is handled automatically by the PWA

        } else if (e.which == 38 || e.which == 37) {
            // ArrowUp or ArrowLeft -> 
            // If not selected, select the first item in track list
            // and if selected, go one up.
            keyboardSelect(-1);

        } else if (e.which == 40 || e.which == 39) {
            // ArrowDown or ArrowRight -> 
            // If not selected, select the first item in track list
            // and if selected, go one down.
            keyboardSelect(1);

        } else if (e.ctrlKey && e.which == 'P'.charCodeAt(0)) {
            // Ctrl+P -> Settings
            Xpotify.openSettings();
        }
        // Custom shortcuts
        else if (e.ctrlKey && e.which == 'M'.charCodeAt(0)) {
            // Ctrl+M -> Go to mini view
            Xpotify.openMiniView();

        } else if (e.ctrlKey && e.which == ','.charCodeAt(0)) {
            // Ctrl+, -> Go to now playing
            Xpotify.openNowPlaying();

        } else if (e.ctrlKey && e.which == 'Q'.charCodeAt(0)) {
            // Ctrl+Q -> Open playing queue
            Action.navigateToPage("/queue");

        } else if (e.altKey && e.which == '1'.charCodeAt(0)) {
            // Alt+1 -> Home
            Action.navigateToPage("/browse");

        } else if (e.altKey && e.which == '2'.charCodeAt(0)) {
            // Alt+2 -> Search
            Action.navigateToPage("/search");

        } else if (e.altKey && e.which == '3'.charCodeAt(0)) {
            // Alt+3 -> Your Library
            Action.navigateToPage("/collection");

        } else if (e.altKey && e.which == '4'.charCodeAt(0)) {
            // Alt+4 -> Now Playing
            Xpotify.openNowPlaying();

        } else {
            shouldPreventDefault = false;
        }

        if (shouldPreventDefault) {
            e.preventDefault();
            return false;
        }
    }

    export function init() {
        window.onkeydown = onKeyDown;
    }
}