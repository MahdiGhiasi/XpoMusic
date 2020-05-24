﻿namespace XpoMusicScript.Common.Color {
    declare var Vibrant: any;

    export function setNowPlayingBarColor(url, lightTheme) {
        try {
            var img = document.createElement('img');
            img.crossOrigin = 'anonymous';
            img.setAttribute('src', url);

            img.addEventListener('load', function () {
                try {
                    var vibrant = new Vibrant(img);
                    var swatches = vibrant.swatches();

                    var brigtnessFactor = 0.6;
                    var opacity = 0.7;
                    var rgb = swatches.Muted.getRgb();
                    if (swatches.Muted.getPopulation() < swatches.Vibrant.getPopulation()) {
                        rgb = swatches.Vibrant.getRgb();
                    }

                    if (lightTheme) {
                        rgb[0] = 255 - rgb[0];
                        rgb[1] = 255 - rgb[1];
                        rgb[2] = 255 - rgb[2];

                        brigtnessFactor = 0.7;
                        opacity = 0.6;
                    }

                    rgb[0] = Math.max(brigtnessFactor * rgb[0], 0);
                    rgb[1] = Math.max(brigtnessFactor * rgb[1], 0);
                    rgb[2] = Math.max(brigtnessFactor * rgb[2], 0);

                    (<HTMLElement>document.querySelectorAll(".Root__now-playing-bar .now-playing-bar")[0]).style.backgroundColor = "rgba(" + rgb[0] + ", " + rgb[1] + ", " + rgb[2] + ", " + opacity + ")";
                }
                catch (ex2) {
                    console.log("setNowPlayingBarColor failed (2)");
                    console.log(ex2);
                }
            });
        }
        catch (ex) {
            console.log("setNowPlayingBarColor failed (1)");
            console.log(ex);
        }
    }

    export function addXpotifyClassToBackground(retryCount) {
        if (retryCount < 0)
            return;

        var rootElement = document.querySelectorAll(".Root__top-container");
        if (rootElement.length === 0) {
            setTimeout(function () {
                addXpotifyClassToBackground(retryCount - 1);
            }, 250);
        } else {
            (<HTMLElement>rootElement[0].previousSibling).setAttribute('data-xpoBackground', '1');
        }
    }

    export function addXpotifyClassToSectionBackground(retryCount) {
        if (retryCount < 0)
            return;

        var elements = document.querySelectorAll(".main-view-container__scroll-node-child > section > div:nth-child(1), .main-view-container__scroll-node-child > section > div:nth-child(2)");
        if (elements.length <= 1) {
            setTimeout(function () {
                addXpotifyClassToSectionBackground(retryCount - 1);
            }, 250);
        } else {
            (<HTMLElement>elements[0]).setAttribute('data-xpoBackground', '1');
            (<HTMLElement>elements[1]).setAttribute('data-xpoBackground', '1');
        }
    }

}