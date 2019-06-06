
function navigateToPage(url) {
    history.pushState({}, null, url);
    history.pushState({}, null, url + "#navigatingToPagePleaseIgnore");
    history.back();

    // TODO: Remove the extra history entry from history

    // TODO: Check which Navigation events are being invoked from webView control.
}
