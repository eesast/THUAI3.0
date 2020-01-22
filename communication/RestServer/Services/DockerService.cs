using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using Communication.RestServer.Services.Interfaces;

namespace Communication.RestServer.Services
{
    internal class DockerService : IDockerService
    {
        //TODO: use docker implement
        private readonly Dictionary<string, Process> procs = new Dictionary<string, Process>();

        public DockerService()
        {

        }

        public string CreateDocker()
        {
            string id = Guid.NewGuid().ToString();
            return id;
        }

        public void RunDocker(string DockerID, IPEndPoint server)
        {
            //测试用
            const string serverFile = "D:\\THUAI3.0\\communication\\docker\\Communication.ServerChatTest.exe";
            procs.Add(DockerID, Process.Start(serverFile, $"{server} {DockerID}"));
        }

        public void TerminateDocker(string DockerID)
        {
            procs[DockerID].Kill();
        }
    }
}
