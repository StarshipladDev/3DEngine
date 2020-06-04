using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoomCloneV2
{
    class ThreadFunctions
    {
        public static void ThreadRunnerServer()
        {

            Server s1 = new Server("localhost",Globals.port);
            Thread.Sleep(1000);

        }
        public static void ThreadRunner(object c1)
        {
            System.Diagnostics.Debug.WriteLine("Starting Thread");
            FileStream f = File.Create("output.txt");
            Client c = (Client)c1;
            c.Write("Hello From "+c.GetName());
            Byte[] ff = System.Text.Encoding.ASCII.GetBytes(c.Read());
            f.Write(ff, 0, ff.Length);
            f.Close();
            c.CloseClient();
        }
        public static void Listen(object c1)
        {
            Array argArray = new object[2];
            argArray = (Array)c1;
            TcpClient client = (TcpClient)argArray.GetValue(0);
            Server serv = (Server)argArray.GetValue(1);
            NetworkStream nwStream = null;
            while (client != null &&  client.Connected)
            {
                try
                {
                    //---get the incoming data through a network stream---
                    nwStream = client.GetStream();
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    //---read incoming stream---
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                    //---convert the data received into a string---
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Debug.WriteLine("Server Received : " + dataReceived);
                    Server.SendMessage(dataReceived, serv);

                }
                catch(Exception e)
                {
                    Debug.WriteLine("Server Error, aborting");
                    Thread.CurrentThread.Abort();
                }
            }
            if (nwStream != null)
            {
                nwStream.Close();
            }
            client.Close();
            Debug.WriteLine("Server Closing Thread : ");
        }
        public static void ClientThread(object c1)
        {
            Client client = (Client)c1;
            client.Write("PRT Client '"+client.GetName()+"' Connected!");
            client.Write("COC"+client.GetID());
            while (true)
            {
                client.Read();
            }
        }
        public static void Write(object c1)
        {
            Client client = (Client)c1;
            client.Write("Hey!^");
            client.CloseClient();
        }
    }
}
