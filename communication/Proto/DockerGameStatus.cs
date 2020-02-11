using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Proto
{
    internal enum DockerGameStatus
    {
        Unknown = -1,
        Idle = 0,
        Listening = 1,
        Heartbeat = 2,
        PendingTerminated = 3
    }
}
