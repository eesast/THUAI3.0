using System.Collections.Concurrent;
using System.Threading;
using Communication.Proto;
using System;
using System.Reflection;
using System.Net;
using Google.Protobuf;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace Communication.Server
{
    public sealed class CommunicationImpl : ICommunication
    {
        private readonly IDServer server = new IDServer();
        private readonly ManualResetEvent full = new ManualResetEvent(false);
        private string roomID, token;
        private static readonly IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        private static readonly IJsonSerializer serializer = new JsonNetSerializer();
        private static readonly IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        private static readonly IDateTimeProvider provider = new UtcDateTimeProvider();
        private readonly JwtDecoder decoder = new JwtDecoder(serializer, new JwtValidator(serializer, provider), urlEncoder, algorithm);
        
        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                roomID = (string) JObject.Parse(decoder.Decode(value))["roomID"];
                token = value;
            }
        }

        private async Task HttpAsync(string uri, string token, string method, JObject data)
        {
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Method = method;
                request.Headers.Add("Authorization", $"bearer {token}");
                if (data != null)
                {
                    request.ContentType = "application/json";
                    var raw = Encoding.UTF8.GetBytes(data.ToString());
                    request.GetRequestStream().Write(raw, 0, raw.Length);
                }
                
                var response = await request.GetResponseAsync() as HttpWebResponse;
                if ((int)response.StatusCode / 100 != 2)
                    Constants.Debug(response.StatusDescription);
            }
            catch (Exception e)
            {
                Constants.Debug(e.ToString());
            }
        }

        private async Task NoticeServer(string token, DockerGameStatus status)
        {
            if (IsOffline) return;
            if (token == null)
            {
                await HttpAsync($"http://localhost:28888/v1/rooms/{roomID}", this.token, "PUT", new JObject
                {
                    ["status"] = (int)status
                });
            }
            else
            {
                await HttpAsync($"http://localhost:28888/v1/rooms/{roomID}/join", token, "GET", null);
            }
        }

        private DockerGameStatus Status
        {
            set
            {
                Task.Run(() => NoticeServer(null, value));
            }
        }

        public int PlayerCount => server.Count;

        public ushort ServerPort
        {
            get => server.Port;
            set => server.Port = value;
        }
        public bool IsOffline { get; set; }

        public event MessageHandler MsgProcess;

        public void OnNewMessage(MessageEventArgs e)
        {
            MsgProcess?.Invoke(this, e);
        }
        public void GameOver()
        {
            Status = DockerGameStatus.Finish;
            server.Stop();
        }

        public void GameStart()
        {
            server.OnReceive += delegate (Message message) //收到信息后将消息传给逻辑处理
            {
                if (message.Content is PlayerToken token)
                {
                    NoticeServer(token.Token, default).Wait();
                    Constants.Debug($"Agent Connected: {server.Count}/{Constants.AgentCount}");
                    if (server.Count == Constants.AgentCount)
                    {
                        full.Set();
                        server.Pause();
                    }
                    return;
                }
                ServerMessage msg = new ServerMessage();
                msg.Agent = message.Address;
                message = message.Content as Message;
                msg.Client = message.Address;
                msg.Message = message.Content;
                MessageEventArgs e = new MessageEventArgs(msg);
                OnNewMessage(e);
            };

            server.InternalQuit += delegate ()
            {
                server.Resume();
            };
            full.Reset();
            server.Start();
            Constants.Debug("Waiting for clients");
            full.WaitOne();
            //FIXME: 现在似乎有的时候会先set状态competing再join，不知道会不会有什么影响
            Status = DockerGameStatus.Competing;
        }

        public void Initialize()
        {
            Status = DockerGameStatus.Waiting;
        }

        public void SendMessage(ServerMessage message)
        {
            Message msg = new Message()
            {
                Address = message.Agent, //封包Agent
                Content = new Message()
                {
                    Address = message.Client, //封包Cient
                    Content = message.Message
                }
            };
            server.Send(msg);
        }

        public void Dispose()
        {
            server.Dispose();
            full.Dispose();
        }
    }
}
