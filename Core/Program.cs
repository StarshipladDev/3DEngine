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
        static void Main()
        {
            //Acctually run the program
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //As documented in the README, a map generator is shown first so the player can
            //re-roll the layout before starting; closing it proceeds into the game.
            Application.Run(new MapGenForm());

            Application.Run(new Form1());
        }
    }
}
