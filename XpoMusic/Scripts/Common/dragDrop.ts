/// <reference path="uriHelper.ts" />

namespace XpoMusicScript.Common.DragDrop {

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
        Action.navigateToPage(uri);
    }

}