
var isProVersion = false;

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
    }
    catch (ex) {
        console.log("resize event failed");
    }
}


function injectBackButton(backButtonDiv) {
    var navbarHeader = document.getElementsByClassName('navBar-header');
    if (navbarHeader.length === 0) {
        setTimeout(function () {
            injectBackButton(backButtonDiv);
        }, 500);
    } else {
        navbarHeader[0].prepend(backButtonDiv);
    }
}

function injectNavbarDownButton(button) {
    var navbar = document.querySelectorAll(".NavBarFooter");
    var sessionInfo = document.querySelectorAll(".sessionInfo");
    if (navbar.length === 0 || sessionInfo.length === 0) {
        setTimeout(function () {
            injectNavbarDownButton(button);
        }, 500);
    } else {
        navbar[0].insertBefore(button, sessionInfo[0]);
    }
}

function injectNowPlayingRightButton(button) {
    var extraControlsBar = document.querySelectorAll('.Root__now-playing-bar .now-playing-bar__right__inner .ExtraControls');
    if (extraControlsBar.length === 0) {
        setTimeout(function () {
            injectNowPlayingRightButton(button);
        }, 500);
    } else {
        extraControlsBar[0].prepend(button);
    }
}

function allowDrop(event) {
    event.preventDefault();
}

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
            idx = uriLowerCase.indexOf("spotify:artist:") + "spotify:artist:".length;
            return "https://open.spotify.com/artist/" + uri.substring(idx);
        }
        else if (uriLowerCase.indexOf("spotify:album:") >= 0) {
            idx = uriLowerCase.indexOf("spotify:album:") + "spotify:album:".length;
            return "https://open.spotify.com/album/" + uri.substring(idx);
        }
        else if (uriLowerCase.indexOf("spotify:playlist:") >= 0) {
            idx = uriLowerCase.indexOf("spotify:playlist:") + "spotify:playlist:".length;
            return "https://open.spotify.com/playlist/" + uri.substring(idx);
        }
        else if (uriLowerCase.indexOf("spotify:track:") >= 0) {
            idx = uriLowerCase.indexOf("spotify:track:") + "spotify:track:".length;
            return "https://open.spotify.com/track/" + uri.substring(idx);
        }
    }

    return "";
}

function drop(event) {
    var data = event.dataTransfer.getData("Text");
    var uri = getPwaUri(data);

    if (uri === undefined || uri.length === 0) {
        return;
    }

    event.preventDefault();

    // Navigate to page
    history.pushState({}, null, uri);
    history.pushState({}, null, uri + "#navigatingToPagePleaseIgnore");
    history.back();
}

errors = "";

// Mark page as injected
var body = document.getElementsByTagName('body')[0];
body.setAttribute('data-scriptinjection', 1);
body.ondrop = drop;
body.ondragover = allowDrop;

// Inject css
try {
    var css = '{{CSSBASE64CONTENT}}';
    var style = document.createElement('style');
    document.getElementsByTagName('head')[0].appendChild(style);
    style.type = 'text/css';
    style.appendChild(document.createTextNode(atob(css)));
}
catch (ex) {
    errors += "injectCssFailed,";
}

// Inject page title
try {
    var titleDiv = document.createElement('div');
    titleDiv.classList.add("xpotifyWindowTitle");
    titleDiv.innerText = isProVersion ? "Xpotify Pro" : "Xpotify";
    body.appendChild(titleDiv);
}
catch (ex) {
    errors += "injectTitleFailed,";
}

// Inject page overlay
try {
    var overlayDiv = document.createElement('div');
    overlayDiv.classList.add("whole-page-overlay");
    body.appendChild(overlayDiv);
}
catch (ex) {
    errors += "injectOverlayFailed,";
}

// Inject back button
try {
    var backButtonDiv = document.createElement('div');
    backButtonDiv.classList.add("backButtonContainer");
    backButtonDiv.classList.add("backButtonContainer-disabled");
    backButtonDiv.innerHTML = "<a class='backbutton' href='#xpotifygoback'><span>&#xE72B;</span></a>";
    injectBackButton(backButtonDiv);
}
catch (ex) {
    errors += "injectBackFailed,";
}

// Inject navbar buttons
try {
    var pinToStartButton = document.createElement('div');
    pinToStartButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifypintostart-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifypintostart">'
        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE718;</span></div>'
        + '<span class="navbar-link__text">Pin this page to Start</span></div></a></div>';
    var settingsButton = document.createElement('div');
    settingsButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifysettings">'
        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE115;</span></div>'
        + '<span class="navbar-link__text">Settings</span></div></a></div>';
    var aboutButton = document.createElement('div');
    aboutButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifyabout">'
        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE946;</span></div>'
        + '<span class="navbar-link__text">About</span></div></a></div>';
    var donateButton = document.createElement('div');
    donateButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifydonate">'
        + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE719;</span></div>'
        + '<span class="navbar-link__text">Donate</span></div></a></div>';
    injectNavbarDownButton(pinToStartButton);
    injectNavbarDownButton(settingsButton);
    //injectNavbarDownButton(aboutButton);
    if (!isProVersion)
        injectNavbarDownButton(donateButton);
}
catch (ex) {
    errors += "injectNavBarFooterFailed,";
}

// Inject compact overlay button to now playing
try {
    var compactOverlayButton = document.createElement('div');
    compactOverlayButton.className = "CompactOverlayButton";
    compactOverlayButton.innerHTML = '<a style="border-bottom: 0px;" href="#xpotifycompactoverlay"><button title="Mini view" class="control-button">'
        + '<div style="font-family: Segoe MDL2 Assets; position:relative; cursor: default;">'
        + '<div style="left: 6px; top: -3px; font-size: 19px; position: absolute;">&#xE7FB;</div>'
        + '<div style="left: 12px; top: -6px; font-size: 9px; position: absolute;">&#xEB9F;</div>'
        + '</div></button></a>';
    injectNowPlayingRightButton(compactOverlayButton);
}
catch (ex) {
    errors += "injectCompactOverlayFailed,";
}

setTimeout(function () {
    window.location.hash = "xpotifyInitialPage";

    setInterval(function () {
        if (window.location.hash === "#xpotifyInitialPage") {
            backButtonDiv.classList.add("backButtonContainer-disabled");
        } else {
            backButtonDiv.classList.remove("backButtonContainer-disabled");
        }
    }, 500);
}, 1000);

window.addEventListener("resize", onResize, true);  
setInterval(onResize, 2000); // Sometimes an OnResize is necessary when users goes to a new page.

if (errors.length > 0)
    throw errors;
