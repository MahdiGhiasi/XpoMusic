﻿namespace XpoMusicScript.SpotifyApi {

    var accessToken = "{{SPOTIFYACCESSTOKEN}}";

    export abstract class ApiBase {

        protected async sendJsonRequestWithToken(uri, method, body = undefined): Promise<Response> {
            if (accessToken.length === 0) {
                accessToken = await window.XpoMusic.GetNewAccessTokenAsync();
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

            window.XpoMusic.Log("SpotifyApi: " + uri + " (" + method + ") -> result status = " + response.status);

            if (response.status == 401 && allowRefreshingToken) {
                // Refresh access token and retry
                window.XpoMusic.Log("Will ask for new token.");
                accessToken = await window.XpoMusic.GetNewAccessTokenAsync();
                return await this.sendJsonRequestWithTokenInternal(uri, method, body, false);
            }

            return response;
        }
    }
}