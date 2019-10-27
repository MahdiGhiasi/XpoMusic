namespace XpoMusicScript.Common.ThemedScrollbar {

    var currentScrollbars = [];

    function getRandomId() {
        return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
    }

    function scrollContainerMainScroll(event) {
        var element = event.currentTarget;
        var id = element.getAttribute('data-xpo-scroll-unique-id');

        window.clearTimeout(currentScrollbars[id].mainElementTimeout);

        var overlay = currentScrollbars[id].overlayElement;

        overlay.removeEventListener('scroll', scrollContainerOverlayScroll);
        overlay.scrollTop = (overlay.scrollHeight - overlay.clientHeight) * (element.scrollTop / (element.scrollHeight - element.clientHeight));

        currentScrollbars[id].mainElementTimeout = setTimeout(function () {
            currentScrollbars[id].overlayElement.addEventListener('scroll', scrollContainerOverlayScroll);
        }, 500);
    }

    function scrollContainerOverlayScroll(event) {
        var overlay = event.currentTarget;
        var id = overlay.getAttribute('data-xpo-scroll-unique-id');

        window.clearTimeout(currentScrollbars[id].overlayElementTimeout);

        var element = currentScrollbars[id].mainElement;

        element.removeEventListener('scroll', scrollContainerMainScroll);
        element.scrollTop = (element.scrollHeight - element.clientHeight) * (overlay.scrollTop / (overlay.scrollHeight - overlay.clientHeight));

        currentScrollbars[id].overlayElementTimeout = setTimeout(function () {
            currentScrollbars[id].mainElement.addEventListener('scroll', scrollContainerMainScroll);
        }, 500);
    }

    function insertAfter(newNode, referenceNode) {
        referenceNode.parentNode.insertBefore(newNode, referenceNode.nextSibling);
    }

    export function initScrollbar(elementsSelector, updateInterval, positionEvaluatorFunction = undefined) {
        var elements = document.querySelectorAll(elementsSelector);

        for (var i = 0; i < elements.length; i++) {
            var element: HTMLElement = elements[i];

            var uniqueId = getRandomId();

            element.classList.add("xpo-scroll-container");
            element.setAttribute('data-xpo-scroll-unique-id', uniqueId);
            element.addEventListener('scroll', scrollContainerMainScroll);

            var overlay = document.createElement('div');
            overlay.classList.add("xpo-scroll-container");
            overlay.classList.add("xpo-extra-scrollbar-fixed");
            overlay.setAttribute('data-xpo-scroll-unique-id', uniqueId);
            overlay.addEventListener('scroll', scrollContainerOverlayScroll);

            var overlayContent = document.createElement('div');
            overlayContent.style.height = element.scrollHeight + "px";

            overlay.append(overlayContent);

            if (positionEvaluatorFunction !== undefined) {
                positionEvaluatorFunction(overlay);
            }

            insertAfter(overlay, element);

            setInterval(function () {
                if (positionEvaluatorFunction !== undefined) {
                    positionEvaluatorFunction(overlay);
                }

                var height = element.scrollHeight / (element.clientHeight / overlay.clientHeight);
                overlayContent.style.height = height + "px";
            }, updateInterval);

            currentScrollbars[uniqueId] = {
                mainElement: element,
                overlayElement: overlay,
                overlayContentElement: overlayContent,
                mainElementTimeout: undefined,
                overlayElementTimeout: undefined,
            };
        }
    }


}