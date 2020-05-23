using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XpoMusic.WebAgent.Model;

namespace XpoMusic.WebAgent
{
    /// <summary>
    /// Classes that are inherited from WebAgentBase can be injected to a WebView2 object, and then accessed from javascript there.
    /// Remember to call Init() on every page load, so the necessary code can be injected.
    /// </summary>
    public abstract class WebAgentBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string objectAccessibleName;
        private readonly WebView2 webView;

        /// <param name="webView">WebView2 object that this object belongs to</param>
        /// <param name="objectAccessibleName">Name of the object inside javascript. It will be accessible in js via window.{objectAccessibleName}</param>
        public WebAgentBase(WebView2 webView, string objectAccessibleName)
        {
            var regex = new Regex("[a-zA-Z_][a-zA-Z0-9_]*");
            var match = regex.Match(objectAccessibleName);

            if (!match.Success)
                throw new Exception($"Invalid value for objectAccessibleName.");

            this.webView = webView;
            this.objectAccessibleName = objectAccessibleName;
        }

        public async Task Init()
        {
            RegisterWebMessageReceiveHandler();
            await RegisterMethods();
        }

        private async Task RegisterMethods()
        {
            var js = new StringBuilder();
            js.AppendLine($"window.{objectAccessibleName} = {{}};");
            js.AppendLine($"window.{objectAccessibleName}.__promises = new Map();");
            js.AppendLine($"window.{objectAccessibleName}.__generateUUID = {UuidGeneratorJsFunction}");

            var methods = this.GetType().GetMethods();
            foreach (var method in methods)
            {
                js.AppendLine(GetJsForMethod(method));
            }

            await webView.ExecuteScriptAsync(js.ToString());
        }

        // From https://stackoverflow.com/a/873856/942659
        private string UuidGeneratorJsFunction =>
            @"
function() {
    // http://www.ietf.org/rfc/rfc4122.txt
    var s = [];
    var hexDigits = '0123456789abcdef';
    for (var i = 0; i < 36; i++)
    {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
    }
    s[14] = '4';  // bits 12-15 of the time_hi_and_version field to 0010
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
    s[8] = s[13] = s[18] = s[23] = '-';

    var uuid = s.join('');
    return uuid;
}
";

        private string GetJsForMethod(MethodInfo method)
        {
            var js = $"window.{objectAccessibleName}.{method.Name} = " +
                $"function({string.Join(',', method.GetParameters().Select(x => x.Name))}) {{ " +
                $"  var promiseUuid = window.{objectAccessibleName}.__generateUUID();" +
                $"  var jsonObj = {{" +
                $"      'objectName': '{objectAccessibleName}'," +
                $"      'methodName': '{method.Name}'," +
                $"      'promiseGuid': promiseUuid," +
                $"      'data': {{}}" +
                $"  }};" +
                string.Join("", method.GetParameters().Select(x => $"jsonObj.data.{x.Name} = {x.Name};")) +
                $"  window.chrome.webview.postMessage(JSON.stringify(jsonObj));" +
                $"  " +
                $"  var promise = new Promise((resolve, reject) => {{" +
                $"    var promiseFunctions = {{'res': resolve, 'rej': reject }};" +
                $"    window.{objectAccessibleName}.__promises.set(promiseUuid, promiseFunctions);" +
                $"  }});" +
                $"  return promise;" +
                $"}}";

            return js;
        }

        private void RegisterWebMessageReceiveHandler()
        {
            webView.WebMessageReceived -= WebView_WebMessageReceived;
            webView.WebMessageReceived += WebView_WebMessageReceived;
        }

        private async void WebView_WebMessageReceived(WebView2 sender, WebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<WebAgentMessage>(e.WebMessageAsString);
                if (message == null)
                    return;
                if (message.objectName != objectAccessibleName)
                    return;

                await ProcessWebMessage(message);
            }
            catch (Exception ex)
            {
                logger.Error($"Exception happened in WebView_WebMessageReceived: {ex}");
            }
        }

        private async Task ProcessWebMessage(WebAgentMessage message)
        {
            var method = GetType().GetMethod(message.methodName);
            var invokeParameters = new List<object>();
            foreach (var param in method.GetParameters())
            {
                var paramVal = message.data[param.Name];
                if (param.ParameterType == typeof(string) && !(paramVal is string))
                    paramVal = paramVal.ToString();
                else if (param.ParameterType != typeof(JObject) && paramVal is JObject jobj)
                    paramVal = jobj.ToObject(param.ParameterType);

                invokeParameters.Add(paramVal);
            }
            var result = method.Invoke(this, invokeParameters.ToArray());

            // If result is a Task (or Task<T>, which inherits Task), wait for it to finish.
            if (result is Task task) 
            {
                await task;

                // if result is a Task<T>, get the result as well.
                if (task.GetType().GenericTypeArguments.Count() > 0) 
                    result = result.GetType().GetProperty("Result").GetValue(result);
                else // otherwise, the result is null.
                    result = null;
           }

            var jsonResult = JsonConvert
                .SerializeObject(result)
                .Replace("'", @"\'");

            var responseJs = $"window.{objectAccessibleName}.__promises.get('{message.promiseGuid}').res(JSON.parse('{jsonResult}'));" +
                $"window.{objectAccessibleName}.__promises.delete('{message.promiseGuid}')";
            await webView.ExecuteScriptAsync(responseJs);
        }
    }
}
