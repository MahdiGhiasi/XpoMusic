/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />

namespace XpoMusicScript.Common.MouseWheelListener {
   
    function volumeMouseWheelHandler(e) {
        var { deltaY } = e;
        var currentVolume = StatusReport.getVolume();
        var newVolume = Math.max(Math.min(currentVolume - (deltaY / 1000), 1), 0);

        Action.seekVolume(newVolume);
    }

    function setVolumeBarListener() {
        var volumeBar = document.querySelector(".Root__top-container .Root__now-playing-bar .now-playing-bar__right__inner .volume-bar > div");

        if (volumeBar === null) {
            // Volume bar not present yet. Will try again later.
            setTimeout(setVolumeBarListener, 1000);
            return;
        }

        volumeBar.addEventListener("mousewheel", (e) => { volumeMouseWheelHandler(e) });
    }

    export function init() {
        setVolumeBarListener();
    }
}