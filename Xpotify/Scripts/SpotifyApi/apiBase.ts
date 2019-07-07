namespace XpotifyScript.SpotifyApi {

    declare var Xpotify: any;

    export abstract class ApiBase {
        protected accessToken: string = "{{SPOTIFYACCESSTOKEN}}";

        protected async sendJsonRequestWithToken(uri, method, body = undefined): Promise<Response> {
            if (this.accessToken.length === 0) {
                this.accessToken = await Xpotify.getNewAccessTokenAsync();
            }

            return await this.sendJsonRequestWithTokenInternal(uri, method, body, true);
        }

        private async sendJsonRequestWithTokenInternal(uri, method, body, allowRefreshingToken): Promise<Response> {
            var response = await fetch(uri, {
                method: method,
                body: JSON.stringify(body),
                headers: {
                    'Authorization': 'Bearer ' + this.accessToken,
                },
            });

            Xpotify.log("SpotifyApi: " + uri + " (" + method + ") -> result status = " + response.status);

            if (response.status == 401 && allowRefreshingToken) {
                // Refresh access token and retry
                Xpotify.log("Will ask for new token.");
                this.accessToken = await Xpotify.GetNewAccessTokenAsync();
                return await this.sendJsonRequestWithTokenInternal(uri, method, body, false);
            }

            return response;
        }
    }
}