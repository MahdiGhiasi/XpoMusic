/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />

namespace XpotifyScript.Common.MouseWheelListener {

    declare var Xpotify: any;
   
    function volumeMouseWheelHandler(e) {
        var { deltaY } = e;
        var currentVolume = StatusReport.getVolume();
        var newVolume = Math.max(Math.min(currentVolume - (deltaY / 1000), 1), 0);

        Action.seekVolume(newVolume);
    }

    function setVolumeBarListener() {
        var volumeBar = document.querySelector("#main > div > div.Root__top-container > div.Root__now-playing-bar > footer > div.now-playing-bar > div.now-playing-bar__right > div > div > div.volume-bar > div");
        volumeBar.addEventListener("mousewheel", (e) => { volumeMouseWheelHandler(e) });
    }

    export function init() {
        setVolumeBarListener();
    }
}