using Communication.Proto;
using System;
using System.Collections.Generic;

namespace Communication.RestServer.Services.Interfaces
{
    interface ICommService
    {
        void CheckDead();
        void StartServer(ushort port);
        List<Tuple<string, int>> QueryRoom();
        void JoinRoom(string roomID, string fileID, byte[] file);
        DockerGameStatus GetStatus(string roomID);
    }
}
