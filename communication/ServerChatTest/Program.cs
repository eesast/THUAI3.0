using Communication.Proto;
using Communication.Server;
using System;

namespace ServerChatTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            ICommunication comm = new CommunicationImpl();
            comm.Initialize();
            Console.Write("Server Listen Port:");
            comm.Port = UInt16.Parse(Console.ReadLine());
            comm.GameStart();
            Console.WriteLine("Game started.");
            while (true)
            {
                ServerMessage message = comm.MessageQueue.Take();
                ChatMessage chat = message.Message as ChatMessage;
                if (chat.Message.StartsWith("/stop")) //测试主动断开连接功能
                {
                    comm.GameOver();
                    break;
                }
                chat.Message = $"[From Player ({message.Agent}, {message.Client})] " + chat.Message + "\n";
                message.Client = -2; //broadcast
                message.Agent = -2; //broadcast
                comm.SendMessage(message);
            }
        }
    }
}
