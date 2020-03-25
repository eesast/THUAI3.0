using System;
using System.Diagnostics;

namespace StartEXE2
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Process process1 = new System.Diagnostics.Process();
            process1.StartInfo.FileName = "./../../../OfflineReg.exe";
            //传递进exe的参数
            //aeguments = ""
            //process.StartInfo.Arguments = argument;
            process1.Start();
            process1.WaitForExit();
            Console.WriteLine("进程全部退出");

        }
    }
}
