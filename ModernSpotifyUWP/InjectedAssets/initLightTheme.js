
// Mark page as injected
var body = document.getElementsByTagName('body')[0];
body.setAttribute('data-scriptinjection-lighttheme', 1);

errors = "";

// Inject css
try {
    var css = '{{CSSBASE64CONTENT}}';
    var style = document.createElement('style');
    document.getElementsByTagName('head')[0].appendChild(style);
    style.type = 'text/css';
    style.appendChild(document.createTextNode(atob(css)));
}
catch (ex) {
    errors += "injectLightCssFailed,";
}


if (errors.length > 0)
    throw errors;
