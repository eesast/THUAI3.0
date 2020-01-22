using Communication.Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.RestServer.Models
{
    internal class DockerGame
    {
        public DockerGameStatus Status;
        public string DockerID;
        public DateTime LastAlive;
        public int Count = 0;
    }
}
