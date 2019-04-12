using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.SpotifyApi
{
    public class ApiBase
    {
        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Task<HttpResponseMessage> SendRequestWithTokenAsync(string url, HttpMethod httpMethod)
        {
            return SendRequestWithTokenAsync(url, httpMethod, new Dictionary<string, string>(), canUseRefreshToken: true);
        }

        public Task<HttpResponseMessage> SendRequestWithTokenAsync(string url, HttpMethod httpMethod, Dictionary<string, string> data)
        {
            return SendRequestWithTokenAsync(url, httpMethod, data, canUseRefreshToken: true);
        }

        private async Task<HttpResponseMessage> SendRequestWithTokenAsync(string url, HttpMethod httpMethod, Dictionary<string, string> data, bool canUseRefreshToken)
        {
            try
            {
                var httpClient = new HttpClient();

                HttpRequestMessage msg = new HttpRequestMessage(httpMethod, url);
                msg.Headers.Clear();
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenHelper.GetTokens().AccessToken);
                msg.Content = new FormUrlEncodedContent(data.Select(x => x).ToList());

                var response = await httpClient.SendAsync(msg);

                if ((response.IsSuccessStatusCode == false) && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized))
                    throw new UnauthorizedAccessException();

                if (response.IsSuccessStatusCode)
                    logger.Info($"Request to {url} returned with status code {response.StatusCode}.");
                else
                    logger.Warn($"Request to {url} returned with status code {response.StatusCode}.");

                return response;
            }
            catch (UnauthorizedAccessException)
            {
                if (!canUseRefreshToken)
                    throw;

                await TokenHelper.GetAndSaveNewTokenAsync();
                return await SendRequestWithTokenAsync(url, httpMethod, data, false);
            }
        }

        public Task<HttpResponseMessage> SendJsonRequestWithTokenAsync(string url, HttpMethod httpMethod, string data)
        {
            return SendJsonRequestWithTokenAsync(url, httpMethod, data, canUseRefreshToken: true);
        }

        private async Task<HttpResponseMessage> SendJsonRequestWithTokenAsync(string url, HttpMethod httpMethod, string data, bool canUseRefreshToken)
        {
            try
            {
                var httpClient = new HttpClient();

                HttpRequestMessage msg = new HttpRequestMessage(httpMethod, url);
                msg.Headers.Clear();
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenHelper.GetTokens().AccessToken);
                msg.Content = new StringContent(data, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(msg);

                if ((response.IsSuccessStatusCode == false) && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized))
                    throw new UnauthorizedAccessException();

                return response;
            }
            catch (UnauthorizedAccessException)
            {
                if (!canUseRefreshToken)
                    throw;

                await TokenHelper.GetAndSaveNewTokenAsync();
                return await SendJsonRequestWithTokenAsync(url, httpMethod, data, false);
            }
        }
    }
}
