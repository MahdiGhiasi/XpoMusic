var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var BrowserHistory;
        (function (BrowserHistory) {
            function canGoBack() {
                return window.location.hash !== "#xpotifyInitialPage";
            }
            BrowserHistory.canGoBack = canGoBack;
            function goBack() {
                if (canGoBack()) {
                    window.history.go(-1);
                }
            }
            BrowserHistory.goBack = goBack;
        })(BrowserHistory = Common.BrowserHistory || (Common.BrowserHistory = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var UiInjector;
        (function (UiInjector) {
            function injectNavbarDownButton(button) {
                var navbar = document.querySelectorAll(".NavBarFooter");
                var sessionInfo = document.querySelectorAll(".sessionInfo");
                if (navbar.length === 0 || sessionInfo.length === 0) {
                    setTimeout(function () {
                        injectNavbarDownButton(button);
                    }, 500);
                }
                else {
                    navbar[0].insertBefore(button, sessionInfo[0]);
                }
            }
            UiInjector.injectNavbarDownButton = injectNavbarDownButton;
            function injectNowPlayingRightButton(button) {
                var extraControlsBar = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__right__inner .ExtraControls');
                if (extraControlsBar.length === 0) {
                    setTimeout(function () {
                        injectNowPlayingRightButton(button);
                    }, 500);
                }
                else {
                    extraControlsBar[0].prepend(button);
                }
            }
            UiInjector.injectNowPlayingRightButton = injectNowPlayingRightButton;
            function injectNowPlayingNavBarButton(button) {
                var extraControlsBar = document.querySelectorAll('.Root__top-container .navBar ul');
                if (extraControlsBar.length === 0) {
                    setTimeout(function () {
                        injectNowPlayingNavBarButton(button);
                    }, 500);
                }
                else {
                    extraControlsBar[0].append(button);
                }
            }
            UiInjector.injectNowPlayingNavBarButton = injectNowPlayingNavBarButton;
        })(UiInjector = Common.UiInjector || (Common.UiInjector = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var Color;
        (function (Color) {
            function setNowPlayingBarColor(url, lightTheme) {
                try {
                    var img = document.createElement('img');
                    img.crossOrigin = 'anonymous';
                    img.setAttribute('src', url);
                    img.addEventListener('load', function () {
                        try {
                            var vibrant = new Vibrant(img);
                            var swatches = vibrant.swatches();
                            var opacity = 0.25;
                            var rgb = swatches.Muted.getRgb();
                            if (swatches.Muted.getPopulation() < swatches.Vibrant.getPopulation()) {
                                rgb = swatches.Vibrant.getRgb();
                            }
                            if (lightTheme) {
                                rgb[0] = 255 - rgb[0];
                                rgb[1] = 255 - rgb[1];
                                rgb[2] = 255 - rgb[2];
                                opacity = 0.3;
                            }
                            document.querySelectorAll(".Root__now-playing-bar .now-playing-bar")[0].style.backgroundColor = "rgba(" + rgb[0] + ", " + rgb[1] + ", " + rgb[2] + ", " + opacity + ")";
                        }
                        catch (ex2) {
                            console.log("setNowPlayingBarColor failed (2)");
                            console.log(ex2);
                        }
                    });
                }
                catch (ex) {
                    console.log("setNowPlayingBarColor failed (1)");
                    console.log(ex);
                }
            }
            Color.setNowPlayingBarColor = setNowPlayingBarColor;
            function addXpotifyClassToBackground(retryCount) {
                if (retryCount < 0)
                    return;
                var rootElement = document.querySelectorAll(".Root__top-container");
                if (rootElement.length === 0) {
                    setTimeout(function () {
                        addXpotifyClassToBackground(retryCount - 1);
                    }, 250);
                }
                else if (rootElement[0].previousSibling.style.backgroundImage === "") {
                    setTimeout(function () {
                        addXpotifyClassToBackground(retryCount - 1);
                    }, 250);
                }
                else {
                    rootElement[0].previousSibling.classList.add('xpotifyBackground');
                }
            }
            Color.addXpotifyClassToBackground = addXpotifyClassToBackground;
        })(Color = Common.Color || (Common.Color = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
/// <reference path="browserHistory.ts" />
/// <reference path="uiInjector.ts" />
/// <reference path="color.ts" />
var XpoMusicScript;
/// <reference path="browserHistory.ts" />
/// <reference path="uiInjector.ts" />
/// <reference path="color.ts" />
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var UiElementModifier;
        (function (UiElementModifier) {
            function createPageTitle() {
                try {
                    var body = document.getElementsByTagName('body')[0];
                    var titleDiv = document.createElement('div');
                    titleDiv.classList.add("xpotifyWindowTitle");
                    titleDiv.innerText = Common.getAppName();
                    body.appendChild(titleDiv);
                }
                catch (ex) {
                    XpoMusic.log("injectTitleFailed");
                    XpoMusic.log(ex.toString());
                    return "injectTitleFailed,";
                }
                return "";
            }
            UiElementModifier.createPageTitle = createPageTitle;
            function createBackButton() {
                try {
                    var body = document.getElementsByTagName('body')[0];
                    var backButtonDiv = document.createElement('div');
                    backButtonDiv.classList.add("backButtonContainer");
                    backButtonDiv.classList.add("backButtonContainer-disabled");
                    backButtonDiv.innerHTML = "<span>&#xE72B;</span>";
                    backButtonDiv.onclick = Common.BrowserHistory.goBack;
                    body.appendChild(backButtonDiv);
                }
                catch (ex) {
                    XpoMusic.log("injectBackFailed");
                    XpoMusic.log(ex.toString());
                    return "injectBackFailed,";
                }
                return "";
            }
            UiElementModifier.createBackButton = createBackButton;
            function createNavBarButtons() {
                try {
                    var pinToStartButton = document.createElement('div');
                    pinToStartButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifypintostart-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE718;</span></div>'
                        + '<span class="navbar-link__text">Pin this page to Start</span></div></a></div>';
                    pinToStartButton.querySelector('a').onclick = function () {
                        XpoMusic.pinToStart();
                        return false;
                    };
                    var settingsButton = document.createElement('div');
                    settingsButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE115;</span></div>'
                        + '<span class="navbar-link__text">Settings</span></div></a></div>';
                    settingsButton.querySelector('a').onclick = function () {
                        XpoMusic.openSettings();
                        return false;
                    };
                    var aboutButton = document.createElement('div');
                    aboutButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE946;</span></div>'
                        + '<span class="navbar-link__text">About</span></div></a></div>';
                    aboutButton.querySelector('a').onclick = function () {
                        XpoMusic.openAbout();
                        return false;
                    };
                    var donateButton = document.createElement('div');
                    donateButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#">'
                        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE719;</span></div>'
                        + '<span class="navbar-link__text">Donate</span></div></a></div>';
                    donateButton.querySelector('a').onclick = function () {
                        XpoMusic.openDonate();
                        return false;
                    };
                    Common.UiInjector.injectNavbarDownButton(pinToStartButton);
                    Common.UiInjector.injectNavbarDownButton(settingsButton);
                    //UiInjector.injectNavbarDownButton(aboutButton);
                    //if (!Common.isProVersion())
                    //    UiInjector.injectNavbarDownButton(donateButton);
                }
                catch (ex) {
                    XpoMusic.log("injectNavBarFooterFailed");
                    XpoMusic.log(ex.toString());
                    return "injectNavBarFooterFailed,";
                }
                return "";
            }
            UiElementModifier.createNavBarButtons = createNavBarButtons;
            function createCompactOverlayButton() {
                // Inject compact overlay button to now playing bar
                try {
                    var compactOverlayButton = document.createElement('div');
                    compactOverlayButton.classList.add("CompactOverlayButton");
                    compactOverlayButton.classList.add("CompactOverlayButton-disabled");
                    compactOverlayButton.innerHTML = '<a style="border-bottom: 0px;" href="#"><button title="Mini view" class="control-button">'
                        + '<div style="font-family: Segoe MDL2 Assets; position:relative; cursor: default;">'
                        + '<div style="left: 6px; top: -3px; font-size: 19px; position: absolute;">&#xE7FB;</div>'
                        + '<div style="left: 12px; top: -6px; font-size: 9px; position: absolute;">&#xEB9F;</div>'
                        + '</div></button></a>';
                    compactOverlayButton.querySelector('a').onclick = function () {
                        XpoMusic.openMiniView();
                        return false;
                    };
                    Common.UiInjector.injectNowPlayingRightButton(compactOverlayButton);
                }
                catch (ex) {
                    XpoMusic.log("injectCompactOverlayFailed");
                    XpoMusic.log(ex.toString());
                    return "injectCompactOverlayFailed,";
                }
                return "";
            }
            UiElementModifier.createCompactOverlayButton = createCompactOverlayButton;
            function addNowPlayingButton() {
                try {
                    var nowPlayingButton = document.createElement('div');
                    nowPlayingButton.innerHTML = '<li class="navBar-group"><div class="GlueDropTarget">'
                        + '<div class="navBar-item navBar-item--with-icon-left nowPlaying-navBar-item nowPlaying-navBar-item-disabled">'
                        + '<a class="link-subtle navBar-link ellipsis-one-line" aria-label="Now Playing" href="#"><div class="navBar-link-text-with-icon-wrapper">'
                        + '<div class="icon NavBar__icon nowPlaying-icon"></div>'
                        + '<span class="navbar-link__text">Now Playing</span></div></a></div></div></li>';
                    nowPlayingButton.querySelector('a').onclick = function () {
                        XpoMusic.openNowPlaying();
                        return false;
                    };
                    Common.UiInjector.injectNowPlayingNavBarButton(nowPlayingButton);
                }
                catch (ex) {
                    XpoMusic.log("addNowPlayingButtonFailed");
                    XpoMusic.log(ex.toString());
                    return "addNowPlayingButtonFailed,";
                }
                return "";
            }
            UiElementModifier.addNowPlayingButton = addNowPlayingButton;
            function addBackgroundClass() {
                // Find and add necessary class to background div
                try {
                    setTimeout(function () {
                        Common.Color.addXpotifyClassToBackground(12);
                    }, 250);
                    // Sometimes the PWA changes the background element on loading, causing the 
                    // background class to be removed. We'll do this again after a few seconds 
                    // to make sure that does not happen.
                    setTimeout(function () {
                        Common.Color.addXpotifyClassToBackground(0);
                    }, 4000);
                }
                catch (ex) {
                    XpoMusic.log("findBackgroundDivFailed");
                    XpoMusic.log(ex.toString());
                    return "findBackgroundDivFailed,";
                }
                return "";
            }
            UiElementModifier.addBackgroundClass = addBackgroundClass;
            function createTrackListAddRemoveButtons() {
                try {
                    var count = 0;
                    var tracks = document.querySelectorAll(".tracklist .tracklist-row");
                    for (var i = 0; i < tracks.length; i++) {
                        if (tracks[i].getAttribute("data-xpotify-addremovebuttonsadded") !== null) {
                            continue;
                        }
                        var addSongDiv = document.createElement('button');
                        addSongDiv.classList.add("tracklist-middle-align");
                        addSongDiv.classList.add("control-button");
                        addSongDiv.classList.add("spoticon-add-16");
                        addSongDiv.classList.add("trackListAddRemoveSongButton");
                        addSongDiv.classList.add("trackListAddSongButton");
                        addSongDiv.setAttribute("title", "Add to your Liked Songs");
                        addSongDiv.addEventListener('click', function (e) {
                            return __awaiter(this, void 0, void 0, function* () {
                                var row = e.target.closest('.tracklist-row');
                                var trackId = row.getAttribute("data-trackid");
                                row.classList.add('tracklistSongExistsInLibrary');
                                row.classList.remove('tracklistSongNotExistsInLibrary');
                                var libraryApi = new XpoMusicScript.SpotifyApi.Library();
                                var result = yield libraryApi.saveTrack(trackId);
                                if (!result) {
                                    row.classList.add('tracklistSongNotExistsInLibrary');
                                    row.classList.remove('tracklistSongExistsInLibrary');
                                }
                            });
                        });
                        var removeSongDiv = document.createElement('button');
                        removeSongDiv.classList.add("tracklist-middle-align");
                        removeSongDiv.classList.add("control-button");
                        removeSongDiv.classList.add("spoticon-added-16");
                        removeSongDiv.classList.add("control-button--active");
                        removeSongDiv.classList.add("trackListAddRemoveSongButton");
                        removeSongDiv.classList.add("trackListRemoveSongButton");
                        removeSongDiv.setAttribute("title", "Remove from your Liked Songs");
                        removeSongDiv.addEventListener('click', function (e) {
                            return __awaiter(this, void 0, void 0, function* () {
                                var row = e.target.closest('.tracklist-row');
                                var trackId = row.getAttribute("data-trackid");
                                row.classList.add('tracklistSongNotExistsInLibrary');
                                row.classList.remove('tracklistSongExistsInLibrary');
                                var libraryApi = new XpoMusicScript.SpotifyApi.Library();
                                var result = yield libraryApi.removeTrack(trackId);
                                if (!result) {
                                    row.classList.add('tracklistSongExistsInLibrary');
                                    row.classList.remove('tracklistSongNotExistsInLibrary');
                                }
                            });
                        });
                        var destContainer = tracks[i].querySelectorAll('.more')[0];
                        if (destContainer !== undefined) {
                            destContainer.appendChild(addSongDiv);
                            destContainer.appendChild(removeSongDiv);
                            count++;
                        }
                        tracks[i].setAttribute("data-xpotify-addremovebuttonsadded", "1");
                    }
                    return count > 0;
                }
                catch (ex) {
                    XpoMusic.log(ex.toString());
                    return false;
                }
            }
            UiElementModifier.createTrackListAddRemoveButtons = createTrackListAddRemoveButtons;
        })(UiElementModifier = Common.UiElementModifier || (Common.UiElementModifier = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var UriHelper;
        (function (UriHelper) {
            function getPwaUri(uri) {
                if (uri === undefined || uri.trim() === "") {
                    return "";
                }
                uri = uri.replace('http://', 'https://');
                var uriLowerCase = uri.toLowerCase();
                if (uriLowerCase.startsWith("https://open.spotify.com"))
                    return uri;
                if (uriLowerCase.startsWith("spotify:")) {
                    if (uriLowerCase.indexOf("spotify:artist:") >= 0) {
                        var idx = uriLowerCase.indexOf("spotify:artist:") + "spotify:artist:".length;
                        return "https://open.spotify.com/artist/" + uri.substring(idx);
                    }
                    else if (uriLowerCase.indexOf("spotify:album:") >= 0) {
                        var idx = uriLowerCase.indexOf("spotify:album:") + "spotify:album:".length;
                        return "https://open.spotify.com/album/" + uri.substring(idx);
                    }
                    else if (uriLowerCase.indexOf("spotify:playlist:") >= 0) {
                        var idx = uriLowerCase.indexOf("spotify:playlist:") + "spotify:playlist:".length;
                        return "https://open.spotify.com/playlist/" + uri.substring(idx);
                    }
                    else if (uriLowerCase.indexOf("spotify:track:") >= 0) {
                        var idx = uriLowerCase.indexOf("spotify:track:") + "spotify:track:".length;
                        return "https://open.spotify.com/track/" + uri.substring(idx);
                    }
                }
                return "";
            }
            UriHelper.getPwaUri = getPwaUri;
        })(UriHelper = Common.UriHelper || (Common.UriHelper = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
/// <reference path="uriHelper.ts" />
var XpoMusicScript;
/// <reference path="uriHelper.ts" />
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var DragDrop;
        (function (DragDrop) {
            function allowDrop(event) {
                event.preventDefault();
            }
            DragDrop.allowDrop = allowDrop;
            function drop(event) {
                var data = event.dataTransfer.getData("Text");
                var uri = Common.UriHelper.getPwaUri(data);
                if (uri === undefined || uri.length === 0) {
                    return;
                }
                event.preventDefault();
                // Navigate to page
                Common.Action.navigateToPage(uri);
            }
            DragDrop.drop = drop;
        })(DragDrop = Common.DragDrop || (Common.DragDrop = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Lib;
    (function (Lib) {
        var Vibrant;
        (function (Vibrant) {
            function init() {
                //@ts-ignore
                (function e$$0(x, z, l) { function h(p, b) { if (!z[p]) {
                    if (!x[p]) {
                        var a = "function" == typeof require && require;
                        if (!b && a)
                            return a(p, !0);
                        if (g)
                            return g(p, !0);
                        a = Error("Cannot find module '" + p + "'");
                        throw a.code = "MODULE_NOT_FOUND", a;
                    }
                    a = z[p] = { exports: {} };
                    x[p][0].call(a.exports, function (a) { var b = x[p][1][a]; return h(b ? b : a); }, a, a.exports, e$$0, x, z, l);
                } return z[p].exports; } for (var g = "function" == typeof require && require, w = 0; w < l.length; w++)
                    h(l[w]); return h; })({ 1: [function (A, x, z) {
                            if (!l)
                                var l = { map: function (h, g) {
                                        var l = {};
                                        return g ?
                                            //@ts-ignore
                                            h.map(function (h, b) { l.index = b; return g.call(l, h); }) : h.slice();
                                    }, naturalOrder: function (h, g) { return h < g ? -1 : h > g ? 1 : 0; }, sum: function (h, g) { var l = {}; return h.reduce(g ? function (h, b, a) { l.index = a; return h + g.call(l, b); } : function (h, b) { return h + b; }, 0); }, max: function (h, g) { return Math.max.apply(null, g ? l.map(h, g) : h); } };
                            A = function () {
                                function h(f, c, a) { return (f << 2 * d) + (c << d) + a; }
                                function g(f) {
                                    function c() { a.sort(f); b = !0; }
                                    var a = [], b = !1;
                                    return { push: function (c) { a.push(c); b = !1; }, peek: function (f) { b || c(); void 0 === f && (f = a.length - 1); return a[f]; },
                                        //@ts-ignore
                                        pop: function () { b || c(); return a.pop(); }, size: function () { return a.length; }, map: function (c) { return a.map(c); }, debug: function () { b || c(); return a; } };
                                }
                                function w(f, c, a, b, m, e, q) { this.r1 = f; this.r2 = c; this.g1 = a; this.g2 = b; this.b1 = m; this.b2 = e; this.histo = q; }
                                function p() { this.vboxes = new g(function (f, c) { return l.naturalOrder(f.vbox.count() * f.vbox.volume(), c.vbox.count() * c.vbox.volume()); }); }
                                function b(f) { var c = Array(1 << 3 * d), a, b, m, r; f.forEach(function (f) { b = f[0] >> e; m = f[1] >> e; r = f[2] >> e; a = h(b, m, r); c[a] = (c[a] || 0) + 1; }); return c; }
                                function a(f, c) { var a = 1E6, b = 0, m = 1E6, d = 0, q = 1E6, n = 0, h, k, l; f.forEach(function (c) { h = c[0] >> e; k = c[1] >> e; l = c[2] >> e; h < a ? a = h : h > b && (b = h); k < m ? m = k : k > d && (d = k); l < q ? q = l : l > n && (n = l); }); return new w(a, b, m, d, q, n, c); }
                                function n(a, c) {
                                    function b(a) { var f = a + "1"; a += "2"; var v, d, m, e; d = 0; for (k = c[f]; k <= c[a]; k++)
                                        if (y[k] > n / 2) {
                                            m = c.copy();
                                            e = c.copy();
                                            v = k - c[f];
                                            d = c[a] - k;
                                            for (v = v <= d ? Math.min(c[a] - 1, ~~(k + d / 2)) : Math.max(c[f], ~~(k - 1 - v / 2)); !y[v];)
                                                v++;
                                            for (d = s[v]; !d && y[v - 1];)
                                                d = s[--v];
                                            m[a] = v;
                                            e[f] = m[a] + 1;
                                            return [m, e];
                                        } }
                                    if (c.count()) {
                                        var d = c.r2 -
                                            //@ts-ignore
                                            c.r1 + 1, m = c.g2 - c.g1 + 1, e = l.max([d, m, c.b2 - c.b1 + 1]);
                                        if (1 == c.count())
                                            return [c.copy()];
                                        var n = 0, y = [], s = [], k, g, t, u, p;
                                        if (e == d)
                                            for (k = c.r1; k <= c.r2; k++) {
                                                u = 0;
                                                for (g = c.g1; g <= c.g2; g++)
                                                    for (t = c.b1; t <= c.b2; t++)
                                                        p = h(k, g, t), u += a[p] || 0;
                                                n += u;
                                                y[k] = n;
                                            }
                                        else if (e == m)
                                            for (k = c.g1; k <= c.g2; k++) {
                                                u = 0;
                                                for (g = c.r1; g <= c.r2; g++)
                                                    for (t = c.b1; t <= c.b2; t++)
                                                        p = h(g, k, t), u += a[p] || 0;
                                                n += u;
                                                y[k] = n;
                                            }
                                        else
                                            for (k = c.b1; k <= c.b2; k++) {
                                                u = 0;
                                                for (g = c.r1; g <= c.r2; g++)
                                                    for (t = c.g1; t <= c.g2; t++)
                                                        p = h(g, t, k), u += a[p] || 0;
                                                n += u;
                                                y[k] = n;
                                            }
                                        y.forEach(function (a, c) { s[c] = n - a; });
                                        return e ==
                                            //@ts-ignore
                                            d ? b("r") : e == m ? b("g") : b("b");
                                    }
                                }
                                var d = 5, e = 8 - d;
                                w.prototype = { volume: function (a) { if (!this._volume || a)
                                        this._volume = (this.r2 - this.r1 + 1) * (this.g2 - this.g1 + 1) * (this.b2 - this.b1 + 1); return this._volume; }, count: function (a) { var c = this.histo; if (!this._count_set || a) {
                                        a = 0;
                                        var b, d, n;
                                        for (b = this.r1; b <= this.r2; b++)
                                            for (d = this.g1; d <= this.g2; d++)
                                                for (n = this.b1; n <= this.b2; n++)
                                                    index = h(b, d, n), a += c[index] || 0;
                                        this._count = a;
                                        this._count_set = !0;
                                    } return this._count; }, copy: function () {
                                        return new w(this.r1, this.r2, this.g1, this.g2, this.b1, 
                                        //@ts-ignore
                                        this.b2, this.histo);
                                    }, avg: function (a) { var c = this.histo; if (!this._avg || a) {
                                        a = 0;
                                        var b = 1 << 8 - d, n = 0, e = 0, g = 0, q, l, s, k;
                                        for (l = this.r1; l <= this.r2; l++)
                                            for (s = this.g1; s <= this.g2; s++)
                                                for (k = this.b1; k <= this.b2; k++)
                                                    q = h(l, s, k), q = c[q] || 0, a += q, n += q * (l + 0.5) * b, e += q * (s + 0.5) * b, g += q * (k + 0.5) * b;
                                        this._avg = a ? [~~(n / a), ~~(e / a), ~~(g / a)] : [~~(b * (this.r1 + this.r2 + 1) / 2), ~~(b * (this.g1 + this.g2 + 1) / 2), ~~(b * (this.b1 + this.b2 + 1) / 2)];
                                    } return this._avg; }, contains: function (a) {
                                        var c = a[0] >> e;
                                        gval = a[1] >> e;
                                        bval = a[2] >> e;
                                        return c >= this.r1 && c <= this.r2 &&
                                            //@ts-ignore
                                            gval >= this.g1 && gval <= this.g2 && bval >= this.b1 && bval <= this.b2;
                                    } };
                                p.prototype = { push: function (a) { this.vboxes.push({ vbox: a, color: a.avg() }); }, palette: function () { return this.vboxes.map(function (a) { return a.color; }); }, size: function () { return this.vboxes.size(); }, map: function (a) { for (var c = this.vboxes, b = 0; b < c.size(); b++)
                                        if (c.peek(b).vbox.contains(a))
                                            return c.peek(b).color; return this.nearest(a); }, nearest: function (a) {
                                        for (var c = this.vboxes, b, n, d, e = 0; e < c.size(); e++)
                                            if (n = Math.sqrt(Math.pow(a[0] - c.peek(e).color[0], 2) + Math.pow(a[1] -
                                                //@ts-ignore
                                                c.peek(e).color[1], 2) + Math.pow(a[2] - c.peek(e).color[2], 2)), n < b || void 0 === b)
                                                b = n, d = c.peek(e).color;
                                        return d;
                                    }, forcebw: function () { var a = this.vboxes; a.sort(function (a, b) { return l.naturalOrder(l.sum(a.color), l.sum(b.color)); }); var b = a[0].color; 5 > b[0] && 5 > b[1] && 5 > b[2] && (a[0].color = [0, 0, 0]); var b = a.length - 1, n = a[b].color; 251 < n[0] && 251 < n[1] && 251 < n[2] && (a[b].color = [255, 255, 255]); } };
                                return { quantize: function (d, c) {
                                        function e(a, b) {
                                            for (var c = 1, d = 0, f; 1E3 > d;)
                                                if (f = a.pop(), f.count()) {
                                                    var m = n(h, f);
                                                    f = m[0];
                                                    m = m[1];
                                                    if (!f)
                                                        break;
                                                    //@ts-ignore
                                                    a.push(f);
                                                    m && (a.push(m), c++);
                                                    if (c >= b)
                                                        break;
                                                    if (1E3 < d++)
                                                        break;
                                                }
                                                else
                                                    a.push(f), d++;
                                        }
                                        if (!d.length || 2 > c || 256 < c)
                                            return !1;
                                        var h = b(d), m = 0;
                                        h.forEach(function () { m++; });
                                        var r = a(d, h), q = new g(function (a, b) { return l.naturalOrder(a.count(), b.count()); });
                                        q.push(r);
                                        e(q, 0.75 * c);
                                        for (r = new g(function (a, b) { return l.naturalOrder(a.count() * a.volume(), b.count() * b.volume()); }); q.size();)
                                            r.push(q.pop());
                                        e(r, c - r.size());
                                        for (q = new p; r.size();)
                                            q.push(r.pop());
                                        return q;
                                    } };
                            }();
                            x.exports = A.quantize;
                        }, {}], 2: [function (A, x, z) {
                            (function () {
                                var l, 
                                //@ts-ignore
                                h, g, w = function (b, a) { return function () { return b.apply(a, arguments); }; }, p = [].slice;
                                window.Swatch = h = function () {
                                    function b(a, b) { this.rgb = a; this.population = b; }
                                    b.prototype.hsl = void 0;
                                    b.prototype.rgb = void 0;
                                    b.prototype.population = 1;
                                    b.yiq = 0;
                                    b.prototype.getHsl = function () { return this.hsl ? this.hsl : this.hsl = g.rgbToHsl(this.rgb[0], this.rgb[1], this.rgb[2]); };
                                    b.prototype.getPopulation = function () { return this.population; };
                                    b.prototype.getRgb = function () { return this.rgb; };
                                    b.prototype.getHex = function () {
                                        return "#" + (16777216 +
                                            //@ts-ignore
                                            (this.rgb[0] << 16) + (this.rgb[1] << 8) + this.rgb[2]).toString(16).slice(1, 7);
                                    };
                                    b.prototype.getTitleTextColor = function () { this._ensureTextColors(); return 200 > this.yiq ? "#fff" : "#000"; };
                                    b.prototype.getBodyTextColor = function () { this._ensureTextColors(); return 150 > this.yiq ? "#fff" : "#000"; };
                                    b.prototype._ensureTextColors = function () { if (!this.yiq)
                                        return this.yiq = (299 * this.rgb[0] + 587 * this.rgb[1] + 114 * this.rgb[2]) / 1E3; };
                                    return b;
                                }();
                                window.Vibrant = g = function () {
                                    function b(a, b, d) {
                                        this.swatches = w(this.swatches, this);
                                        var e, f, c, g, p, m, r, q;
                                        "undefined" === typeof b && (b = 64);
                                        "undefined" === typeof d && (d = 5);
                                        p = new l(a);
                                        r = p.getImageData().data;
                                        m = p.getPixelCount();
                                        a = [];
                                        for (g = 0; g < m;)
                                            e = 4 * g, q = r[e + 0], c = r[e + 1], f = r[e + 2], e = r[e + 3], 125 <= e && (250 < q && 250 < c && 250 < f || a.push([q, c, f])), g += d;
                                        this._swatches = this.quantize(a, b).vboxes.map(function (a) { return function (a) { return new h(a.color, a.vbox.count()); }; }(this));
                                        this.maxPopulation = this.findMaxPopulation;
                                        this.generateVarationColors();
                                        this.generateEmptySwatches();
                                        p.removeCanvas();
                                    }
                                    b.prototype.quantize =
                                        A("quantize");
                                    b.prototype._swatches = [];
                                    b.prototype.TARGET_DARK_LUMA = 0.26;
                                    b.prototype.MAX_DARK_LUMA = 0.45;
                                    b.prototype.MIN_LIGHT_LUMA = 0.55;
                                    b.prototype.TARGET_LIGHT_LUMA = 0.74;
                                    b.prototype.MIN_NORMAL_LUMA = 0.3;
                                    b.prototype.TARGET_NORMAL_LUMA = 0.5;
                                    b.prototype.MAX_NORMAL_LUMA = 0.7;
                                    b.prototype.TARGET_MUTED_SATURATION = 0.3;
                                    b.prototype.MAX_MUTED_SATURATION = 0.4;
                                    b.prototype.TARGET_VIBRANT_SATURATION = 1;
                                    b.prototype.MIN_VIBRANT_SATURATION = 0.35;
                                    b.prototype.WEIGHT_SATURATION = 3;
                                    b.prototype.WEIGHT_LUMA = 6;
                                    b.prototype.WEIGHT_POPULATION =
                                        1;
                                    b.prototype.VibrantSwatch = void 0;
                                    b.prototype.MutedSwatch = void 0;
                                    b.prototype.DarkVibrantSwatch = void 0;
                                    b.prototype.DarkMutedSwatch = void 0;
                                    b.prototype.LightVibrantSwatch = void 0;
                                    b.prototype.LightMutedSwatch = void 0;
                                    b.prototype.HighestPopulation = 0;
                                    b.prototype.generateVarationColors = function () {
                                        this.VibrantSwatch = this.findColorVariation(this.TARGET_NORMAL_LUMA, this.MIN_NORMAL_LUMA, this.MAX_NORMAL_LUMA, this.TARGET_VIBRANT_SATURATION, this.MIN_VIBRANT_SATURATION, 1);
                                        this.LightVibrantSwatch = this.findColorVariation(this.TARGET_LIGHT_LUMA, this.MIN_LIGHT_LUMA, 1, this.TARGET_VIBRANT_SATURATION, this.MIN_VIBRANT_SATURATION, 1);
                                        this.DarkVibrantSwatch = this.findColorVariation(this.TARGET_DARK_LUMA, 0, this.MAX_DARK_LUMA, this.TARGET_VIBRANT_SATURATION, this.MIN_VIBRANT_SATURATION, 1);
                                        this.MutedSwatch = this.findColorVariation(this.TARGET_NORMAL_LUMA, this.MIN_NORMAL_LUMA, this.MAX_NORMAL_LUMA, this.TARGET_MUTED_SATURATION, 0, this.MAX_MUTED_SATURATION);
                                        this.LightMutedSwatch = this.findColorVariation(this.TARGET_LIGHT_LUMA, this.MIN_LIGHT_LUMA, 1, this.TARGET_MUTED_SATURATION, 0, this.MAX_MUTED_SATURATION);
                                        return this.DarkMutedSwatch = this.findColorVariation(this.TARGET_DARK_LUMA, 0, this.MAX_DARK_LUMA, this.TARGET_MUTED_SATURATION, 0, this.MAX_MUTED_SATURATION);
                                    };
                                    b.prototype.generateEmptySwatches = function () {
                                        var a;
                                        void 0 === this.VibrantSwatch && void 0 !== this.DarkVibrantSwatch && (a = this.DarkVibrantSwatch.getHsl(), a[2] = this.TARGET_NORMAL_LUMA, this.VibrantSwatch = new h(b.hslToRgb(a[0], a[1], a[2]), 0));
                                        if (void 0 === this.DarkVibrantSwatch && void 0 !== this.VibrantSwatch)
                                            return a = this.VibrantSwatch.getHsl(),
                                                a[2] = this.TARGET_DARK_LUMA, this.DarkVibrantSwatch = new h(b.hslToRgb(a[0], a[1], a[2]), 0);
                                    };
                                    b.prototype.findMaxPopulation = function () { var a, b, d, e, f; d = 0; e = this._swatches; a = 0; for (b = e.length; a < b; a++)
                                        f = e[a], d = Math.max(d, f.getPopulation()); return d; };
                                    b.prototype.findColorVariation = function (a, b, d, e, f, c) {
                                        var g, h, m, l, q, p, s, k;
                                        l = void 0;
                                        q = 0;
                                        p = this._swatches;
                                        g = 0;
                                        for (h = p.length; g < h; g++)
                                            if (k = p[g], s = k.getHsl()[1], m = k.getHsl()[2], s >= f && s <= c && m >= b && m <= d && !this.isAlreadySelected(k) && (m = this.createComparisonValue(s, e, m, a, k.getPopulation(), this.HighestPopulation), void 0 === l || m > q))
                                                l = k, q = m;
                                        return l;
                                    };
                                    b.prototype.createComparisonValue = function (a, b, d, e, f, c) { return this.weightedMean(this.invertDiff(a, b), this.WEIGHT_SATURATION, this.invertDiff(d, e), this.WEIGHT_LUMA, f / c, this.WEIGHT_POPULATION); };
                                    b.prototype.invertDiff = function (a, b) { return 1 - Math.abs(a - b); };
                                    b.prototype.weightedMean = function () { var a, b, d, e, f, c; f = 1 <= arguments.length ? p.call(arguments, 0) : []; for (a = d = b = 0; a < f.length;)
                                        e = f[a], c = f[a + 1], b += e * c, d += c, a += 2; return b / d; };
                                    b.prototype.swatches =
                                        function () { return { Vibrant: this.VibrantSwatch, Muted: this.MutedSwatch, DarkVibrant: this.DarkVibrantSwatch, DarkMuted: this.DarkMutedSwatch, LightVibrant: this.LightVibrantSwatch, LightMuted: this.LightMuted }; };
                                    b.prototype.isAlreadySelected = function (a) { return this.VibrantSwatch === a || this.DarkVibrantSwatch === a || this.LightVibrantSwatch === a || this.MutedSwatch === a || this.DarkMutedSwatch === a || this.LightMutedSwatch === a; };
                                    b.rgbToHsl = function (a, b, d) {
                                        var e, f, c, g, h;
                                        a /= 255;
                                        b /= 255;
                                        d /= 255;
                                        g = Math.max(a, b, d);
                                        h = Math.min(a, b, d);
                                        //@ts-ignore
                                        f = void 0;
                                        c = (g + h) / 2;
                                        if (g === h)
                                            f = h = 0;
                                        else {
                                            e = g - h;
                                            h = 0.5 < c ? e / (2 - g - h) : e / (g + h);
                                            switch (g) {
                                                case a:
                                                    f = (b - d) / e + (b < d ? 6 : 0);
                                                    break;
                                                case b:
                                                    f = (d - a) / e + 2;
                                                    break;
                                                case d: f = (a - b) / e + 4;
                                            }
                                            f /= 6;
                                        }
                                        return [f, h, c];
                                    };
                                    b.hslToRgb = function (a, b, d) { var e, f, c; e = f = c = void 0; e = function (a, b, c) { 0 > c && (c += 1); 1 < c && (c -= 1); return c < 1 / 6 ? a + 6 * (b - a) * c : 0.5 > c ? b : c < 2 / 3 ? a + (b - a) * (2 / 3 - c) * 6 : a; }; 0 === b ? c = f = e = d : (b = 0.5 > d ? d * (1 + b) : d + b - d * b, d = 2 * d - b, c = e(d, b, a + 1 / 3), f = e(d, b, a), e = e(d, b, a - 1 / 3)); return [255 * c, 255 * f, 255 * e]; };
                                    return b;
                                }();
                                window.CanvasImage = l = function () {
                                    function b(a) {
                                        this.canvas =
                                            document.createElement("canvas");
                                        this.context = this.canvas.getContext("2d");
                                        document.body.appendChild(this.canvas);
                                        this.width = this.canvas.width = a.width;
                                        this.height = this.canvas.height = a.height;
                                        this.context.drawImage(a, 0, 0, this.width, this.height);
                                    }
                                    b.prototype.clear = function () { return this.context.clearRect(0, 0, this.width, this.height); };
                                    b.prototype.update = function (a) { return this.context.putImageData(a, 0, 0); };
                                    b.prototype.getPixelCount = function () { return this.width * this.height; };
                                    b.prototype.getImageData = function () {
                                        return this.context.getImageData(0, 0, this.width, this.height);
                                    };
                                    b.prototype.removeCanvas = function () { return this.canvas.parentNode.removeChild(this.canvas); };
                                    return b;
                                }();
                            }).call(this);
                        }, { quantize: 1 }] }, {}, [2]);
            }
            Vibrant.init = init;
        })(Vibrant = Lib.Vibrant || (Lib.Vibrant = {}));
    })(Lib = XpoMusicScript.Lib || (XpoMusicScript.Lib = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Lib;
    (function (Lib) {
        var FocusVisible;
        (function (FocusVisible) {
            function init() {
                /**
                 * Applies the :focus-visible polyfill at the given scope.
                 * A scope in this case is either the top-level Document or a Shadow Root.
                 *
                 * @param {(Document|ShadowRoot)} scope
                 * @see https://github.com/WICG/focus-visible
                 */
                function applyFocusVisiblePolyfill(scope) {
                    var hadKeyboardEvent = true;
                    var hadFocusVisibleRecently = false;
                    var hadFocusVisibleRecentlyTimeout = null;
                    var inputTypesWhitelist = {
                        text: true,
                        search: true,
                        url: true,
                        tel: true,
                        email: true,
                        password: true,
                        number: true,
                        date: true,
                        month: true,
                        week: true,
                        time: true,
                        datetime: true,
                        'datetime-local': true
                    };
                    /**
                     * Helper function for legacy browsers and iframes which sometimes focus
                     * elements like document, body, and non-interactive SVG.
                     * @param {Element} el
                     */
                    function isValidFocusTarget(el) {
                        if (el &&
                            el !== document &&
                            el.nodeName !== 'HTML' &&
                            el.nodeName !== 'BODY' &&
                            'classList' in el &&
                            'contains' in el.classList) {
                            return true;
                        }
                        return false;
                    }
                    /**
                     * Computes whether the given element should automatically trigger the
                     * `focus-visible` class being added, i.e. whether it should always match
                     * `:focus-visible` when focused.
                     * @param {Element} el
                     * @return {boolean}
                     */
                    function focusTriggersKeyboardModality(el) {
                        var type = el.type;
                        var tagName = el.tagName;
                        if (tagName == 'INPUT' && inputTypesWhitelist[type] && !el.readOnly) {
                            return true;
                        }
                        if (tagName == 'TEXTAREA' && !el.readOnly) {
                            return true;
                        }
                        if (el.isContentEditable) {
                            return true;
                        }
                        return false;
                    }
                    /**
                     * Add the `focus-visible` class to the given element if it was not added by
                     * the author.
                     * @param {Element} el
                     */
                    function addFocusVisibleClass(el) {
                        if (el.classList.contains('focus-visible')) {
                            return;
                        }
                        el.classList.add('focus-visible');
                        el.setAttribute('data-focus-visible-added', '');
                    }
                    /**
                     * Remove the `focus-visible` class from the given element if it was not
                     * originally added by the author.
                     * @param {Element} el
                     */
                    function removeFocusVisibleClass(el) {
                        if (!el.hasAttribute('data-focus-visible-added')) {
                            return;
                        }
                        el.classList.remove('focus-visible');
                        el.removeAttribute('data-focus-visible-added');
                    }
                    /**
                     * If the most recent user interaction was via the keyboard;
                     * and the key press did not include a meta, alt/option, or control key;
                     * then the modality is keyboard. Otherwise, the modality is not keyboard.
                     * Apply `focus-visible` to any current active element and keep track
                     * of our keyboard modality state with `hadKeyboardEvent`.
                     * @param {KeyboardEvent} e
                     */
                    function onKeyDown(e) {
                        if (e.metaKey || e.altKey || e.ctrlKey) {
                            return;
                        }
                        if (isValidFocusTarget(scope.activeElement)) {
                            addFocusVisibleClass(scope.activeElement);
                        }
                        hadKeyboardEvent = true;
                    }
                    /**
                     * If at any point a user clicks with a pointing device, ensure that we change
                     * the modality away from keyboard.
                     * This avoids the situation where a user presses a key on an already focused
                     * element, and then clicks on a different element, focusing it with a
                     * pointing device, while we still think we're in keyboard modality.
                     * @param {Event} e
                     */
                    function onPointerDown(e) {
                        hadKeyboardEvent = false;
                    }
                    /**
                     * On `focus`, add the `focus-visible` class to the target if:
                     * - the target received focus as a result of keyboard navigation, or
                     * - the event target is an element that will likely require interaction
                     *   via the keyboard (e.g. a text box)
                     * @param {Event} e
                     */
                    function onFocus(e) {
                        // Prevent IE from focusing the document or HTML element.
                        if (!isValidFocusTarget(e.target)) {
                            return;
                        }
                        if (hadKeyboardEvent || focusTriggersKeyboardModality(e.target)) {
                            addFocusVisibleClass(e.target);
                        }
                    }
                    /**
                     * On `blur`, remove the `focus-visible` class from the target.
                     * @param {Event} e
                     */
                    function onBlur(e) {
                        if (!isValidFocusTarget(e.target)) {
                            return;
                        }
                        if (e.target.classList.contains('focus-visible') ||
                            e.target.hasAttribute('data-focus-visible-added')) {
                            // To detect a tab/window switch, we look for a blur event followed
                            // rapidly by a visibility change.
                            // If we don't see a visibility change within 100ms, it's probably a
                            // regular focus change.
                            hadFocusVisibleRecently = true;
                            window.clearTimeout(hadFocusVisibleRecentlyTimeout);
                            hadFocusVisibleRecentlyTimeout = window.setTimeout(function () {
                                hadFocusVisibleRecently = false;
                                window.clearTimeout(hadFocusVisibleRecentlyTimeout);
                            }, 100);
                            removeFocusVisibleClass(e.target);
                        }
                    }
                    /**
                     * If the user changes tabs, keep track of whether or not the previously
                     * focused element had .focus-visible.
                     * @param {Event} e
                     */
                    function onVisibilityChange(e) {
                        if (document.visibilityState == 'hidden') {
                            // If the tab becomes active again, the browser will handle calling focus
                            // on the element (Safari actually calls it twice).
                            // If this tab change caused a blur on an element with focus-visible,
                            // re-apply the class when the user switches back to the tab.
                            if (hadFocusVisibleRecently) {
                                hadKeyboardEvent = true;
                            }
                            addInitialPointerMoveListeners();
                        }
                    }
                    /**
                     * Add a group of listeners to detect usage of any pointing devices.
                     * These listeners will be added when the polyfill first loads, and anytime
                     * the window is blurred, so that they are active when the window regains
                     * focus.
                     */
                    function addInitialPointerMoveListeners() {
                        document.addEventListener('mousemove', onInitialPointerMove);
                        document.addEventListener('mousedown', onInitialPointerMove);
                        document.addEventListener('mouseup', onInitialPointerMove);
                        document.addEventListener('pointermove', onInitialPointerMove);
                        document.addEventListener('pointerdown', onInitialPointerMove);
                        document.addEventListener('pointerup', onInitialPointerMove);
                        document.addEventListener('touchmove', onInitialPointerMove);
                        document.addEventListener('touchstart', onInitialPointerMove);
                        document.addEventListener('touchend', onInitialPointerMove);
                    }
                    function removeInitialPointerMoveListeners() {
                        document.removeEventListener('mousemove', onInitialPointerMove);
                        document.removeEventListener('mousedown', onInitialPointerMove);
                        document.removeEventListener('mouseup', onInitialPointerMove);
                        document.removeEventListener('pointermove', onInitialPointerMove);
                        document.removeEventListener('pointerdown', onInitialPointerMove);
                        document.removeEventListener('pointerup', onInitialPointerMove);
                        document.removeEventListener('touchmove', onInitialPointerMove);
                        document.removeEventListener('touchstart', onInitialPointerMove);
                        document.removeEventListener('touchend', onInitialPointerMove);
                    }
                    /**
                     * When the polfyill first loads, assume the user is in keyboard modality.
                     * If any event is received from a pointing device (e.g. mouse, pointer,
                     * touch), turn off keyboard modality.
                     * This accounts for situations where focus enters the page from the URL bar.
                     * @param {Event} e
                     */
                    function onInitialPointerMove(e) {
                        // Work around a Safari quirk that fires a mousemove on <html> whenever the
                        // window blurs, even if you're tabbing out of the page. ¯\_(ツ)_/¯
                        if (e.target.nodeName && e.target.nodeName.toLowerCase() === 'html') {
                            return;
                        }
                        hadKeyboardEvent = false;
                        removeInitialPointerMoveListeners();
                    }
                    // For some kinds of state, we are interested in changes at the global scope
                    // only. For example, global pointer input, global key presses and global
                    // visibility change should affect the state at every scope:
                    document.addEventListener('keydown', onKeyDown, true);
                    document.addEventListener('mousedown', onPointerDown, true);
                    document.addEventListener('pointerdown', onPointerDown, true);
                    document.addEventListener('touchstart', onPointerDown, true);
                    document.addEventListener('visibilitychange', onVisibilityChange, true);
                    addInitialPointerMoveListeners();
                    // For focus and blur, we specifically care about state changes in the local
                    // scope. This is because focus / blur events that originate from within a
                    // shadow root are not re-dispatched from the host element if it was already
                    // the active element in its own scope:
                    scope.addEventListener('focus', onFocus, true);
                    scope.addEventListener('blur', onBlur, true);
                    // We detect that a node is a ShadowRoot by ensuring that it is a
                    // DocumentFragment and also has a host property. This check covers native
                    // implementation and polyfill implementation transparently. If we only cared
                    // about the native implementation, we could just check if the scope was
                    // an instance of a ShadowRoot.
                    if (scope.nodeType === Node.DOCUMENT_FRAGMENT_NODE && scope.host) {
                        // Since a ShadowRoot is a special kind of DocumentFragment, it does not
                        // have a root element to add a class to. So, we add this attribute to the
                        // host element instead:
                        scope.host.setAttribute('data-js-focus-visible', '');
                    }
                    else if (scope.nodeType === Node.DOCUMENT_NODE) {
                        document.documentElement.classList.add('js-focus-visible');
                    }
                }
                // It is important to wrap all references to global window and document in
                // these checks to support server-side rendering use cases
                // @see https://github.com/WICG/focus-visible/issues/199
                if (typeof window !== 'undefined') {
                    // Make the polyfill helper globally available. This can be used as a signal
                    // to interested libraries that wish to coordinate with the polyfill for e.g.,
                    // applying the polyfill to a shadow root:
                    // @ts-ignore
                    window.applyFocusVisiblePolyfill = applyFocusVisiblePolyfill;
                    // Notify interested libraries of the polyfill's presence, in case the
                    // polyfill was loaded lazily:
                    var event;
                    try {
                        event = new CustomEvent('focus-visible-polyfill-ready');
                    }
                    catch (error) {
                        // IE11 does not support using CustomEvent as a constructor directly:
                        event = document.createEvent('CustomEvent');
                        event.initCustomEvent('focus-visible-polyfill-ready', false, false, {});
                    }
                    window.dispatchEvent(event);
                }
                if (typeof document !== 'undefined') {
                    // Apply the polyfill to the global document, so that no JavaScript
                    // coordination is required to use the polyfill in the top-level document:
                    applyFocusVisiblePolyfill(document);
                }
            }
            FocusVisible.init = init;
        })(FocusVisible = Lib.FocusVisible || (Lib.FocusVisible = {}));
    })(Lib = XpoMusicScript.Lib || (XpoMusicScript.Lib = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var Resize;
        (function (Resize) {
            function onResize() {
                try {
                    /*
                     *  I couldn't fix the width of the new track list for Edge (Works fine in Chrome but not in Edge),
                     *  so I use a javascript workaround for that.
                     */
                    var contentDiv = document.querySelectorAll(".main-view-container__content");
                    if (contentDiv.length === 0) {
                        contentDiv = document.querySelectorAll(".main-view-container__scroll-node");
                    }
                    // 230px is added because it's added in css as well, for acrylic behind artist page.
                    contentDiv[0].style.width = 230 + (window.innerWidth - document.querySelectorAll(".Root__nav-bar")[0].offsetWidth) + "px";
                    var adContainerDiv = document.querySelectorAll(".AdsContainer");
                    if (adContainerDiv.length > 0)
                        adContainerDiv[0].style.width = (window.innerWidth - document.querySelectorAll(".Root__nav-bar")[0].offsetWidth) + "px";
                }
                catch (ex) {
                    console.log("resize event failed");
                }
            }
            Resize.onResize = onResize;
        })(Resize = Common.Resize || (Common.Resize = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var StartupAnimation;
        (function (StartupAnimation) {
            function init() {
                try {
                    var pivotItems = document.querySelectorAll(".Root__main-view nav ul li");
                    var entranceItems = document.querySelectorAll(".container-fluid");
                    for (var i = 0; i < pivotItems.length; i++) {
                        pivotItems[i].style.animation = 'none';
                    }
                    for (var i = 0; i < entranceItems.length; i++) {
                        entranceItems[i].style.animation = 'none';
                        entranceItems[i].style.opacity = '0';
                    }
                    setTimeout(function () {
                        try {
                            for (var i = 0; i < pivotItems.length; i++) {
                                pivotItems[i].style.animation = '';
                            }
                        }
                        catch (ex2) {
                            console.log(ex2);
                        }
                    }, 400);
                    setTimeout(function () {
                        try {
                            for (var i = 0; i < entranceItems.length; i++) {
                                entranceItems[i].style.animation = '';
                                entranceItems[i].style.opacity = '1';
                            }
                        }
                        catch (ex2) {
                            console.log(ex2);
                        }
                    }, 400);
                }
                catch (ex) {
                    console.log(ex);
                }
            }
            StartupAnimation.init = init;
        })(StartupAnimation = Common.StartupAnimation || (Common.StartupAnimation = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var IndeterminateProgressBarHandler;
        (function (IndeterminateProgressBarHandler) {
            function trackLoadCheck(initialProgress) {
                try {
                    var playbackBar = (document.querySelectorAll(".Root__now-playing-bar .playback-bar")[0]);
                    var progressBarProgress = (playbackBar.querySelectorAll(".progress-bar__fg")[0]).style.transform;
                    var isPlaying = true;
                    try {
                        isPlaying = Common.StatusReport.getIsPlaying();
                    }
                    catch (ex) { }
                    if (progressBarProgress != initialProgress || isPlaying === false) {
                        var progressBarBg = (playbackBar.querySelectorAll(".progress-bar__bg")[0]);
                        var times = playbackBar.querySelectorAll(".playback-bar__progress-time");
                        progressBarBg.style.opacity = '1';
                        (times[0]).style.opacity = '1';
                        (times[1]).style.opacity = '1';
                        XpoMusic.hideProgressBar();
                    }
                    else {
                        setTimeout(function () {
                            trackLoadCheck(initialProgress);
                        }, 250);
                    }
                }
                catch (ex) {
                    console.log("trackLoadCheck failed.", ex);
                }
            }
            function onTrackLoadBegin() {
                try {
                    var playbackBar = (document.querySelectorAll(".Root__now-playing-bar .playback-bar")[0]);
                    var progressBarBg = (playbackBar.querySelectorAll(".progress-bar__bg")[0]);
                    var times = playbackBar.querySelectorAll(".playback-bar__progress-time");
                    progressBarBg.style.opacity = '0';
                    (times[0]).style.opacity = '0';
                    (times[1]).style.opacity = '0';
                    var rect = progressBarBg.getBoundingClientRect();
                    XpoMusic.showProgressBar(rect.left / window.innerWidth, rect.top / window.innerHeight, rect.width / window.innerWidth);
                    setTimeout(function () {
                        var progressBarProgress = (playbackBar.querySelectorAll(".progress-bar__fg")[0]).style.transform;
                        if (progressBarProgress.indexOf("translate") == -1) {
                            // Something's wrong. Will ignore progress bar
                            progressBarBg.style.opacity = '1';
                            (times[0]).style.opacity = '1';
                            (times[1]).style.opacity = '1';
                            console.log("onTrackLoadBegin failed, and reverted changes.");
                            return;
                        }
                        setTimeout(function () {
                            trackLoadCheck(progressBarProgress);
                        }, 250);
                    }, 250);
                }
                catch (ex) {
                    console.log("onTrackLoadBegin failed.", ex);
                }
            }
            IndeterminateProgressBarHandler.onTrackLoadBegin = onTrackLoadBegin;
        })(IndeterminateProgressBarHandler = Common.IndeterminateProgressBarHandler || (Common.IndeterminateProgressBarHandler = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
/// <reference path="indeterminateProgressBarHandler.ts" />
var XpoMusicScript;
/// <reference path="indeterminateProgressBarHandler.ts" />
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var RequestIntercepter;
        (function (RequestIntercepter) {
            function startInterceptingFetch() {
                const fetch = window.fetch;
                window.fetch = (...args) => ((args) => __awaiter(this, void 0, void 0, function* () {
                    // Request interception
                    if (args[0].toString().endsWith('/state')) {
                        console.log(args);
                        var reqJson = JSON.stringify(args);
                        if (reqJson.indexOf("before_track_load") >= 0) {
                            Common.IndeterminateProgressBarHandler.onTrackLoadBegin();
                        }
                    }
                    else if (args[0].toString().endsWith('/v1/devices')) {
                        console.log(args);
                        console.log(args[1].body);
                        var appName = Common.getAppName();
                        var spotifyConnectName = Common.getDeviceName() + ' (' + appName + ')';
                        args[1].body = args[1].body.toString().replace('Web Player (Microsoft Edge)', spotifyConnectName);
                    }
                    // Sending the real request
                    var result = yield fetch(...args);
                    // Response interception
                    // Returning response
                    return result;
                }))(args);
            }
            RequestIntercepter.startInterceptingFetch = startInterceptingFetch;
        })(RequestIntercepter = Common.RequestIntercepter || (Common.RequestIntercepter = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var StatusReport;
        (function (StatusReport) {
            function getTextContent(element, index) {
                var e = document.querySelectorAll(element);
                if (e.length > index) {
                    return (e[index]).innerText;
                }
                else {
                    throw "Couldn't find element.";
                }
            }
            function getTrackFingerprint() {
                return getTextContent('.Root__now-playing-bar .now-playing-bar__left', 0);
            }
            StatusReport.getTrackFingerprint = getTrackFingerprint;
            function getTrackName() {
                return getTextContent('.Root__now-playing-bar .now-playing-bar__left .track-info .track-info__name', 0);
            }
            StatusReport.getTrackName = getTrackName;
            function getTrackArtist() {
                return getTextContent('.Root__now-playing-bar .now-playing-bar__left .track-info .track-info__artists', 0);
            }
            StatusReport.getTrackArtist = getTrackArtist;
            function getTrackAlbumId() {
                var artistUri = ((document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__left .track-info .track-info__name a")[0])).href;
                return artistUri.substring(artistUri.lastIndexOf('/') + 1);
            }
            StatusReport.getTrackAlbumId = getTrackAlbumId;
            function getTrackArtistId() {
                var artistUri = ((document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__left .track-info .track-info__artists a")[0])).href;
                return artistUri.substring(artistUri.lastIndexOf('/') + 1);
            }
            StatusReport.getTrackArtistId = getTrackArtistId;
            function timeStringToMilliseconds(time) {
                if (time.match('^[0-9]+\:[0-9][0-9]?$') === null)
                    throw "Invalid time format";
                var parts = time.split(':');
                return (Number.parseInt(parts[0]) * 60 + Number.parseInt(parts[1])) * 1000;
            }
            StatusReport.timeStringToMilliseconds = timeStringToMilliseconds;
            function getElapsedTime() {
                var time = getTextContent('.Root__now-playing-bar .now-playing-bar__center .playback-bar .playback-bar__progress-time', 0);
                return timeStringToMilliseconds(time);
            }
            StatusReport.getElapsedTime = getElapsedTime;
            function getTotalTime() {
                var time = getTextContent('.Root__now-playing-bar .now-playing-bar__center .playback-bar .playback-bar__progress-time', 1);
                return timeStringToMilliseconds(time);
            }
            StatusReport.getTotalTime = getTotalTime;
            function getIsSavedToLibrary() {
                var e = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left .spoticon-heart-active-16');
                return (e.length > 0);
                // This is not fatal, so we don't throw exception if this fails.
            }
            StatusReport.getIsSavedToLibrary = getIsSavedToLibrary;
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
            StatusReport.getIsPlaying = getIsPlaying;
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
            StatusReport.getIsPrevTrackAvailable = getIsPrevTrackAvailable;
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
            StatusReport.getIsNextTrackAvailable = getIsNextTrackAvailable;
            function getTrackId() {
                var fingerprint = getTrackFingerprint();
                // @ts-ignore
                if (window.xpotify_prevFingerprint === fingerprint) {
                    // @ts-ignore
                    return window.xpotify_prevTrackId;
                }
                var tracks = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__left .track-info .track-info__name a');
                if (tracks.length === 0)
                    return "";
                var element = tracks[0];
                var menus = document.querySelectorAll("nav[role=menu]");
                for (var i = 0; i < menus.length; i++) {
                    menus[i].style.display = 'none';
                }
                // Open context menu on track name on now playing bar
                var e = element.ownerDocument.createEvent('MouseEvents');
                e.initMouseEvent('contextmenu', true, true, element.ownerDocument.defaultView, 1, 0, 0, 0, 0, false, false, false, false, 2, null);
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
                e2.initMouseEvent('mousedown', true, true, window, 0, 0, 0, clickX, clickY, false, false, false, false, 0, null);
                document.elementFromPoint(clickX, clickY).dispatchEvent(e2);
                e2 = document.createEvent('MouseEvents');
                e2.initMouseEvent('mouseup', true, true, window, 0, 0, 0, clickX, clickY, false, false, false, false, 0, null);
                document.elementFromPoint(clickX, clickY).dispatchEvent(e2);
                setTimeout(function () {
                    for (var i = 0; i < menus.length; i++) {
                        menus[i].style.opacity = '0';
                        menus[i].style.display = 'unset';
                    }
                }, 500);
                // @ts-ignore
                window.xpotify_prevFingerprint = fingerprint;
                // @ts-ignore
                window.xpotify_prevTrackId = trackId;
                // We will ignore (not throw exception) if we can't find trackId, as this is not fatal anyway.
                return trackId;
            }
            StatusReport.getTrackId = getTrackId;
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
            StatusReport.getVolume = getVolume;
            function getNowPlaying() {
                var trackName, trackId, albumId, artistName, artistId, elapsedTime, totalTime, trackFingerprint;
                var isPrevTrackAvailable, isNextTrackAvailable, isPlaying, isSavedToLibrary, volume;
                var success = true;
                try {
                    trackName = getTrackName();
                }
                catch (ex) {
                    console.log("Failed to get trackName.");
                    success = false;
                }
                try {
                    trackId = getTrackId();
                }
                catch (ex) {
                    console.log("Failed to get trackId.");
                    trackId = ex.toString();
                    success = false;
                }
                try {
                    albumId = getTrackAlbumId();
                }
                catch (ex) {
                    console.log("Failed to get albumId.");
                    success = false;
                }
                try {
                    artistName = getTrackArtist();
                }
                catch (ex) {
                    console.log("Failed to get artistName.");
                    success = false;
                }
                try {
                    artistId = getTrackArtistId();
                }
                catch (ex) {
                    console.log("Failed to get artistId.");
                    success = false;
                }
                try {
                    elapsedTime = getElapsedTime();
                }
                catch (ex) {
                    console.log("Failed to get elapsedTime.");
                    success = false;
                }
                try {
                    totalTime = getTotalTime();
                }
                catch (ex) {
                    console.log("Failed to get totalTime.");
                    success = false;
                }
                try {
                    trackFingerprint = getTrackFingerprint();
                }
                catch (ex) {
                    console.log("Failed to get trackFingerprint.");
                    success = false;
                }
                try {
                    isPrevTrackAvailable = getIsPrevTrackAvailable();
                }
                catch (ex) {
                    console.log("Failed to get isPrevTrackAvailable.");
                    success = false;
                }
                try {
                    isNextTrackAvailable = getIsNextTrackAvailable();
                }
                catch (ex) {
                    console.log("Failed to get isNextTrackAvailable.");
                    success = false;
                }
                try {
                    isPlaying = getIsPlaying();
                }
                catch (ex) {
                    console.log("Failed to get isPlaying.");
                    success = false;
                }
                try {
                    isSavedToLibrary = getIsSavedToLibrary();
                }
                catch (ex) {
                    console.log("Failed to get isSavedToLibrary.");
                    success = false;
                }
                try {
                    volume = getVolume();
                }
                catch (ex) {
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
                }
                else if (backButtonDiv[0].classList.contains("backButtonContainer-disabled")) {
                    return false;
                }
                else {
                    return true;
                }
            }
            StatusReport.isBackPossible = isBackPossible;
            function sendStatusReport() {
                var data = JSON.stringify({
                    BackButtonEnabled: isBackPossible(),
                    NowPlaying: getNowPlaying(),
                });
                XpoMusic.statusReport(data);
            }
            function initRegularStatusReport() {
                setInterval(sendStatusReport, 1000);
            }
            StatusReport.initRegularStatusReport = initRegularStatusReport;
        })(StatusReport = Common.StatusReport || (Common.StatusReport = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var Action;
        (function (Action) {
            function nextTrack() {
                var nextButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-forward-16");
                if (nextButton.length === 1) {
                    nextButton[0].click();
                    return "1";
                }
                return "0";
            }
            Action.nextTrack = nextTrack;
            function pause() {
                var pauseButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-pause-16");
                if (pauseButton.length === 1) {
                    pauseButton[0].click();
                    return "1";
                }
                return "0";
            }
            Action.pause = pause;
            function play() {
                var playButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-play-16");
                if (playButton.length === 1) {
                    playButton[0].click();
                    return "1";
                }
                return "0";
            }
            Action.play = play;
            function playPause() {
                if (play() === '0') {
                    pause();
                }
            }
            Action.playPause = playPause;
            function prevTrack() {
                var prevButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16");
                if (prevButton.length === 1) {
                    prevButton[0].click();
                    return "1";
                }
                return "0";
            }
            Action.prevTrack = prevTrack;
            function prevTrackForce() {
                var prevButton = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar__center .spoticon-skip-back-16");
                if (prevButton.length === 1) {
                    var currentPlaying = Common.StatusReport.getTrackFingerprint();
                    prevButton[0].click();
                    var delay = 300;
                    if (document.querySelectorAll(".ConnectBar").length > 0)
                        delay = 1250;
                    setTimeout(function () {
                        var newCurrentPlaying = Common.StatusReport.getTrackFingerprint();
                        if (currentPlaying === newCurrentPlaying) {
                            prevButton[0].click();
                        }
                    }, delay);
                    return "1";
                }
                return "0";
            }
            Action.prevTrackForce = prevTrackForce;
            function tryAutoPlayTrack(remainingCount) {
                if (remainingCount <= 0)
                    return;
                //console.log('r' + remainingCount);
                setTimeout(function () {
                    var modalPlayButton = document.querySelectorAll(".autoplay-modal-container .btn-green");
                    if (modalPlayButton.length > 0)
                        modalPlayButton[0].click();
                    else
                        tryAutoPlayTrack(remainingCount - 1);
                }, 500);
            }
            function autoPlayTrack() {
                tryAutoPlayTrack(26);
            }
            Action.autoPlayTrack = autoPlayTrack;
            function tryAutoPlayPlaylist(remainingCount) {
                if (remainingCount <= 0)
                    return;
                //console.log('r' + remainingCount);
                setTimeout(function () {
                    var modalPlayButton = document.querySelectorAll(".autoplay-modal-container .btn-green");
                    var tracklistPlayButton = document.querySelectorAll(".TrackListHeader .btn-green");
                    if (modalPlayButton.length > 0)
                        modalPlayButton[0].click();
                    else if (tracklistPlayButton.length > 0)
                        tracklistPlayButton[0].click();
                    else
                        tryAutoPlayPlaylist(remainingCount - 1);
                }, 500);
            }
            function autoPlayPlaylist() {
                tryAutoPlayPlaylist(26);
            }
            Action.autoPlayPlaylist = autoPlayPlaylist;
            function enableNowPlaying() {
                var nowPlayingButton = document.querySelectorAll('.nowPlaying-navBar-item');
                if (nowPlayingButton.length > 0) {
                    nowPlayingButton[0].classList.remove('nowPlaying-navBar-item-disabled');
                }
                else {
                    setTimeout(enableNowPlaying, 1000);
                    return;
                }
                var compactOverlayButton = document.querySelectorAll('.CompactOverlayButton');
                if (compactOverlayButton.length > 0) {
                    compactOverlayButton[0].classList.remove('CompactOverlayButton-disabled');
                }
                else {
                    setTimeout(enableNowPlaying, 1000);
                    return;
                }
            }
            Action.enableNowPlaying = enableNowPlaying;
            function goBackIfPossible() {
                if (Common.BrowserHistory.canGoBack()) {
                    window.history.go(-1);
                    return "1";
                }
                return "0";
            }
            Action.goBackIfPossible = goBackIfPossible;
            function goForwardIfPossible() {
                window.history.go(1);
            }
            Action.goForwardIfPossible = goForwardIfPossible;
            function isPlayingOnThisApp() {
                return (document.querySelectorAll(".Root__now-playing-bar .ConnectBar").length === 0) ? "1" : "0";
            }
            Action.isPlayingOnThisApp = isPlayingOnThisApp;
            function navigateToPage(url) {
                history.pushState({}, null, url);
                history.pushState({}, null, url + "#navigatingToPagePleaseIgnore");
                history.back();
            }
            Action.navigateToPage = navigateToPage;
            function seekPlayback(percentage) {
                var progressBar = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar .playback-bar .progress-bar")[0];
                var rect = progressBar.getBoundingClientRect();
                var x = rect.left + 1 + (rect.width - 2) * percentage;
                var y = rect.top + rect.height / 2;
                var mouseDownEvent = document.createEvent('MouseEvents');
                mouseDownEvent.initMouseEvent('mousedown', true, true, window, 0, 0, 0, x, y, false, false, false, false, 0, null);
                document.elementFromPoint(x, y).dispatchEvent(mouseDownEvent);
                var mouseUpEvent = document.createEvent('MouseEvents');
                mouseUpEvent.initMouseEvent('mouseup', true, true, window, 0, 0, 0, x, y, false, false, false, false, 0, null);
                document.elementFromPoint(x, y).dispatchEvent(mouseUpEvent);
            }
            Action.seekPlayback = seekPlayback;
            function seekVolume(percentage) {
                var progressBar = document.querySelectorAll(".Root__now-playing-bar .now-playing-bar .volume-bar .progress-bar")[0];
                var rect = progressBar.getBoundingClientRect();
                var x = rect.left + (rect.width - 1) * percentage;
                var y = rect.top + rect.height / 2;
                var mouseDownEvent = document.createEvent('MouseEvents');
                mouseDownEvent.initMouseEvent('mousedown', true, true, window, 0, 0, 0, x, y, false, false, false, false, 0, null);
                document.elementFromPoint(x, y).dispatchEvent(mouseDownEvent);
                var mouseUpEvent = document.createEvent('MouseEvents');
                mouseUpEvent.initMouseEvent('mouseup', true, true, window, 0, 0, 0, x, y, false, false, false, false, 0, null);
                document.elementFromPoint(x, y).dispatchEvent(mouseUpEvent);
            }
            Action.seekVolume = seekVolume;
            function newPlaylist() {
                var newPlaylistButton = document.querySelectorAll('.CreatePlaylistButton');
                if (newPlaylistButton.length > 0) {
                    newPlaylistButton[0].click();
                }
                else {
                    // Old UI
                    Action.navigateToPage("/collection/playlists");
                    setTimeout(function () {
                        document.querySelectorAll('.Root__main-view .asideButton button')[0].click();
                    }, 250);
                    return;
                }
            }
            Action.newPlaylist = newPlaylist;
        })(Action = Common.Action || (Common.Action = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var PageTitleFinder;
        (function (PageTitleFinder) {
            function titleCase(str) {
                var splitStr = str.toLowerCase().split(' ');
                for (var i = 0; i < splitStr.length; i++) {
                    // You do not need to check if i is larger than splitStr length, as your for does that for you
                    // Assign it back to the array
                    splitStr[i] = splitStr[i].charAt(0).toUpperCase() + splitStr[i].substring(1);
                }
                // Directly return the joined string
                return splitStr.join(' ');
            }
            function searchForNavBarSelectedItem() {
                var candidates = document.querySelectorAll(".Root__main-view nav a");
                var selIndex = -1;
                for (var i = 0; i < candidates.length; i++) {
                    if (candidates[i].classList.length == 1) {
                        continue;
                    }
                    else if (candidates[i].classList.length == 2) {
                        if (selIndex == -1) {
                            selIndex = i;
                        }
                        else {
                            return '';
                        }
                    }
                    else {
                        return '';
                    }
                }
                if (selIndex == -1) {
                    return '';
                }
                else {
                    return titleCase(candidates[selIndex].innerText);
                }
            }
            function searchForHTag(selector) {
                var candidates = document.querySelectorAll(".Root__main-view " + selector);
                for (var i = 0; i < candidates.length; i++) {
                    var s = candidates[i].innerText;
                    if (s.length > 0 && s.length < 80) {
                        return s;
                    }
                }
                return '';
            }
            function getTitle() {
                var h1AndNav = document.querySelectorAll('.Root__main-view h1, .Root__main-view nav');
                var result;
                if (h1AndNav.length > 0) {
                    // Between h1 and nav tags, prioritise the one that comes first
                    if (h1AndNav[0].tagName.toLowerCase() == "nav") {
                        result = searchForNavBarSelectedItem();
                        if (result != '')
                            return result;
                        result = searchForHTag('h1');
                        if (result != '')
                            return result;
                    }
                    else {
                        result = searchForHTag('h1');
                        if (result != '')
                            return result;
                        result = searchForNavBarSelectedItem();
                        if (result != '')
                            return result;
                    }
                }
                result = searchForHTag('h2');
                if (result != '')
                    return result;
                return window.location.href.substring(window.location.href.lastIndexOf('/') + 1).replace(/-/g, ' ');
            }
            PageTitleFinder.getTitle = getTitle;
        })(PageTitleFinder = Common.PageTitleFinder || (Common.PageTitleFinder = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />
var XpoMusicScript;
/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var KeyboardShortcutListener;
        (function (KeyboardShortcutListener) {
            function copyToClipboard(str) {
                // From https://hackernoon.com/copying-text-to-clipboard-with-javascript-df4d4988697f
                const el = document.createElement('textarea');
                el.value = str;
                el.setAttribute('readonly', '');
                el.style.position = 'absolute';
                el.style.left = '-9999px';
                document.body.appendChild(el);
                const selected = document.getSelection().rangeCount > 0
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
            function scrollIntoViewEx(element) {
                var bounds = element.getBoundingClientRect();
                if (bounds.top > (window.innerHeight - 200)) {
                    element.scrollIntoView(false);
                    bounds = element.getBoundingClientRect();
                    if (bounds.top > (window.innerHeight - 300)) {
                        var parent = getScrollParent(element, false);
                        parent.scrollTop += 300;
                    }
                }
                else if (bounds.top < 30) {
                    element.scrollIntoView(true);
                    bounds = element.getBoundingClientRect();
                    if (bounds.top < 200) {
                        var parent = getScrollParent(element, false);
                        parent.scrollTop -= 200;
                    }
                }
            }
            function keyboardSelect(offset) {
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
                }
                else {
                    idx += offset;
                    idx = Math.min(idx, focusableElements.length - 1);
                    idx = Math.max(idx, 0);
                }
                var selectedElement = focusableElements[idx];
                selectedElement.focus();
                scrollIntoViewEx(selectedElement);
            }
            function keyDownExternalCall(charCode, shiftPressed, ctrlPressed, altPressed) {
                // @ts-ignore
                return processKeyDown({
                    which: charCode,
                    altKey: altPressed,
                    shiftKey: shiftPressed,
                    ctrlKey: ctrlPressed,
                }) ? "1" : "0";
            }
            KeyboardShortcutListener.keyDownExternalCall = keyDownExternalCall;
            function processKeyDown(e) {
                // Ignore if focus is on a textbox and the selected key is anything other than arrow up/down keys
                var isInputFocused = document.activeElement.tagName.toLowerCase() === "input";
                if (isInputFocused && e.which != 38 && e.which != 40 && !e.ctrlKey && !e.altKey)
                    return;
                var shortcutKeyProcessed = true;
                // Spotify style shortcuts
                if (e.ctrlKey && e.which == 'N'.charCodeAt(0)) {
                    // Ctrl+N -> New Playlist
                    Common.Action.newPlaylist();
                }
                else if (e.ctrlKey && e.which == 'C'.charCodeAt(0) && !isInputFocused) {
                    // Ctrl+C -> Copy
                    copyToClipboard(window.location.href.replace("#xpotifyInitialPage", ""));
                }
                else if (e.ctrlKey && e.which == 'V'.charCodeAt(0) && !isInputFocused) {
                    // Ctrl+V -> Paste
                    XpoMusic.navigateToClipboardUri();
                    //} else if (e.which == 46) {
                    // Delete key -> Delete
                }
                else if (e.which == ' '.charCodeAt(0)) {
                    // Space -> Play/pause
                    if (Common.StatusReport.getIsPlaying())
                        Common.Action.pause();
                    else
                        Common.Action.play();
                }
                else if (e.ctrlKey && e.which == 39) {
                    // Ctrl+ArrowRight -> Next Track
                    Common.Action.nextTrack();
                }
                else if (e.ctrlKey && e.which == 37) {
                    // Ctrl+ArrowLeft -> Prev Track
                    Common.Action.prevTrack();
                }
                else if (e.ctrlKey && e.shiftKey && e.which == 38) {
                    // Ctrl+Shift+ArrowUp -> Max volume
                    Common.Action.seekVolume(1);
                }
                else if (e.ctrlKey && e.shiftKey && e.which == 40) {
                    // Ctrl+Shift+ArrowDown -> Mute
                    Common.Action.seekVolume(0);
                }
                else if (e.ctrlKey && e.which == 38) {
                    // Ctrl+ArrowUp -> Volume up
                    var volume = Common.StatusReport.getVolume();
                    var newVolume = Math.min(volume + 0.1, 1);
                    Common.Action.seekVolume(newVolume);
                }
                else if (e.ctrlKey && e.which == 40) {
                    // Ctrl+ArrowDown -> Volume down
                    var volume = Common.StatusReport.getVolume();
                    var newVolume = Math.max(volume - 0.1, 0);
                    Common.Action.seekVolume(newVolume);
                }
                else if (e.ctrlKey && e.which == 'L'.charCodeAt(0)) {
                    // Ctrl+L -> Search
                    Common.Action.navigateToPage("/search");
                    //} else if (e.ctrlKey && e.which == 'F'.charCodeAt(0)) {
                    // Ctrl+F -> Filter (in Songs and Playlists) -- not present at the moment
                }
                else if (e.altKey && e.which == 37) {
                    // Alt+ArrowLeft -> Go back
                    Common.Action.goBackIfPossible();
                }
                else if (e.altKey && e.which == 39) {
                    // Alt+ArrowRight -> Go forward
                    Common.Action.goForwardIfPossible();
                    //} else if (e.which == 13) {
                    // Enter -> Play selected row -- not present at the moment
                    // Enter key is handled automatically by the PWA
                }
                else if (e.which == 38 || e.which == 37) {
                    // ArrowUp or ArrowLeft -> 
                    // If not selected, select the first item in track list
                    // and if selected, go one up.
                    keyboardSelect(-1);
                }
                else if (e.which == 40 || e.which == 39) {
                    // ArrowDown or ArrowRight -> 
                    // If not selected, select the first item in track list
                    // and if selected, go one down.
                    keyboardSelect(1);
                }
                else if (e.ctrlKey && e.which == 'P'.charCodeAt(0)) {
                    // Ctrl+P -> Settings
                    XpoMusic.openSettings();
                }
                // Custom shortcuts
                else if (e.ctrlKey && e.which == 'M'.charCodeAt(0)) {
                    // Ctrl+M -> Go to mini view
                    XpoMusic.openMiniView();
                }
                else if (e.ctrlKey && e.which == 188) {
                    // Ctrl+, -> Go to now playing
                    XpoMusic.openNowPlaying();
                }
                else if (e.ctrlKey && e.which == 'Q'.charCodeAt(0)) {
                    // Ctrl+Q -> Open playing queue
                    Common.Action.navigateToPage("/queue");
                }
                else if (e.altKey && (e.which == '1'.charCodeAt(0) || e.which == 97)) {
                    // Alt+1 -> Home
                    Common.Action.navigateToPage("/browse");
                }
                else if (e.altKey && (e.which == '2'.charCodeAt(0) || e.which == 98)) {
                    // Alt+2 -> Search
                    Common.Action.navigateToPage("/search");
                }
                else if (e.altKey && (e.which == '3'.charCodeAt(0) || e.which == 99)) {
                    // Alt+3 -> Your Library
                    Common.Action.navigateToPage("/collection");
                }
                else if (e.altKey && (e.which == '4'.charCodeAt(0) || e.which == 100)) {
                    // Alt+4 -> Now Playing
                    XpoMusic.openNowPlaying();
                }
                else {
                    shortcutKeyProcessed = false;
                }
                return shortcutKeyProcessed;
            }
            function onKeyDown(e) {
                var shortcutKeyProcessed = processKeyDown(e);
                if (shortcutKeyProcessed) {
                    e.preventDefault();
                    return false;
                }
            }
            function init() {
                window.onkeydown = onKeyDown;
            }
            KeyboardShortcutListener.init = init;
        })(KeyboardShortcutListener = Common.KeyboardShortcutListener || (Common.KeyboardShortcutListener = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />
var XpoMusicScript;
/// <reference path="statusReport.ts" />
/// <reference path="action.ts" />
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var MouseWheelListener;
        (function (MouseWheelListener) {
            function volumeMouseWheelHandler(e) {
                var { deltaY } = e;
                var currentVolume = Common.StatusReport.getVolume();
                var newVolume = Math.max(Math.min(currentVolume - (deltaY / 1000), 1), 0);
                Common.Action.seekVolume(newVolume);
            }
            function setVolumeBarListener() {
                var volumeBar = document.querySelector(".Root__top-container .Root__now-playing-bar .now-playing-bar__right__inner .volume-bar > div");
                if (volumeBar === null) {
                    // Volume bar not present yet. Will try again later.
                    setTimeout(setVolumeBarListener, 1000);
                    return;
                }
                volumeBar.addEventListener("mousewheel", (e) => { volumeMouseWheelHandler(e); });
            }
            function init() {
                setVolumeBarListener();
            }
            MouseWheelListener.init = init;
        })(MouseWheelListener = Common.MouseWheelListener || (Common.MouseWheelListener = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
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
var XpoMusicScript;
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
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        function isProVersion() {
            //@ts-ignore
            return '{{XPOTIFYISPROVERSION}}' === '1';
        }
        Common.isProVersion = isProVersion;
        function getDeviceName() {
            return '{{DEVICENAME}}';
        }
        Common.getDeviceName = getDeviceName;
        function getAppName() {
            return isProVersion() ? 'Xpo Music Pro' : 'Xpo Music';
        }
        Common.getAppName = getAppName;
        function isLightTheme() {
            return (document.getElementsByTagName('body')[0].getAttribute('data-xpotifyTheme') === 'light');
        }
        Common.isLightTheme = isLightTheme;
        function init() {
            var errors = "";
            markPageAsInjected();
            initDragDrop();
            XpoMusic.log("Initializing UiElemetModifier stuff...");
            errors += injectCss();
            errors += Common.UiElementModifier.createPageTitle();
            errors += Common.UiElementModifier.createBackButton();
            errors += Common.UiElementModifier.createNavBarButtons();
            errors += Common.UiElementModifier.createCompactOverlayButton();
            errors += Common.UiElementModifier.addNowPlayingButton();
            errors += Common.UiElementModifier.addBackgroundClass();
            errors += initNowPlayingBarCheck();
            XpoMusic.log("Setting page hash and initializing resize and periodic checks...");
            setInitialPageHash();
            initOnResizeCheck();
            initPeriodicPageCheck();
            XpoMusic.log("Initializing libraries...");
            XpoMusicScript.Lib.FocusVisible.init();
            XpoMusic.log("Initializing MouseWheelListener...");
            Common.MouseWheelListener.init();
            XpoMusic.log("Initializing KeyboardShortcutListener...");
            Common.KeyboardShortcutListener.init();
            XpoMusic.log("Initializing RequestIntercepter...");
            Common.RequestIntercepter.startInterceptingFetch();
            XpoMusic.log("Initializing StatusReport...");
            Common.StatusReport.initRegularStatusReport();
            XpoMusic.log("Initializing StartupAnimation...");
            Common.StartupAnimation.init();
            // @ts-ignore
            if (window.XpoMusicScript === undefined)
                // @ts-ignore
                window.XpoMusicScript = XpoMusicScript;
            XpoMusic.log("Common.init() finished. errors = '" + errors + "'");
            return errors;
        }
        Common.init = init;
        function markPageAsInjected() {
            var body = document.getElementsByTagName('body')[0];
            body.setAttribute('data-scriptinjection', '1');
        }
        function initDragDrop() {
            var body = document.getElementsByTagName('body')[0];
            body.ondrop = Common.DragDrop.drop;
            body.ondragover = Common.DragDrop.allowDrop;
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
                XpoMusicScript.Lib.Vibrant.init();
                setInterval(function () {
                    try {
                        var url = document.querySelectorAll(".Root__now-playing-bar .now-playing .cover-art-image")[0].style.backgroundImage.slice(5, -2);
                        var lightTheme = isLightTheme();
                        if (window["xpotifyNowPlayingIconUrl"] !== url || window["xpotifyNowPlayingLastSetLightTheme"] !== lightTheme) {
                            window["xpotifyNowPlayingIconUrl"] = url;
                            window["xpotifyNowPlayingLastSetLightTheme"] = lightTheme;
                            Common.Color.setNowPlayingBarColor(url, lightTheme);
                        }
                    }
                    catch (ex) { }
                }, 1000);
            }
            catch (ex) {
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
                    var backButtonDiv = backButtonDivC[0];
                    if (Common.BrowserHistory.canGoBack()) {
                        backButtonDiv.classList.remove("backButtonContainer-disabled");
                    }
                    else {
                        backButtonDiv.classList.add("backButtonContainer-disabled");
                    }
                }, 500);
            }, 1000);
        }
        function initOnResizeCheck() {
            window.addEventListener("resize", Common.Resize.onResize, true);
            setInterval(Common.Resize.onResize, 5000); // Sometimes an OnResize is necessary when users goes to a new page.
        }
        function periodicPageCheck() {
            try {
                if (document.querySelectorAll(".tracklist").length > 0) {
                    Common.TracklistExtended.initTracklistMod();
                }
            }
            catch (ex) {
                console.log(ex);
            }
        }
        function initPeriodicPageCheck() {
            setInterval(periodicPageCheck, 1000);
        }
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Light;
    (function (Light) {
        var PageOverlay;
        (function (PageOverlay) {
            function createPageOverlay() {
                try {
                    var body = document.getElementsByTagName('body')[0];
                    var overlayDiv = document.createElement('div');
                    overlayDiv.classList.add("whole-page-overlay");
                    body.appendChild(overlayDiv);
                }
                catch (ex) {
                    return "injectOverlayFailed,";
                }
                return "";
            }
            PageOverlay.createPageOverlay = createPageOverlay;
        })(PageOverlay = Light.PageOverlay || (Light.PageOverlay = {}));
    })(Light = XpoMusicScript.Light || (XpoMusicScript.Light = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
/// <reference path="../Common/initScript-common.ts" />
/// <reference path="pageOverlay.ts" />
var XpoMusicScript;
/// <reference path="../Common/initScript-common.ts" />
/// <reference path="pageOverlay.ts" />
(function (XpoMusicScript) {
    document.getElementsByTagName('body')[0].setAttribute('data-xpotifyTheme', 'light');
    var errors = "";
    errors += XpoMusicScript.Common.init();
    errors += XpoMusicScript.Light.PageOverlay.createPageOverlay();
    if (errors.length > 0) {
        try {
            // @ts-ignore
            XpoMusic.initFailed(errors);
        }
        catch (ex) { }
        throw errors;
    }
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var PlaybackStuckHelper;
        (function (PlaybackStuckHelper) {
            function sleep(ms) {
                return new Promise(resolve => setTimeout(resolve, ms));
            }
            function resolveStart() {
                return __awaiter(this, void 0, void 0, function* () {
                    var initialElapsedTime = Common.StatusReport.getElapsedTime();
                    if (Common.Action.nextTrack() == '0')
                        return;
                    while (!Common.StatusReport.getIsPlaying() || Common.StatusReport.getElapsedTime() === 0 || Common.StatusReport.getElapsedTime() === initialElapsedTime)
                        yield sleep(500);
                    if (Common.Action.prevTrack() == '0')
                        return;
                    do {
                        yield sleep(500);
                    } while (!Common.StatusReport.getIsPlaying());
                    if (Common.Action.prevTrack() == '0')
                        return;
                    var newInitialElapsedTime = Common.StatusReport.getElapsedTime();
                    while (!Common.StatusReport.getIsPlaying() || Common.StatusReport.getElapsedTime() === 0 || Common.StatusReport.getElapsedTime() === newInitialElapsedTime)
                        yield sleep(100);
                });
            }
            function tryResolveStart() {
                return __awaiter(this, void 0, void 0, function* () {
                    // @ts-ignore
                    if (window.xpotifyStuckStartResolvingInProgress === true)
                        return;
                    // @ts-ignore
                    window.xpotifyStuckStartResolvingInProgress = true;
                    var volume = Common.StatusReport.getVolume();
                    try {
                        Common.Action.seekVolume(0);
                        yield resolveStart();
                    }
                    finally {
                        Common.Action.seekVolume(volume);
                        // @ts-ignore
                        window.xpotifyStuckStartResolvingInProgress = false;
                    }
                });
            }
            PlaybackStuckHelper.tryResolveStart = tryResolveStart;
            function tryResolveMiddle() {
                return __awaiter(this, void 0, void 0, function* () {
                    var elapsedTime = Common.StatusReport.getElapsedTime();
                    var totalTime = Common.StatusReport.getTotalTime();
                    var changeTime = Math.max(1000, totalTime / 200);
                    var newElapsedTime = elapsedTime - changeTime;
                    if (newElapsedTime <= 0)
                        newElapsedTime = elapsedTime + changeTime;
                    Common.Action.seekPlayback(newElapsedTime / totalTime);
                    yield sleep(1000);
                    Common.Action.play();
                });
            }
            PlaybackStuckHelper.tryResolveMiddle = tryResolveMiddle;
        })(PlaybackStuckHelper = Common.PlaybackStuckHelper || (Common.PlaybackStuckHelper = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var Common;
    (function (Common) {
        var TracklistExtended;
        (function (TracklistExtended) {
            function injectTrackIdsToTrackList() {
                // Hide menus
                var menus = document.querySelectorAll("nav[role=menu]");
                for (var i = 0; i < menus.length; i++) {
                    menus[i].style.display = 'none';
                }
                var tracks = document.querySelectorAll(".Root__main-view .tracklist-container .tracklist-row");
                var counter = 0;
                // Find track url for each tracklist-row and add it to data-trackid
                for (var i = 0; i < tracks.length; i++) {
                    var element = tracks[i];
                    if (element.getAttribute('data-trackid') !== null)
                        continue;
                    counter++;
                    var e = element.ownerDocument.createEvent('MouseEvents');
                    e.initMouseEvent('contextmenu', true, true, element.ownerDocument.defaultView, 1, 0, 0, 0, 0, false, false, false, false, 2, null);
                    element.dispatchEvent(e);
                    var tas = document.querySelectorAll("nav[role=menu] textarea");
                    for (var j = 0; j < tas.length; j++) {
                        if (tas[j].textContent.toLowerCase().startsWith("https://open.spotify.com/track/")) {
                            element.setAttribute('data-trackid', tas[j].textContent.substr("https://open.spotify.com/track/".length));
                        }
                    }
                }
                XpoMusic.log(counter.toString() + " trackIds extracted.");
                // Close context menu
                var clickX = 1;
                var clickY = window.innerHeight - 1;
                var e2 = document.createEvent('MouseEvents');
                e2.initMouseEvent('mousedown', true, true, window, 0, 0, 0, clickX, clickY, false, false, false, false, 0, null);
                document.elementFromPoint(clickX, clickY).dispatchEvent(e2);
                e2 = document.createEvent('MouseEvents');
                e2.initMouseEvent('mouseup', true, true, window, 0, 0, 0, clickX, clickY, false, false, false, false, 0, null);
                document.elementFromPoint(clickX, clickY).dispatchEvent(e2);
                // Don't let menu to be visible
                setTimeout(function () {
                    for (var i = 0; i < menus.length; i++) {
                        menus[i].style.opacity = '0';
                        menus[i].style.display = 'unset';
                    }
                }, 500);
            }
            TracklistExtended.injectTrackIdsToTrackList = injectTrackIdsToTrackList;
            function setAddRemoveButtons() {
                return __awaiter(this, void 0, void 0, function* () {
                    var tracklist = document.querySelectorAll(".Root__main-view .tracklist");
                    var rows = tracklist[0].querySelectorAll('.tracklist-row');
                    var trackIds = [];
                    for (var i = 0; i < rows.length; i++) {
                        var attr = rows[i].getAttribute('data-trackid');
                        if (attr !== undefined)
                            trackIds.push(attr);
                    }
                    var api = new XpoMusicScript.SpotifyApi.Library();
                    var result = yield api.isTracksSaved(trackIds);
                    result.forEach(function (item, index) {
                        var element = document.querySelectorAll('.tracklist-row[data-trackid="' + trackIds[index] + '"]');
                        if (element.length < 1)
                            return;
                        if (item) {
                            element[0].classList.remove('tracklistSongNotExistsInLibrary');
                            element[0].classList.add('tracklistSongExistsInLibrary');
                        }
                        else {
                            element[0].classList.remove('tracklistSongExistsInLibrary');
                            element[0].classList.add('tracklistSongNotExistsInLibrary');
                        }
                    });
                });
            }
            TracklistExtended.setAddRemoveButtons = setAddRemoveButtons;
            function initTracklistMod() {
                return __awaiter(this, void 0, void 0, function* () {
                    var tracklist = document.querySelectorAll(".Root__main-view .tracklist");
                    if (tracklist.length === 0)
                        return; // No tracklist found
                    if (window.location.href.startsWith("https://open.spotify.com/collection/tracks"))
                        return; // No tracklist add remove button for Liked Songs page (they're all liked by definition!)
                    if (tracklist[0].querySelectorAll(".tracklist-row").length === 0)
                        return; // List not loaded yet
                    if (!Common.UiElementModifier.createTrackListAddRemoveButtons())
                        return; // No new elements present
                    XpoMusic.log('Some new track list elements found. will try to find track ids.');
                    injectTrackIdsToTrackList();
                    // TODO: Don't ask server for the items we already know the status
                    yield setAddRemoveButtons();
                });
            }
            TracklistExtended.initTracklistMod = initTracklistMod;
        })(TracklistExtended = Common.TracklistExtended || (Common.TracklistExtended = {}));
    })(Common = XpoMusicScript.Common || (XpoMusicScript.Common = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var SpotifyApi;
    (function (SpotifyApi) {
        var accessToken = "{{SPOTIFYACCESSTOKEN}}";
        class ApiBase {
            sendJsonRequestWithToken(uri, method, body = undefined) {
                return __awaiter(this, void 0, void 0, function* () {
                    if (accessToken.length === 0) {
                        accessToken = yield XpoMusic.getNewAccessTokenAsync();
                    }
                    return yield this.sendJsonRequestWithTokenInternal(uri, method, body, true);
                });
            }
            sendJsonRequestWithTokenInternal(uri, method, body, allowRefreshingToken) {
                return __awaiter(this, void 0, void 0, function* () {
                    var response = yield fetch(uri, {
                        method: method,
                        body: JSON.stringify(body),
                        headers: {
                            'Authorization': 'Bearer ' + accessToken,
                        },
                    });
                    XpoMusic.log("SpotifyApi: " + uri + " (" + method + ") -> result status = " + response.status);
                    if (response.status == 401 && allowRefreshingToken) {
                        // Refresh access token and retry
                        XpoMusic.log("Will ask for new token.");
                        accessToken = yield XpoMusic.getNewAccessTokenAsync();
                        return yield this.sendJsonRequestWithTokenInternal(uri, method, body, false);
                    }
                    return response;
                });
            }
        }
        SpotifyApi.ApiBase = ApiBase;
    })(SpotifyApi = XpoMusicScript.SpotifyApi || (XpoMusicScript.SpotifyApi = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
var XpoMusicScript;
(function (XpoMusicScript) {
    var SpotifyApi;
    (function (SpotifyApi) {
        class Library extends SpotifyApi.ApiBase {
            isTrackSaved(trackId) {
                return __awaiter(this, void 0, void 0, function* () {
                    return (yield this.isTracksSaved([trackId]))[0];
                });
            }
            isTracksSaved(trackIds) {
                return __awaiter(this, void 0, void 0, function* () {
                    var output = [];
                    for (var i = 0; i < trackIds.length; i += 50) {
                        var slice = trackIds.slice(i, i + 50);
                        output = output.concat(yield this.isTracksSavedInternal(slice));
                    }
                    return output;
                });
            }
            isTracksSavedInternal(trackIds) {
                return __awaiter(this, void 0, void 0, function* () {
                    var url = 'https://api.spotify.com/v1/me/tracks/contains?ids=' + trackIds.join(',');
                    var result = yield this.sendJsonRequestWithToken(url, 'get');
                    var data = yield result.json();
                    return data;
                });
            }
            saveTrack(trackId) {
                return __awaiter(this, void 0, void 0, function* () {
                    var url = "https://api.spotify.com/v1/me/tracks?ids=" + trackId;
                    var result = yield this.sendJsonRequestWithToken(url, 'put');
                    return result.status >= 200 && result.status <= 299;
                });
            }
            removeTrack(trackId) {
                return __awaiter(this, void 0, void 0, function* () {
                    var url = "https://api.spotify.com/v1/me/tracks?ids=" + trackId;
                    var result = yield this.sendJsonRequestWithToken(url, 'delete');
                    return result.status >= 200 && result.status <= 299;
                });
            }
        }
        SpotifyApi.Library = Library;
    })(SpotifyApi = XpoMusicScript.SpotifyApi || (XpoMusicScript.SpotifyApi = {}));
})(XpoMusicScript || (XpoMusicScript = {}));
//# sourceMappingURL=init-light.js.map