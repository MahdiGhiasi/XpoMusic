/// <reference path="../Common/initScript-common.ts" />

namespace InitScript {

    document.getElementsByTagName('body')[0].setAttribute('data-xpotifyTheme', 'dark');

    var errors = "";

    errors += Common.init();

    if (errors.length > 0)
        throw errors;
}