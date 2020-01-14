using Communication.Proto;
using Communication.Server;
using System;

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
            ICommunication comm = new CommunicationImpl();
            comm.Initialize();
            comm.MsgProcess += new MessageHandler(PrintChatMessage);
            Console.Write("Server Listen Port:");
            comm.Port = UInt16.Parse(Console.ReadLine());
            comm.GameStart();
            Console.WriteLine("Game started.");
            Console.ReadLine();
        }
    }
}
