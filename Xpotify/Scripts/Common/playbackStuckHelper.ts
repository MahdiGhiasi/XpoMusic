namespace XpotifyScript.Common.PlaybackStuckHelper
{
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    async function resolveStart() {
        var initialElapsedTime = StatusReport.getElapsedTime();

        if (Action.nextTrack() == '0')
            return;

        while (!StatusReport.getIsPlaying() || StatusReport.getElapsedTime() === 0 || StatusReport.getElapsedTime() === initialElapsedTime)
            await sleep(500);
        
        if (Action.prevTrack() == '0')
            return;

        do {
            await sleep(500);
        } while (!StatusReport.getIsPlaying())
        
        if (Action.prevTrack() == '0')
            return;

        var newInitialElapsedTime = StatusReport.getElapsedTime();
        while (!StatusReport.getIsPlaying() || StatusReport.getElapsedTime() === 0 || StatusReport.getElapsedTime() === newInitialElapsedTime)
            await sleep(100);
    }

    export async function tryResolveStart() {
        // @ts-ignore
        if (window.xpotifyStuckStartResolvingInProgress === true)
            return;

        // @ts-ignore
        window.xpotifyStuckStartResolvingInProgress = true;

        var volume = StatusReport.getVolume();
        try {
            Action.seekVolume(0);

            await resolveStart();
        } finally {
            Action.seekVolume(volume);

            // @ts-ignore
            window.xpotifyStuckStartResolvingInProgress = false;
        }
    }


    export async function tryResolveMiddle() {
        var elapsedTime = StatusReport.getElapsedTime();
        var totalTime = StatusReport.getTotalTime();

        var changeTime = Math.max(1000, totalTime / 200);

        var newElapsedTime = elapsedTime - changeTime;
        if (newElapsedTime <= 0)
            newElapsedTime = elapsedTime + changeTime;

        Action.seekPlayback(newElapsedTime / totalTime);

        await sleep(1000);
        Action.play();
    }
}
