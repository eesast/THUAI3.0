using Communication.Proto;
using Communication.Server;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace ServerChatTest
{
    class Program
    {
        // 示例消息处理
        static private void PrintChatMessage(Object com, EventArgs e)
        {
            CommunicationImpl comm = com as CommunicationImpl;
            MessageEventArgs ev = e as MessageEventArgs;
            ServerMessage message = ev.message;
            ChatMessage chat = message.Message as ChatMessage;
            if (chat.Message.StartsWith("/stop")) //测试主动断开连接功能
            {
                comm.GameOver();
            }
            else
            {
                chat.Message = $"[From Player ({message.Agent}, {message.Client})] " + chat.Message + "\n";
                message.Client = -2; //broadcast
                message.Agent = -2; //broadcast
                comm.SendMessage(message);
            }
        }

        public static void Main(string[] args)
        {
            //这里没有进行更改，因为不知道api还不能进行测试
            using ICommunication comm = new CommunicationImpl();
            //string[] t = args[0].Split(':');
            //Console.WriteLine(args[0]);
            //comm.EndPoint = new IPEndPoint(IPAddress.Parse(t[0]), ushort.Parse(t[1]));
            //comm.ID = args[1];
            comm.ServerPort = Constants.ServerPort;
            comm.Token = new JwtEncoder(new HMACSHA256Algorithm(), new JsonNetSerializer(), new JwtBase64UrlEncoder())
                .Encode(new JObject
                {
                    ["roomID"] = "123"
                }, "key");
            comm.Initialize();
            comm.MsgProcess += new MessageHandler(PrintChatMessage);
            comm.GameStart();
            Console.WriteLine("Game started.");
            Console.ReadLine();
        }
    }
}
