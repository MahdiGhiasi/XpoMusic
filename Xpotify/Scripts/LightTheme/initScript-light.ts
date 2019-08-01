/// <reference path="../Common/initScript-common.ts" />
/// <reference path="pageOverlay.ts" />

namespace XpoMusicScript {

    document.getElementsByTagName('body')[0].setAttribute('data-xpotifyTheme', 'light');

    var errors = "";

    errors += Common.init();
    errors += Light.PageOverlay.createPageOverlay();

    if (errors.length > 0) {
        try {
            // @ts-ignore
            Xpotify.initFailed(errors);
        }
        catch (ex) { }

        throw errors;
    }
}