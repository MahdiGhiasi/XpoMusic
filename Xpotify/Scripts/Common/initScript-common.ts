/// <reference path="uiElementModifier.ts" />
/// <reference path="dragDrop.ts" />
/// <reference path="vibrant.ts" />
/// <reference path="color.ts" />
/// <reference path="browserHistory.ts" />
/// <reference path="resize.ts" />
/// <reference path="startupAnimation.ts" />
/// <reference path="requestIntercepter.ts" />

namespace XpotifyScript.Common {

    export function isProVersion(): boolean {
        //@ts-ignore
        return '{{XPOTIFYISPROVERSION}}' === '1';
    }

    export function isLightTheme(): boolean {
        return (document.getElementsByTagName('body')[0].getAttribute('data-xpotifyTheme') === 'light');
    }

    export function init() {
        var errors = "";

        markPageAsInjected();
        initDragDrop();

        errors += injectCss();
        errors += UiElementModifier.createPageTitle();
        errors += UiElementModifier.createBackButton();
        errors += UiElementModifier.createNavBarButtons();
        errors += UiElementModifier.createCompactOverlayButton();
        errors += UiElementModifier.addNowPlayingButton();
        errors += UiElementModifier.addBackgroundClass();
        errors += initNowPlayingBarCheck();
        setInitialPageHash();
        initOnResizeCheck();
        RequestIntercepter.startInterceptingFetch();
        StartupAnimation.init();

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
            Common.Lib.Vibrant.init();
            
            setInterval(function () {
                var url = (<HTMLElement>document.querySelectorAll(".Root__now-playing-bar .now-playing .cover-art-image")[0]).style.backgroundImage.slice(5, -2);
                var lightTheme = isLightTheme();

                if (window["xpotifyNowPlayingIconUrl"] !== url || window["xpotifyNowPlayingLastSetLightTheme"] !== lightTheme) {
                    window["xpotifyNowPlayingIconUrl"] = url;
                    window["xpotifyNowPlayingLastSetLightTheme"] = lightTheme;

                    Color.setNowPlayingBarColor(url, lightTheme);
                }
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
}