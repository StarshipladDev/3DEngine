using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    class Server
    {
        Thread[] thread = new Thread[10];
        TcpClient[] clients = new TcpClient[10];
        TcpListener[] listeners = new TcpListener[10];
        int counter = 0;
        bool yeet = true;
        TcpListener listener;
        public Server(String address,String port)
        {
            try
            {
                //---listen at the specified IP and port no.---
                // IPAddress localAdd = IPAddress.Parse(address);
                IPAddress localAdd = IPAddress.Any;
                
                while (yeet)
                {
                    listeners[counter] = new TcpListener(localAdd, Int32.Parse(port));
                    listener = listeners[counter];
                    Debug.WriteLine("Listening on address/port " + address + "/" + port + " ...");
                    listener.Start();
                    Console.WriteLine("Listner"+counter+" Listening successfully on address/port " + address + "/" + port + " ...");
                    //---incoming client connected---
                    clients[counter] = listener.AcceptTcpClient();
                    Debug.WriteLine("Listner"+counter+" accepted TCP client");
                    object[] args = new object[2];
                    args[0] = clients[counter];
                    args[1] = this;
                    thread[counter] = new Thread(ThreadFunctions.Listen);
                    thread[counter].Start(args);
                    Thread.Sleep(500);
                    SendMessage("Welcome Client "+counter+" !^",this);
                    counter++;

                }
                Stop();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("error: "+e.Message);
            }
           
        }
        public static void SendMessage(String s,Server serv)
        {
            //Globals.flags[5] = true;
            //Globals.Message = s;
            int i = 0;
            while (i < serv.counter)
            {
                NetworkStream nws =serv.clients[i].GetStream();
                //---write back the text to the client---
                Debug.WriteLine("Server: Sending back : " + s.Substring(0,s.Length-1));
                Byte[] ba = Encoding.ASCII.GetBytes(s);
                nws.Write(ba, 0, ba.Length);
                i++;
            }
        }
        public void Stop()
        {
            int i = 0;
            while (i < counter)
            {
                thread[i].Abort();
                listeners[i].Stop();
                clients[i].Close();
                i++;
            }
        }
    }
}
