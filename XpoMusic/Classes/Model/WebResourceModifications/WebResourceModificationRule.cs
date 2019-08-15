using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XpoMusic.Classes.Model.WebResourceModifications
{
    public class WebResourceModificationRule
    {
        public string UriRegexMatch { get; set; }
        public WebResourceModificationRuleType Type { get; set; }
        public string AlternativeFileUri { get; set; }
        public WebResourceStringModificationRule[] StringModificationRules { get; set; }

        public bool Match(string requestUri) => Regex.IsMatch(requestUri, UriRegexMatch);
    }
}
