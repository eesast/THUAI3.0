using Communication.Proto;
using Communication.RestServer.Models;
using Communication.RestServer.Services.Interfaces;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using static Communication.RestServer.Services.ServiceProvider;

namespace Communication.RestServer.Services
{
    internal sealed class CommService : ICommService, IDisposable
    {
        private readonly IDServer server = new IDServer();
        private static readonly TimeSpan HeartbeatTimeout = new TimeSpan(0, 0, 0, 0, Constants.HeartbeatInternal);
        private readonly Dictionary<int, DockerGame> games = new Dictionary<int, DockerGame>();
        
        private int SearchRoom(string roomID)
        {
            foreach (int add in games.Keys)
                if (games[add].DockerID == roomID)
                    return add;
            return -1;
        }

        public void StartServer(ushort port)
        {
            server.Port = port;
            server.OnReceive += (message) =>
            {
                IMessage content = message.Content;
                if (games.ContainsKey(message.Address))
                    games[message.Address].LastAlive = DateTime.Now;

                switch (content.GetType().ToString())
                {
                    case "Communication.Proto.LauncherID":
                        games[message.Address] = new DockerGame
                        {
                            DockerID = (content as LauncherID).Id,
                            LastAlive = DateTime.Now,
                            Status = DockerGameStatus.Unknown,
                            Count = 0
                        };
                        break;
                    case "Communication.Proto.GameStatus":
                        GameStatus status = content as GameStatus;
                        games[message.Address].Status = (DockerGameStatus)status.Status;
                        games[message.Address].Count = status.ClientCount;
                        break;
                }
            };
            server.Start();
        }

        public void CheckDead()
        {
            DateTime now = DateTime.Now;
            List<int> terminate = new List<int>();
            foreach (KeyValuePair<int, DockerGame> pair in games)
                if (now - pair.Value.LastAlive > HeartbeatTimeout || pair.Value.Status == DockerGameStatus.PendingTerminated)
                    terminate.Add(pair.Key);
            foreach (int id in terminate)
            {
                Get<IDockerService>().TerminateDocker(games[id].DockerID);
                games.Remove(id);
            }
        }

        public void Dispose()
        {
            server.Dispose();
        }

        public List<Tuple<string, int>> QueryRoom()
        {
            return games.Select((game) => new Tuple<string, int>(game.Value.DockerID, game.Value.Count)).ToList();
        }

        public void JoinRoom(string roomID, string fileID, byte[] file)
        {
            //TODO: use dictionary implement
            server.Send(new Message
            {
                Address = SearchRoom(roomID),
                Content = new ClientFile
                {
                    ClientID = fileID,
                    Data = ByteString.CopyFrom(file)
                }
            });
        }

        public DockerGameStatus GetStatus(string roomID)
        {
            return games[SearchRoom(roomID)].Status;
        }
    }
}
