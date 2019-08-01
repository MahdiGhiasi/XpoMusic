namespace XpoMusicScript.SpotifyApi {

    declare var XpoMusic: any;
    var accessToken = "{{SPOTIFYACCESSTOKEN}}";

    export abstract class ApiBase {

        protected async sendJsonRequestWithToken(uri, method, body = undefined): Promise<Response> {
            if (accessToken.length === 0) {
                accessToken = await XpoMusic.getNewAccessTokenAsync();
            }

            return await this.sendJsonRequestWithTokenInternal(uri, method, body, true);
        }

        private async sendJsonRequestWithTokenInternal(uri, method, body, allowRefreshingToken): Promise<Response> {
            var response = await fetch(uri, {
                method: method,
                body: JSON.stringify(body),
                headers: {
                    'Authorization': 'Bearer ' + accessToken,
                },
            });

            XpoMusic.log("SpotifyApi: " + uri + " (" + method + ") -> result status = " + response.status);

            if (response.status == 401 && allowRefreshingToken) {
                // Refresh access token and retry
                XpoMusic.log("Will ask for new token.");
                accessToken = await XpoMusic.getNewAccessTokenAsync();
                return await this.sendJsonRequestWithTokenInternal(uri, method, body, false);
            }

            return response;
        }
    }
}