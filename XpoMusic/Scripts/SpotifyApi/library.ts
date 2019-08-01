namespace XpoMusicScript.SpotifyApi {
    export class Library extends ApiBase {
        public async isTrackSaved(trackId: string): Promise<boolean> {
            return (await this.isTracksSaved([trackId]))[0];
        }

        public async isTracksSaved(trackIds: string[]): Promise<boolean[]> {
            var output = [];
            for (var i = 0; i < trackIds.length; i += 50) {
                var slice = trackIds.slice(i, i + 50);
                output = output.concat(await this.isTracksSavedInternal(slice));
            }
            return output;
        }

        private async isTracksSavedInternal(trackIds: string[]): Promise<boolean[]> {
            var url = 'https://api.spotify.com/v1/me/tracks/contains?ids=' + trackIds.join(',');
            var result = await this.sendJsonRequestWithToken(url, 'get');

            var data = await result.json();
            return data;
        }

        public async saveTrack(trackId: string): Promise<boolean> {
            var url = "https://api.spotify.com/v1/me/tracks?ids=" + trackId;
            var result = await this.sendJsonRequestWithToken(url, 'put');

            return result.status >= 200 && result.status <= 299;
        }

        public async removeTrack(trackId: string): Promise<boolean> {
            var url = "https://api.spotify.com/v1/me/tracks?ids=" + trackId;
            var result = await this.sendJsonRequestWithToken(url, 'delete');

            return result.status >= 200 && result.status <= 299;
        }
    }
}