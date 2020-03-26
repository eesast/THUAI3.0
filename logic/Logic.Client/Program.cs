using System;
using System.Windows.Forms;
using Client;
using System.Runtime.InteropServices;
using CommandLine;
using Communication.Proto;

namespace GameForm
{
    static class Program
    {
        public static Form1 form;
        //输出信息之前,先AllocConsole()   ,用完后 FreeConsole()
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();


        [STAThread]
        static void Main(string[] args)
        {
            AllocConsole();

            Parser.Default.ParseArguments<AugmentOptions>(args)
                    .WithParsed<AugmentOptions>(o =>
                    {
                        if (!Convert.ToBoolean(o.debugLevel & 1))
                            Player.ClientDebug = new Action<string>(s => { });
                        if (!Convert.ToBoolean(o.debugLevel & 2))
                            Communication.Proto.Constants.Debug = new Constants.DebugFunc((str) => { });
                        if (!Convert.ToBoolean(o.debugLevel & 4))
                        {
                            THUnity2D.GameObject.Debug = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                            THUnity2D.GameObject.DebugWithoutEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                            THUnity2D.GameObject.DebugWithoutID = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                            THUnity2D.GameObject.DebugWithoutIDEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                        }
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        form = new Form1(o.agentPort, (Talent)o.talent);
                        Application.Run(form);
                    });

            Console.WriteLine("\n\nEnd");
            Console.ReadKey();
        }
    }
}
