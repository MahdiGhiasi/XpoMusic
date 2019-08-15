using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using XpoMusic.Classes;
using XpoMusic.Classes.Model.WebResourceModifications;

namespace XpoMusic.Helpers
{
    public static class WebResourceModificationHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static async Task<HttpResponseMessage> WebResourceRequested(HttpRequestMessage request)
        {
            if (AppConstants.Instance.ModificationRules == null)
                return null;

            var requestUri = request.RequestUri.ToString();

            foreach (var rule in AppConstants.Instance.ModificationRules)
            {
                if (!rule.Match(requestUri))
                    continue;

                if (rule.Type == WebResourceModificationRuleType.ReplaceWholeFile)
                {
                    return await DownloadFile(new HttpRequestMessage(request.Method, new Uri(rule.AlternativeFileUri)));
                }
                else if (rule.Type == WebResourceModificationRuleType.ModifyString)
                {
                    var response = await DownloadFile(request);

                    if (!response.IsSuccessStatusCode)
                        return response;

                    var responseBuffer = await response.Content.ReadAsBufferAsync();
                    var responseByteArray = responseBuffer.ToArray();
                    var responseString = Encoding.UTF8.GetString(responseByteArray, 0, responseByteArray.Length);

                    foreach (var stringModificationRule in rule.StringModificationRules)
                        responseString = stringModificationRule.Apply(responseString);

                    var newResponse = new HttpResponseMessage(response.StatusCode)
                    {
                        RequestMessage = request,
                    };
                    if (response?.Content?.Headers?.ContentType?.MediaType != null)
                        newResponse.Content = new HttpStringContent(responseString, Windows.Storage.Streams.UnicodeEncoding.Utf8, response.Content.Headers.ContentType.MediaType);
                    else
                        newResponse.Content = new HttpStringContent(responseString, Windows.Storage.Streams.UnicodeEncoding.Utf8);

                    return newResponse;
                }
            }

            return null;
        }

        private static async Task<HttpResponseMessage> DownloadFile(HttpRequestMessage request)
        {
            using (var httpClient = new HttpClient())
            {
#if DEBUG
                var sw = new Stopwatch();
                sw.Start();
#endif
                var response = await httpClient.SendRequestAsync(request);
#if DEBUG
                sw.Stop();
                logger.Debug($"Getting {request.RequestUri} took {sw.ElapsedMilliseconds} ms");
#endif
                return response;
            }
        }

    }
}
