﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.WebAgent.Model
{
    public sealed class StatusReportReceivedEventArgs
    {
        public WebAppStatus Status { get; set; }
    }
}
