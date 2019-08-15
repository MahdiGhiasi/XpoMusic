using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XpoMusic.Classes.Model.WebResourceModifications
{
    public class WebResourceStringModificationRule
    {
        public string RegexMatch { get; set; }
        public string ReplaceTo { get; set; }

        public string Apply(string responseString) => Regex.Replace(responseString, RegexMatch, ReplaceTo);
    }
}