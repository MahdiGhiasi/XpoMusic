namespace XpoMusicScript.Common.WebPlayerBackup {

    export function runWebPlayerBackup() {
        if (document.querySelectorAll("#main").length > 0) {
            window.XpoMusic.Log("runWebPlayerBackup requested but #main already exists.");
            return;
        }
        window.XpoMusic.Log("web-player-backup is not present in the current script version.");
    }
}