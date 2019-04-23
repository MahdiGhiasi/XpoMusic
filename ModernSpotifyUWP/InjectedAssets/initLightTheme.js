
// Mark page as injected
var body = document.getElementsByTagName('body')[0];
body.setAttribute('data-scriptinjection-lighttheme', 1);

// Inject style.css
var customStyleLink = document.createElement('link');
customStyleLink.rel = 'stylesheet';
customStyleLink.type = 'text/css';
customStyleLink.href = 'ms-appx-web:///InjectedAssets/style-light.css';
document.getElementsByTagName('head')[0].appendChild(customStyleLink);
