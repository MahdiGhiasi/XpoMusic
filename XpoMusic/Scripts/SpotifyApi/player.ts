/// <reference path="apiBase.ts" />

namespace XpoMusicScript.SpotifyApi {
    export class Player extends ApiBase {
        public async getCurrentlyPlaying(): Promise<any> {
            var url = 'https://api.spotify.com/v1/me/player';
            var result = await this.sendJsonRequestWithToken(url, 'get');

            var data = await result.json();
            return data;
        }
    }
}