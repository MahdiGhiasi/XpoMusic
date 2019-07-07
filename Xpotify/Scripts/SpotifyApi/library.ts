namespace XpotifyScript.SpotifyApi {
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
            return result;
        }
    }
}