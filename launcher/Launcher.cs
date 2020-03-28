using System;
using System.Diagnostics;

namespace StartGame
{
    class Program
    {
        static void Main(string[] args)
        {

			string serverPort = "20000";
			string clientPort = "30000";
			string argumentsServer, argumentsAgent, argumentsClient;
			System.Diagnostics.Process processServer = new System.Diagnostics.Process();
			processServer.StartInfo.FileName = ".\\THUAI3.0\\Logic.Server.exe";
			//传递进exe的参数
			argumentsServer = "-p " + serverPort + " -d 1 -c 2 -a 1 -t 600";
			processServer.StartInfo.Arguments = argumentsServer;
			processServer.Start();
			Console.WriteLine("进程Server");

			System.Diagnostics.Process processAgent = new System.Diagnostics.Process();
			processAgent.StartInfo.FileName = ".\\THUAI3.0\\Communication.Agent.exe";

			argumentsAgent = "--server 127.0.0.1:" + serverPort + " --port " + clientPort + " -t THUAI/offline -d 0";
			processAgent.StartInfo.Arguments = argumentsAgent;
			processAgent.Start();
			Console.WriteLine("进程Agent");

			System.Diagnostics.Process processClient = new System.Diagnostics.Process();
			processClient.StartInfo.FileName = ".\\THUAI3.0\\Logic.Client.exe";

			argumentsClient = "-p " + clientPort + " -d 1 -t 2";
			processClient.StartInfo.Arguments = argumentsClient;
			processClient.Start();
			Console.WriteLine("进程Client");

			processServer.WaitForExit();
			processAgent.WaitForExit();
			processClient.WaitForExit();
			Console.WriteLine("进程全部退出");

        }
    }
}
