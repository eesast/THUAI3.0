using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;
using System.Runtime.InteropServices;

namespace WindowsFormsApp2
{
    static class Program
    {
        public static Form1 form;
        //输出信息之前,先AllocConsole()   ,用完后 FreeConsole()
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AllocConsole();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);

            Console.WriteLine("\n\nEnd");
            Console.ReadKey();
        }
    }
}
