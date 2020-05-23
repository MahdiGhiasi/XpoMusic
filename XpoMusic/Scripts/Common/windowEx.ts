export { };

declare global {
    interface Window {
        XpoMusic: any; // WebAgent
        xpo___nowPlayingEnabled: boolean;
    }
}
window.XpoMusic = window.XpoMusic || {};
window.xpo___nowPlayingEnabled == false;
