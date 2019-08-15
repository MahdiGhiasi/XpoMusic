/// <reference path="uiElementModifier.ts" />
/// <reference path="dragDrop.ts" />
/// <reference path="../Lib/vibrant.ts" />
/// <reference path="../Lib/focus-visible.ts" />
/// <reference path="color.ts" />
/// <reference path="browserHistory.ts" />
/// <reference path="resize.ts" />
/// <reference path="startupAnimation.ts" />
/// <reference path="requestIntercepter.ts" />
/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />
/// <reference path="pageTitleFinder.ts" />
/// <reference path="keyboardShortcutListener.ts" />
/// <reference path="mouseWheelListener.ts" />
/// <reference path="web-player-backup.ts" />


namespace XpoMusicScript.Common {

    declare var XpoMusic: any;

    export function isProVersion(): boolean {
        //@ts-ignore
        return '{{XPOTIFYISPROVERSION}}' === '1';
    }

    export function getDeviceName(): string {
        return '{{DEVICENAME}}';
    }

    export function getAppName(): string {
        return isProVersion() ? 'Xpo Music Pro' : 'Xpo Music';
    }

    export function isLightTheme(): boolean {
        return (document.getElementsByTagName('body')[0].getAttribute('data-xpotifyTheme') === 'light');
    }

    export function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    export function init() {
        if (document.querySelectorAll("#main").length === 0
            && XpoMusic.isWebPlayerBackupEnabled()
            && XpoMusic.getOSBuildVersion() < 17763) {
            // This is for 1803 and below. (v1809 == 17763)
            XpoMusic.log("#main is missing. Will try runWebPlayerBackup()");
            try {
                WebPlayerBackup.runWebPlayerBackup();
            } catch (ex) {
                XpoMusic.log("runWebPlayerBackup() failed: " + ex);
            }
        }

        var errors = "";

        markPageAsInjected();
        initDragDrop();

        XpoMusic.log("Initializing UiElemetModifier stuff...");
        errors += injectCss();
        errors += UiElementModifier.createBackButton();
        errors += UiElementModifier.createNavBarButtons();
        errors += UiElementModifier.createCompactOverlayButton();
        errors += UiElementModifier.addNowPlayingButton();
        errors += UiElementModifier.addBackgroundClass();
        errors += initNowPlayingBarCheck();

        XpoMusic.log("Setting page hash and initializing resize and periodic checks...");
        setInitialPageHash();
        initOnResizeCheck();
        initPeriodicPageCheck();

        XpoMusic.log("Initializing libraries...");
        Lib.FocusVisible.init();

        XpoMusic.log("Initializing MouseWheelListener...");
        MouseWheelListener.init();

        XpoMusic.log("Initializing KeyboardShortcutListener...");
        KeyboardShortcutListener.init();

        XpoMusic.log("Initializing RequestIntercepter...");
        RequestIntercepter.startInterceptingFetch();

        XpoMusic.log("Initializing StatusReport...");
        StatusReport.initRegularStatusReport();

        XpoMusic.log("Initializing StartupAnimation...");
        StartupAnimation.init();

        // @ts-ignore
        if (window.XpoMusicScript === undefined)
            // @ts-ignore
            window.XpoMusicScript = XpoMusicScript;

        XpoMusic.log("Common.init() finished. errors = '" + errors + "'");
        return errors;
    }

    function markPageAsInjected() {
        var body = document.getElementsByTagName('body')[0];
        body.setAttribute('data-scriptinjection', '1');
    }

    function initDragDrop() {
        var body = document.getElementsByTagName('body')[0];
        body.ondrop = DragDrop.drop;
        body.ondragover = DragDrop.allowDrop;
    }

    function injectCss() {
        try {
            var css = '{{XPOTIFYCSSBASE64CONTENT}}';
            var style = document.createElement('style');
            document.getElementsByTagName('head')[0].appendChild(style);
            style.type = 'text/css';
            style.appendChild(document.createTextNode(atob(css)));
        }
        catch (ex) {
            return "injectCssFailed,";
        }
        return "";
    }

    function initNowPlayingBarCheck() {
        // Check and set now playing bar background color when now playing album art changes
        try {
            Lib.Vibrant.init();
            
            setInterval(function () {
                try {
                    var url = (<HTMLElement>document.querySelectorAll(".Root__now-playing-bar .now-playing .cover-art-image")[0]).style.backgroundImage.slice(5, -2);
                    var lightTheme = isLightTheme();

                    if (window["xpotifyNowPlayingIconUrl"] !== url || window["xpotifyNowPlayingLastSetLightTheme"] !== lightTheme) {
                        window["xpotifyNowPlayingIconUrl"] = url;
                        window["xpotifyNowPlayingLastSetLightTheme"] = lightTheme;

                        Color.setNowPlayingBarColor(url, lightTheme);
                    }
                }
                catch (ex) { }
            }, 1000);
        } catch (ex) {
            return "nowPlayingBarColorPollInitFailed,";
        }

        return "";
    }

    function setInitialPageHash() {
        setTimeout(function () {
            window.location.hash = "xpotifyInitialPage";

            setInterval(function () {
                var backButtonDivC = document.querySelectorAll(".backButtonContainer");
                if (backButtonDivC.length === 0) {
                    return;
                }
                var backButtonDiv = <HTMLElement>backButtonDivC[0];

                if (BrowserHistory.canGoBack()) {
                    backButtonDiv.classList.remove("backButtonContainer-disabled");
                } else {
                    backButtonDiv.classList.add("backButtonContainer-disabled");
                }
            }, 500);
        }, 1000);
    }

    function initOnResizeCheck() {
        window.addEventListener("resize", Resize.onResize, true);
        setInterval(Resize.onResize, 5000); // Sometimes an OnResize is necessary when users goes to a new page.
    }

    function periodicPageCheck() {
        try {
            if (document.querySelectorAll(".tracklist").length > 0) {
                TracklistExtended.initTracklistMod();
            }
        }
        catch (ex) {
            XpoMusic.log(ex);
        }
    }

    function initPeriodicPageCheck() {
        setInterval(periodicPageCheck, 1000);
    }
}
