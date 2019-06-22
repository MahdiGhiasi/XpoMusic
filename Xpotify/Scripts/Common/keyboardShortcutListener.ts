namespace XpotifyScript.Common.KeyboardShortcutListener {

    function onKeyUp(e: KeyboardEvent) {
        if (e.ctrlKey && e.which == 'C'.charCodeAt(0)) {
            
        }
    }

    export function init() {
        window.onkeyup = onKeyUp;
    }
}