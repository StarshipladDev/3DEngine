using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace DoomCloneV2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void ThreadRunner(object c1)
        {
            System.Diagnostics.Debug.WriteLine("Starting Thread");
            FileStream f = File.Create("output.txt");
            Client c = (Client)c1;
            c.Write("Hello From^");
            Byte[] ff = System.Text.Encoding.ASCII.GetBytes(c.Read());
            f.Write(ff,0,ff.Length);
            f.Close();
            c.CloseClient();
        }
        public static void ThreadRunnerServer()
        {

            Server s1 = new Server("192.168.200.40", "50001");
            Thread.Sleep(1000);
            
        }
        static void Main()
        {
            //Acctually run the program
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MapGenForm());

            Application.Run(new Form1());

            /*
            Thread thread1 = new Thread(ThreadRunner);
            Thread thread2 = new Thread(ThreadRunnerServer);

            thread2.Start();
            Client c1 = new Client("50001", "localhost", "Client1");
            //Client c2 = new Client("1556", "localhost", "Client2Poo");


            thread1.Start(c1);
            //Thread thread2 = new Thread(ThreadRunner);
            //thread2.Start(c2);
            */


        }
    }
}
