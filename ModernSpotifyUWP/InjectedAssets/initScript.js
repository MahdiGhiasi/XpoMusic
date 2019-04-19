
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
    var navbar = document.querySelectorAll(".navBar");
    var sessionInfo = document.querySelectorAll(".navBar .sessionInfo");
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

// Mark page as injected
var body = document.getElementsByTagName('body')[0];
body.setAttribute('data-scriptinjection', 1);

// Inject style.css
var customStyleLink = document.createElement('link');
customStyleLink.rel = 'stylesheet';
customStyleLink.type = 'text/css';
customStyleLink.href = 'ms-appx-web:///InjectedAssets/style.css';
document.getElementsByTagName('head')[0].appendChild(customStyleLink);

// Inject back button
var backButtonDiv = document.createElement('div');
backButtonDiv.innerHTML = "<a class='backbutton' href='#xpotifygoback'><span>&#xE72B;</span></a>";

injectBackButton(backButtonDiv);

// Inject navbar buttons
var pinToStartButton = document.createElement('div');
pinToStartButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifypintostart-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifypintostart">'
    + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE718;</span></div>'
    + '<span class="navbar-link__text">Pin this page to start</span></div></a></div>';
var settingsButton = document.createElement('div');
settingsButton.innerHTML = '<div class="navBar-item navBar-item--with-icon-left NavBar__xpotifysettings-item"><a class="link-subtle navBar-link ellipsis-one-line" href="#xpotifysettings">'
    + '<div class="navBar-link-text-with-icon-wrapper"><div class="icon segoe-icon NavBar__icon"><span style="font-family:Segoe MDL2 Assets;">&#xE170;</span></div>'
    + '<span class="navbar-link__text">About Xpotify</span></div></a></div>';
injectNavbarDownButton(pinToStartButton);
injectNavbarDownButton(settingsButton);

// Inject compact overlay button to now playing
var compactOverlayButton = document.createElement('div');
compactOverlayButton.className = "CompactOverlayButton";
compactOverlayButton.innerHTML = '<a style="border-bottom: 0px;" href="#xpotifycompactoverlay"><button title="Mini view" class="control-button">'
    + '<div style="font-family: Segoe MDL2 Assets; position:relative; cursor: default;">'
    + '<div style="left: 6px; top: -3px; font-size: 19px; position: absolute;">&#xE7FB;</div>'
    + '<div style="left: 12px; top: -6px; font-size: 9px; position: absolute;">&#xEB9F;</div>'
    + '</div></button></a>';
injectNowPlayingRightButton(compactOverlayButton);
