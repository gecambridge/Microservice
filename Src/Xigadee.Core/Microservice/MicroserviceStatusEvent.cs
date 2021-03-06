﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This class is used to log status change for the Microservice during start up and stop requests.
    /// </summary>
    public class MicroserviceStatusEventArgs:EventArgs
    {
        public MicroserviceStatusEventArgs(MicroserviceComponentStatusChangeAction status, string title)
        {
            Status = status;
            Title = title;
        }

        public MicroserviceComponentStatusChangeAction Status { get;}

        public string Title { get; }

        public MicroserviceComponentStatusChangeState State { get; set; } =  MicroserviceComponentStatusChangeState.Beginning;

        public MicroserviceStatusChangeException Ex { get; set; }

        public string Debug()
        {
            return $"{Status}: {Title} = {State}";
        }
    }

    public enum MicroserviceComponentStatusChangeAction
    {
        Starting,
        Stopping
    }

    public enum MicroserviceComponentStatusChangeState
    {
        Beginning,
        Completed,
        Failed
    }
}
