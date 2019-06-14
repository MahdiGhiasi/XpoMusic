/// <reference path="uriHelper.ts" />

namespace InitScript.Common.DragDrop {

    export function allowDrop(event) {
        event.preventDefault();
    }

    export function drop(event) {
        var data = event.dataTransfer.getData("Text");
        var uri = UriHelper.getPwaUri(data);

        if (uri === undefined || uri.length === 0) {
            return;
        }

        event.preventDefault();

        // Navigate to page
        history.pushState({}, null, uri);
        history.pushState({}, null, uri + "#navigatingToPagePleaseIgnore");
        history.back();
    }

}