namespace InitScript.Light.PageOverlay {

    export function createPageOverlay() {
        try {
            var body = <HTMLElement>document.getElementsByTagName('body')[0];
            var overlayDiv = document.createElement('div');
            overlayDiv.classList.add("whole-page-overlay");
            body.appendChild(overlayDiv);
        }
        catch (ex) {
            return "injectOverlayFailed,";
        }
        return "";
    }
}