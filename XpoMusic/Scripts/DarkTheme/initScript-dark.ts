/// <reference path="../Common/initScript-common.ts" />

namespace XpoMusicScript {

    document.getElementsByTagName('body')[0].setAttribute('data-xpotifyTheme', 'dark');

    var errors = "";

    errors += Common.init();

    if (errors.length > 0) {
        try {
            // @ts-ignore
            XpoMusic.initFailed(errors);
        }
        catch (ex) { }

        throw errors;
    }
}