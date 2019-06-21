namespace XpotifyScript.Common.Resize {
    export function onResize() {
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
            (<HTMLElement>contentDiv[0]).style.width = 230 + (window.innerWidth - (<HTMLElement>document.querySelectorAll(".Root__nav-bar")[0]).offsetWidth) + "px";


            var adContainerDiv = document.querySelectorAll(".AdsContainer");
            if (adContainerDiv.length > 0)
                (<HTMLElement>adContainerDiv[0]).style.width = (window.innerWidth - (<HTMLElement>document.querySelectorAll(".Root__nav-bar")[0]).offsetWidth) + "px";
        }
        catch (ex) {
            console.log("resize event failed");
        }
    }
}