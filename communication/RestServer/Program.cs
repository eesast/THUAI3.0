using System.Threading;
using Communication.RestServer.Services;
using Communication.RestServer.Services.Interfaces;
using static Communication.RestServer.Services.ServiceProvider;

namespace Communication.RestServer
{
    class Program
    {
        public const ushort TcpServerPort = 1000;
        public const ushort HttpServerPort = 8080;
        private static void RegisterServices()
        {
            Register(new DockerService() as IDockerService);
            Register(new RestService() as IRestService);
            Register(new CommService() as ICommService);
            Register(new FileService() as IFileService);
            Register(new AuthService() as IAuthService);
        }
        public static void Main(string[] args)
        {
            RegisterServices();
            Get<IRestService>().StartServer(HttpServerPort);
            Get<ICommService>().StartServer(TcpServerPort);
            Thread.Sleep(int.MaxValue);
        }
    }
}
