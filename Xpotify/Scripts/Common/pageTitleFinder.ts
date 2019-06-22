namespace XpotifyScript.Common.PageTitleFinder {

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
            } else if (candidates[i].classList.length == 2) {
                if (selIndex == -1) {
                    selIndex = i;
                } else {
                    return '';
                }
            } else {
                return '';
            }
        }

        if (selIndex == -1) {
            return '';
        } else {
            return titleCase((<HTMLElement>candidates[selIndex]).innerText);
        }
    }

    function searchForHTag(selector) {
        var candidates = document.querySelectorAll(".Root__main-view " + selector);
        for (var i = 0; i < candidates.length; i++) {
            var s = (<HTMLElement>candidates[i]).innerText;
            if (s.length > 0 && s.length < 80) {
                return s;
            }
        }
        return '';
    }

    export function getTitle() {
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
            } else {
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
}
