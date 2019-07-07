/// <reference path="indeterminateProgressBarHandler.ts" />

namespace XpotifyScript.Common.RequestIntercepter {

    export function startInterceptingFetch() {

        const fetch = window.fetch;
        window.fetch = (...args) => (async (args) => {

            // Request interception
            if (args[0].toString().endsWith('/state')) {
                console.log(args);
                var reqJson = JSON.stringify(args);
                if (reqJson.indexOf("before_track_load") >= 0) {
                    IndeterminateProgressBarHandler.onTrackLoadBegin();
                }
            }

            // Sending the real request
            var result = await fetch(...args);

            // Response interception

            // Returning response
            return result;
        })(args);
    }

}