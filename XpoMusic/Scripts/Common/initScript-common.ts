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
/// <reference path="themedScrollbar.ts" />
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

    export function getCurrentZoomLevel(): number {
        // @ts-ignore
        return (screen.deviceXDPI / screen.logicalXDPI);
    }

    export function init() {
        if (document.querySelectorAll("#main").length === 0
            && XpoMusic.isWebPlayerBackupEnabled()) {
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

        XpoMusic.log("Removing top right 'Upgrade' button if present...");
        removeTopUpgradeButton();

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

        XpoMusic.log("Initializing themed scrollbars...");
        initThemedScrollbars();

        XpoMusic.log("Initializing StartupAnimation...");
        StartupAnimation.init();

        // @ts-ignore
        if (window.XpoMusicScript === undefined)
            // @ts-ignore
            window.XpoMusicScript = XpoMusicScript;

        XpoMusic.log("Common.init() finished. errors = '" + errors + "'");
        return errors;
    }

    function initThemedScrollbars() {
        if (document.querySelectorAll(".main-view-container__scroll-node").length == 0
            || document.querySelectorAll(".Rootlist__playlists-scroll-node").length == 0) {
            setTimeout(initThemedScrollbars, 200);
            return;
        }

        var adsBar = document.querySelector(".Root__main-view--has-ads > div:nth-child(3)");
        ThemedScrollbar.initScrollbar(".main-view-container__scroll-node", 1000, function (el) {
            if (adsBar != null) {
                el.style.bottom = (adsBar.clientHeight) + "px";
            } else {
                el.style.bottom = "0px";
            }

            var headerIsBlurred = !(<HTMLElement>document.querySelector('.Root__top-bar header div:nth-child(1)')).classList.contains('xpo-transparent-top-bar-background');
            if (headerIsBlurred)
                el.style.top = "85px";
            else
                el.style.top = (32.0 / getCurrentZoomLevel()) + "px";
        });

        ThemedScrollbar.initScrollbar(".Rootlist__playlists-scroll-node", 1000);
    }

    function removeTopUpgradeButton() {
        try {
            var itemCandidate = document.querySelectorAll('.main-view-container__scroll-node-child > div > button');
            if (itemCandidate.length == 0) {
                return;
            } else if (itemCandidate.length > 1) {
                XpoMusic.log("Too many candidates for removeTopUpgradeButton. Will not remove.");
                return;
            }

            var item = itemCandidate[0].parentElement;
            var css = "." + item.classList[0] + " { display: none; }";
            injectCustomCssContent(css);
        } catch (ex) {
            XpoMusic.log('RemoveTopUpgradeButton failed: ' + ex);
        }
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
        var css = '{{XPOTIFYCSSBASE64CONTENT}}';
        return injectCustomCssContent(atob(css));
    }

    function injectCustomCssContent(content) {
        try {
            var style = document.createElement('style');
            document.getElementsByTagName('head')[0].appendChild(style);
            style.type = 'text/css';
            style.appendChild(document.createTextNode(content));
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

    /*
    function forceRedrawItem1(item: HTMLElement) {
        item.style.paddingRight = "0.1px";
        setTimeout(function () {
            item.style.paddingRight = "0px";
        }, 100);
    }


    function forceRedrawItem2(item: HTMLElement) {
        item.style.opacity = "0.999";
        setTimeout(function () {
            item.style.opacity = "1";
        }, 100);
    }

    function forceRedrawScreen() {
        forceRedrawItem1(document.querySelector('.Root__now-playing-bar'));

        var navs = document.querySelectorAll('nav');
        for (var i = 0; i < navs.length; i++) {
            forceRedrawItem1(navs[i]);
        }

        var imgs = document.querySelectorAll('.cover-art-image, img');
        for (var i = 0; i < imgs.length; i++) {
            forceRedrawItem2(<HTMLElement>imgs[i]);
        }
    }
    */

    function InjectTopBarCustomStyles() {
        var topBarBackground = document.querySelectorAll('.Root__top-bar header > div:nth-child(1)');
        if (topBarBackground.length !== 1)
            return;

        var element = <HTMLElement>(topBarBackground[0]);
        element.style.backgroundColor = "rgba(18, 18, 18, 0.6)";
        element.style.setProperty('backdrop-filter', 'blur(30px)');
        element.style.setProperty('-webkit-backdrop-filter', 'blur(30px)');
    }

    function expandPivotInTopBar() {
        try {
            var childNodesList = document.querySelectorAll('.Root__top-bar nav ul li ul li');
            document.querySelectorAll('.xpo-modified-pivot-item').forEach(x => x.parentNode.removeChild(x));

            if (childNodesList.length === 0)
                return;

            var childNodes = Array.prototype.slice.call(childNodesList);
            var correctNodes = Array.prototype.slice.call(document.querySelectorAll('.Root__top-bar nav ul li')).filter(el => !childNodes.includes(el));
            var correctClassName = correctNodes[0].className;
            childNodes.forEach(x => x.className = correctClassName);
            var correctAClassName = correctNodes[0].querySelector('a').className;
            childNodes.forEach(x => x.querySelector('a').className = correctAClassName);
            var containingLiElement = childNodes[0].closest('ul').closest('li');
            var correctUl = containingLiElement.closest('ul');
            childNodes.forEach(x => {
                var newNode = <HTMLElement>correctUl.appendChild(x.cloneNode(true));
                newNode.classList.add('xpo-modified-pivot-item');

                var newA = newNode.querySelector('a');
                var href = newA.href;
                newA.href = "#";

                newA.onclick = function () {
                    Common.Action.navigateToPage(href);
                    return false;
                };
            });
            containingLiElement.style.display = 'none';
        }
        catch (ex) {
            XpoMusic.log(ex);
        }
    }

    var pagePrevLocation = "";
    var redrawFixPrevLocation = "";
    function periodicPageCheck() {
        try {
            if (document.querySelectorAll(".tracklist").length > 0) {
                TracklistExtended.initTracklistMod();
            }

            InjectTopBarCustomStyles();
            expandPivotInTopBar();

            //if (isLightTheme()) {
            //    // Fix Edge glitches where css invert() stops working sometimes after page change.
            //    if (redrawFixPrevLocation !== window.location.href) {
            //        setTimeout(forceRedrawScreen, 700);
            //        setTimeout(forceRedrawScreen, 1250);
            //        redrawFixPrevLocation = window.location.href;
            //    }
            //}

            if (pagePrevLocation !== window.location.href) {
                if (window.location.href.startsWith('https://open.spotify.com/search')) {
                    (<HTMLElement>document.querySelector('.Root__top-bar input')).focus();
                    (<HTMLElement>document.querySelector('.Root__main-view')).classList.remove('xpo-collection-page');
                    (<HTMLElement>document.querySelector('.Root__top-bar')).classList.remove('xpo-top-bar-hidden');
                    (<HTMLElement>document.querySelector('.Root__top-bar header div:nth-child(1)')).classList.remove('xpo-transparent-top-bar-background');

                } else if (window.location.href.startsWith('https://open.spotify.com/artist')
                    || window.location.href.startsWith('https://open.spotify.com/album')
                    || window.location.href.startsWith('https://open.spotify.com/show')
                    || window.location.href.startsWith('https://open.spotify.com/playlist')
                    || window.location.href.startsWith('https://open.spotify.com/collection/tracks')) {

                    (<HTMLElement>document.querySelector('.Root__main-view')).classList.remove('xpo-collection-page');
                    (<HTMLElement>document.querySelector('.Root__top-bar')).classList.add('xpo-top-bar-hidden');
                    (<HTMLElement>document.querySelector('.Root__top-bar header div:nth-child(1)')).classList.add('xpo-transparent-top-bar-background');

                } else if (window.location.href.startsWith('https://open.spotify.com/collection')) {
                    (<HTMLElement>document.querySelector('.Root__main-view')).classList.add('xpo-collection-page');
                    (<HTMLElement>document.querySelector('.Root__top-bar')).classList.remove('xpo-top-bar-hidden');
                    (<HTMLElement>document.querySelector('.Root__top-bar header div:nth-child(1)')).classList.remove('xpo-transparent-top-bar-background');

                } else {
                    (<HTMLElement>document.querySelector('.Root__main-view')).classList.remove('xpo-collection-page');
                    (<HTMLElement>document.querySelector('.Root__top-bar')).classList.remove('xpo-top-bar-hidden');
                    (<HTMLElement>document.querySelector('.Root__top-bar header div:nth-child(1)')).classList.add('xpo-transparent-top-bar-background');
                }

                pagePrevLocation = window.location.href;
            }
        }
        catch (ex) {
            XpoMusic.log(ex);
        }
    }

    function initPeriodicPageCheck() {
        setInterval(periodicPageCheck, 1000);

        // Fix Edge glitches where css invert() stops working sometimes after page change.
        //if (isLightTheme()) {
        //    setInterval(forceRedrawScreen, 2000);
        //}
    }
}
