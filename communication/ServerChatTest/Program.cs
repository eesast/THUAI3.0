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
            comm.Port = 8080;
            comm.GameStart();
            Console.WriteLine("Game started.");
            while (true)
            {
                ServerMessage message = comm.MessageQueue.Take();
                (message.Message.Content as ChatMessage).Message += "[From Player #" + message.Client + "]\n";
                message.Client = -1; //broadcast
                message.Agent = -1;
                Console.Write("send");
                comm.SendMessage(message);
            }
        }
    }
}
