using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.WebAgent.Model
{
    class WebAgentMessage
    {
        public string objectName { get; set; }
        public string methodName { get; set; }
        public Guid promiseGuid { get; set; }
        public Dictionary<string, object> data { get; set; }
    }
}
