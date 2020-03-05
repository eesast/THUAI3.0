using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Proto
{
    internal enum DockerGameStatus
    {
        Unknown = -1,
        Waiting = 0,
        Competing = 1,
        Finish = 2
    }
}
