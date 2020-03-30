namespace XpoMusicScript.Common.WebPlayerBackup {

    declare var XpoMusic: any;

    export function runWebPlayerBackup() {
        if (document.querySelectorAll("#main").length > 0) {
            XpoMusic.log("runWebPlayerBackup requested but #main already exists.");
            return;
        }
        XpoMusic.log("web-player-backup is not present in the current script version.");
    }
}