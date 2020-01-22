using System.Net;

namespace Communication.RestServer.Services.Interfaces
{
    interface IDockerService
    {
        string CreateDocker();
        void RunDocker(string DockerID, IPEndPoint server);
        void TerminateDocker(string DockerID);

    }
}
