/// <reference path="indeterminateProgressBarHandler.ts" />

namespace XpoMusicScript.Common.RequestIntercepter {

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
            } else if (args[0].toString().endsWith('/v1/devices')) {
                console.log(args);
                console.log(args[1].body);
                var appName = Common.getAppName();
                var spotifyConnectName = Common.getDeviceName() + ' (' + appName + ')';

                // On 1809+, we try to change 'Web Player([browser])' to 'Xpo Music' in web-player.xxx.js. But it might fail
                // in case of code change, and also this feature is not supported on 1803 and below, so we replace both
                // 'Web Player (Microsoft Edge)' and 'Xpo Music' to the desired name.
                args[1].body = args[1].body.toString().replace('Web Player (Microsoft Edge)', spotifyConnectName).replace('Xpo Music', spotifyConnectName);
            }

            // Sending the real request
            var result = await fetch(...args);

            // Response interception

            // Returning response
            return result;
        })(args);
    }

}